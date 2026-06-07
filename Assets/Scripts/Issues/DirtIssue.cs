using FixRush;
using System.Collections;
using UnityEngine;

public class DirtIssue : IIssue
{
    // Context
    private Vehicle _vehicle;
    private Trigger _trigger;

    // Class constructor
    public DirtIssue(Vehicle v)
    {
        _vehicle = v;
    }

    // IIssue implementation
    IIssue.Type IIssue.IssueType => IIssue.Type.Dirty;
    public bool IsFixed { get; private set; } = false;
    public int ReparationFee { get; private set; } = 10;

    private bool _passedCheck = false;
    internal int WashCounter = 2;
    private int _maxWashCounter = 2;
    private bool _readyToGo;

    public void CleanUp()
    {
        if (_trigger != null)
        {
            GameObject.Destroy(_trigger.gameObject);
        }

        // UI
        InGameUI.Instance.ShowTaskPanel(false, IIssue.Type.Dirty);
    }

    private void SetUp()
    {
        // Create trigger
        _trigger = GameObject.Instantiate(_vehicle.TriggerPrefab).GetComponent<Trigger>().Set(
            _vehicle.Size.z * 0.7f,
            2f,
            true,
            HandleInteractionStarted,
            HandleInteraction,
            HandleInteractionCancel,
            IInteractable.Type.Hold
        );

        _trigger.transform.SetParent(_vehicle.transform);
        _trigger.transform.localPosition = Vector3.zero;
        _trigger.name = "WashTrigger";
    }

    internal void ResolveDirtIssue()
    {
        _vehicle.NetworkHandler.SyncDirtAlpha(0);
        IsFixed = true;
    }

    // This flow defines the issue's lifecycle
    public IEnumerator FixingCoroutine()
    {
        // Show UI Panel
        InGameUI.Instance.ShowTaskPanel(true, IIssue.Type.Dirty);

        SetUp();
        yield return new WaitUntil(() => _readyToGo);
        yield return new WaitForSeconds(1.5f);
        yield return new WaitUntil(()=> IsFixed);

        // Add Cash
        if (PhotonManager.Instance.IsMasterClient)
        {
            GameManager.Instance.AddCashMaster(ReparationFee);
        }
    }

    internal void Wash()
    {
        WashCounter--;

        float alpha = (float)WashCounter / _maxWashCounter;

        _vehicle.NetworkHandler.SyncSuds(false);
        _vehicle.NetworkHandler.SyncDirtAlpha(alpha);

        if (WashCounter == 0)
        {
            // Fix the issue
            _vehicle.NetworkHandler.RequestResolveDirtIssue(_vehicle);
            _readyToGo = true;
        }
    }

    public void HandleInteraction(PlayerController player, Trigger trigger)
    {
        if (!_passedCheck) { return; }

        _vehicle.NetworkHandler.SyncWashDirtIssue();

        player.GrabbedObj.GetComponent<Bucket>().IsFull = false;
    }

    public void HandleInteractionStarted(PlayerController player, Trigger trigger)
    {
        if (player.GrabbedObj == null)
        {
            InGameUI.Instance.ShowHint("You need to grab a wash tool to clean the car", 2f);
            return;
        }

        if (player.GrabbedObj.tag != "WashTool")
        {
            InGameUI.Instance.ShowHint("This tool can't be used to clean the car", 2f);
            return;
        }

        if (!player.GrabbedObj.GetComponent<Bucket>().IsFull)
        {
            InGameUI.Instance.ShowHint("The bucket is empty, find a water source to refill it", 2f);
            return;
        }

        _passedCheck = true;

        _vehicle.NetworkHandler.SyncSuds(true, trigger.HoldTime);
        InGameUI.Instance.StartTaskProgress(trigger.HoldTime);
    }

    public void HandleInteractionCancel(PlayerController player, Trigger trigger)
    {
        _vehicle.NetworkHandler.SyncSuds(false);
        InGameUI.Instance.StopTaskProgress(false);
    }
}