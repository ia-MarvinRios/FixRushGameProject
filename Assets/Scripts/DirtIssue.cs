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

        SetUp();
    }

    // IIssue implementation
    IIssue.Type IIssue.IssueType => IIssue.Type.Dirty;
    public bool IsFixed { get; private set; } = false;

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
            _vehicle.Size.z, 
            1f, 
            HandleInteraction,
            IInteractable.Type.Simple
        );

        _trigger.transform.SetParent(_vehicle.transform);
        _trigger.transform.localPosition = Vector3.zero;
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

        yield return new WaitUntil(()=> IsFixed);
    }

    public void HandleInteraction(PlayerController player, Trigger trigger)
    {
        Debug.Log("<color=#FF69B4> SAQUENME DE LA CARRERA YA NO AGUANTO PROGRAMAR TANTA VAINA!! </color>");

        _vehicle.NetworkHandler.SyncDirtAlpha(0);

        _vehicle.NetworkHandler.RequestResolveDirtIssue(_vehicle);

        IsFixed = true;
    }
}