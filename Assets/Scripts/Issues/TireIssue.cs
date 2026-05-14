using FixRush;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireIssue : IIssue
{
    private Car _car;
    private Trigger _jackTrigger;
    private Trigger _tempTrigger;
    private Dictionary<Trigger, GameObject> _triggers = new Dictionary<Trigger, GameObject>();

    private int _counter = 0;

    public TireIssue(Car car)
    {
        _car = car;

        SetUp();
    }

    public IIssue.Type IssueType => IIssue.Type.Tires;

    public bool IsFixed { get; private set; } = false;

    private bool _passedCheck = false;

    public void CleanUp()
    {
        _triggers.Clear();

        // Hide UI Panel
        InGameUI.Instance.ShowTaskPanel(false, IIssue.Type.Tires);
    }

    private void SetUp()
    {
        // --- Create jack trigger ---
        _jackTrigger = GameObject.Instantiate(_car.TriggerPrefab).GetComponent<Trigger>().Set(
            _car.Size.z,
            5f,
            true,
            JackInteractionStarted,
            JackInteraction,
            JackInteractionCancel,
            IInteractable.Type.Hold
        );

        _jackTrigger.transform.SetParent(_car.transform);
        _jackTrigger.transform.localPosition = Vector3.zero;
        _jackTrigger.name = "JackTrigger";

        // --- Create remove tire triggers ---
        _counter = _car.TiresFront.Length + _car.TiresRear.Length;
        Debug.Log($"[TireIssue] Counter set to {_counter}");

        foreach (GameObject tire in _car.TiresFront)
        {
            CreateRemoveTireTrigger(tire.transform, true);
        }

        foreach (GameObject tire in _car.TiresRear)
        {
            CreateRemoveTireTrigger(tire.transform, true);
        }
    }

    private void CreateRemoveTireTrigger(Transform parent, bool disableOnCreation)
    {
        // Create trigger
        _tempTrigger = GameObject.Instantiate(_car.TriggerPrefab).GetComponent<Trigger>().Set(
            1f,
            5f,
            true,
            HandleTireInteractionStarted,
            HandleTireInteraction,
            HandleTireInteractionCanceled,
            IInteractable.Type.Hold
        );

        _tempTrigger.transform.SetParent(parent);
        _tempTrigger.transform.localPosition = Vector3.zero;
        _tempTrigger.gameObject.name = "RemoveTireTrigger";

        // Add to dictionary
        _triggers.Add(_tempTrigger, parent.gameObject);

        // Deactivation
        _tempTrigger.gameObject.SetActive(!disableOnCreation);
    }

    public IEnumerator FixingCoroutine()
    {
        // Show UI Panel
        InGameUI.Instance.ShowTaskPanel(true, IIssue.Type.Tires);

        yield return new WaitUntil(() => IsFixed);
    }

    #region JACK_INTERACTIONS

    private void JackInteractionStarted(PlayerController player, Trigger trigger)
    {
        if (player.GrabbedObj == null)
        {
            InGameUI.Instance.ShowHint("You need to grab a Jack tool first", 2f);
            return;
        }

        if (player.GrabbedObj.tag != "Jack")
        {
            InGameUI.Instance.ShowHint("This tool can't be used to jack the car", 2f);
            return;
        }

        _passedCheck = true;

        // Audio and UI
        AudioManager.Instance.PlaySoundByName("Jack");
        InGameUI.Instance.StartTaskProgress(trigger.HoldTime);
    }
    private void JackInteraction(PlayerController player, Trigger trigger)
    {
        if (!_passedCheck) { return; }

        // logic
        if (player.GrabbedObj.TryGetComponent(out Jack jack))
        {
            jack.Drop(player);
            
            // If it's not jacked, jack
            if (!_car.IsJacked)
            {
                _car.NetworkHandler.SetActiveChildren(_car.gameObject, _jackTrigger.name, false);
                foreach (var t in _triggers)
                {
                    _car.NetworkHandler.SetActiveChildren(t.Value.gameObject, true);
                }
            }
        }

        if (IsFixed)
        {
            _car.NetworkHandler.SetActiveChildren(_car.gameObject, _jackTrigger.name, false);
        }

        _car.IsJacked = !_car.IsJacked;

        _passedCheck = false;
    }
    private void JackInteractionCancel(PlayerController player, Trigger trigger)
    {
        _passedCheck = false;

        // Audio and UI
        AudioManager.Instance.StopAllFX();
        InGameUI.Instance.StopTaskProgress(false);
    }

    #endregion

    #region TIRES_INTERACTION

    private void HandleTireInteractionStarted(PlayerController player, Trigger trigger)
    {
        if (player.GrabbedObj == null)
        {
            InGameUI.Instance.ShowHint("You need to grab a CrossWrench tool first", 2f);
            return;
        }

        if (player.GrabbedObj.tag != "CrossWrench")
        {
            InGameUI.Instance.ShowHint("This tool can't be used to remove the tires", 2f);
            return;
        }

        _passedCheck = true;

        // Audio and UI
        AudioManager.Instance.PlaySoundByName("Jack");
        InGameUI.Instance.StartTaskProgress(trigger.HoldTime);
    }

    public void HandleTireInteraction(PlayerController player, Trigger trigger)
    {
        if (!_passedCheck) { return; }

        _counter--;

        // Deactivate tire
        _triggers.TryGetValue(trigger, out GameObject tire);
        if (tire != null)
        {
            _car.NetworkHandler.SetActiveObject(tire, false);
        }

        _triggers.Remove(trigger);
        GameObject.Destroy(trigger.gameObject);

        _passedCheck = false;

        if (_counter <= 0)
        {
            // Reactivate jack trigger
            _car.NetworkHandler.SetActiveChildren(_car.gameObject, _jackTrigger.name, true);

            IsFixed = true;
        }
    }

    private void HandleTireInteractionCanceled(PlayerController player, Trigger trigger)
    {
        _passedCheck = false;

        // Audio and UI
        AudioManager.Instance.StopAllFX();
        InGameUI.Instance.StopTaskProgress(false);
    }

    #endregion
}
