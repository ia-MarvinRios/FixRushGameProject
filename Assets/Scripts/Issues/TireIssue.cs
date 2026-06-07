using FixRush;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireIssue : IIssue
{
    private Car _car;
    private Trigger _jackTrigger;
    private Trigger _tempTrigger;
    private Dictionary<Trigger, GameObject> _triggers = new Dictionary<Trigger, GameObject>();

    public TireIssue(Car car)
    {
        _car = car;

        SetUp();
    }

    public IIssue.Type IssueType => IIssue.Type.Tires;

    internal bool SafeDestruction = false;
    public bool IsFixed { get; private set; } = false;
    public int ReparationFee { get; private set; } = 160;
    private bool _passedCheck = false;
    
    public void CleanUp()
    {
        foreach (Trigger trigger in _triggers.Keys)
        {
            if (trigger != null)
            {
                GameObject.Destroy(trigger.gameObject);
            }
        }

        if (_jackTrigger != null) GameObject.Destroy(_jackTrigger.gameObject);
        if (_tempTrigger != null) GameObject.Destroy(_tempTrigger.gameObject);

        _triggers.Clear();
        _tempTrigger = null;
        _jackTrigger = null;

        // Hide UI Panel
        InGameUI.Instance.ShowTaskPanel(false, IIssue.Type.Tires);
    }

    public IEnumerator FixingCoroutine()
    {
        // Show UI Panel
        InGameUI.Instance.ShowTaskPanel(true, IIssue.Type.Tires);

        yield return new WaitUntil(() => IsFixed);
        yield return new WaitUntil(() => !_car.IsJacked);
        yield return new WaitForSeconds(1.5f);

        // Add cash
        if (PhotonManager.Instance.IsMasterClient)
        {
            GameManager.Instance.AddCashMaster(ReparationFee);
        }
    }

    private void SetUp()
    {
        // --- Create jack trigger ---
        _jackTrigger = GameObject.Instantiate(_car.TriggerPrefab).GetComponent<Trigger>().Set(
            _car.Size.z * 0.7f,
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

        foreach (GameObject tire in _car.TiresFront)
        {
            CreateRemoveTireTrigger(tire.transform, true);
        }

        foreach (GameObject tire in _car.TiresRear)
        {
            CreateRemoveTireTrigger(tire.transform, true);
        }
    }

    private void TriggerDestructionHandler(Trigger trigger) 
    {
        Debug.Log("Safe: " + SafeDestruction);
        if (!SafeDestruction) { return; }

        SafeDestruction = false;

        // Create replace trigger if is the case
        if (trigger.name.Contains("RemoveTireTrigger"))
        {
            CreateReplaceTireTrigger(_car.transform, _triggers[trigger].transform);
        }

        // --- Destroy and update dictionary ---
        _triggers.Remove(trigger);

        if (_triggers.Count <= 0)
        {
            // Reactivate jack trigger
            _car.NetworkHandler.SetActiveChildren(_car.gameObject, _jackTrigger.name, true);

            Debug.Log("[TireIssue] All tires completely fixed, fixing issue...");

            IsFixed = true;
        }

        Debug.Log("[TireIssue] Triggers Count: " + _triggers.Count);
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

        _tempTrigger.OnDestroyTrigger(TriggerDestructionHandler);

        _tempTrigger.transform.SetParent(parent);
        _tempTrigger.transform.localPosition = Vector3.zero;
        _tempTrigger.gameObject.name = $"RemoveTireTrigger({parent.name})";

        // Add to dictionary
        _triggers.Add(_tempTrigger, parent.gameObject);

        // Deactivation
        _tempTrigger.gameObject.SetActive(!disableOnCreation);
    }

    private void CreateReplaceTireTrigger(Transform parent, Transform tire)
    {
        // Create trigger
        _tempTrigger = GameObject.Instantiate(_car.TriggerPrefab).GetComponent<Trigger>().Set(
            1f,
            0f,
            true,
            HandleReplaceTireInteractionStarted,
            HandleReplaceTireInteraction,
            HandleReplaceTireInteractionCanceled,
            IInteractable.Type.Simple
        );

        _tempTrigger.OnDestroyTrigger(TriggerDestructionHandler);

        _tempTrigger.transform.SetParent(parent);
        _tempTrigger.transform.position = tire.position;
        _tempTrigger.gameObject.name = $"ReplaceTireTrigger({tire.name})";

        // Add to dictionary
        _triggers.Add(_tempTrigger, tire.gameObject);
    }

    private void SetActiveTireTriggers(int zPos, bool active)
    {
        if (zPos == 0) { return; }

        // Front
        if (zPos > 0)
        {
            foreach (GameObject tire in _car.TiresFront)
            {
                _car.NetworkHandler.SetActiveChildren(tire, active);
            }
        }
        // Rear
        else
        {
            foreach (GameObject tire in _car.TiresRear)
            {
                _car.NetworkHandler.SetActiveChildren(tire, active);
            }
        }
    }

    private void SetActiveTireTriggers(bool active)
    {
        // Front
        foreach (GameObject tire in _car.TiresFront)
        {
            _car.NetworkHandler.SetActiveChildren(tire, active);
        }
        // Rear
        foreach (GameObject tire in _car.TiresRear)
        {
            _car.NetworkHandler.SetActiveChildren(tire, active);
        }
    }

    #region JACK_INTERACTIONS

    private void JackInteractionStarted(PlayerController player, Trigger trigger)
    {
        if (player.GrabbedObj == null && !_car.IsJacked)
        {
            InGameUI.Instance.ShowHint("You need to grab a Jack tool first", 2f);
            return;
        }

        if (player.GrabbedObj == null && _car.IsJacked)
        {
            _passedCheck = true;

            // Audio and UI
            AudioManager.Instance.PlaySoundByName("Jack");
            InGameUI.    Instance.StartTaskProgress(trigger.HoldTime);

            return;
        }

        if (player.GrabbedObj.tag != "Jack")
        {
            InGameUI.Instance.ShowHint("This tool can't be used to jack the car", 2f);
            return;
        }

        if (player.GrabbedObj != null)
        {
            if (player.GrabbedObj.tag == "Jack" && _car.mJack != null)
            {
                InGameUI.Instance.ShowHint("The car is already jacked!", 2f);
                return;
            }
        }

        _passedCheck = true;

        // Audio and UI
        AudioManager.Instance.PlaySoundByName("Jack");
        InGameUI.    Instance.StartTaskProgress(trigger.HoldTime);
    }
    private void JackInteraction(PlayerController player, Trigger trigger)
    {
        if (!_passedCheck) { return; }

        // --- If it's jacked ---
        if (_car.IsJacked)
        {
            if (_car.mJack == null)
            {
                InGameUI.Instance.ShowHint("The car is unjacked already!", 2f);
                return;
            }
            _car.NetworkHandler.RequestJackUnjackCar(player, _car.mJack, false);
            SetActiveTireTriggers(false);
        }

        // --- If it's not jacked ---
        else if (player.GrabbedObj != null && !_car.IsJacked)
        {
            if (!player.GrabbedObj.TryGetComponent(out Jack jack)) { return; }

            int z = _car.GetPlayerReferencePos(player);
            _car.NetworkHandler.RequestJackUnjackCar(player, jack, true);

            SetActiveTireTriggers(z, true);

            player.GrabbedObj = null;
        }

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

        // Deactivate tire
        _triggers.TryGetValue(trigger, out GameObject tire);
        if (tire != null)
        {
            _car.NetworkHandler.SetActiveObject(tire, false);
        }

        // Destroy it's trigger
        _car.NetworkHandler.SetSafeDestruction(true);
        _car.NetworkHandler.DestroyChildren(tire, trigger.name);

        _passedCheck = false;
    }

    private void HandleTireInteractionCanceled(PlayerController player, Trigger trigger)
    {
        _passedCheck = false;

        // Audio and UI
        AudioManager.Instance.StopAllFX();
        InGameUI.Instance.StopTaskProgress(false);
    }

    #endregion

    #region REPLACE_TIRES

    private void HandleReplaceTireInteractionStarted(PlayerController player, Trigger trigger)
    {
        if (player.GrabbedObj == null)
        {
            InGameUI.Instance.ShowHint("You need to grab a new wheel first", 2f);
            return;
        }

        if (player.GrabbedObj.tag != "NewWheel")
        {
            InGameUI.Instance.ShowHint("This is not a NEW wheel", 2f);
            return;
        }

        _passedCheck = true;

        // Audio and UI
        AudioManager.Instance.PlaySoundByName("PlaceObject");
        InGameUI.Instance.StartTaskProgress(trigger.HoldTime);

    }

    private void HandleReplaceTireInteraction(PlayerController player, Trigger trigger)
    {
        if (!_passedCheck) { return; }

        // Activate tire
        _triggers.TryGetValue(trigger, out GameObject tire);
        if (tire != null)
        {
            _car.NetworkHandler.SetActiveObject(tire, true);
        }

        // Destroy it's trigger
        _car.NetworkHandler.SetSafeDestruction(true);
        _car.NetworkHandler.DestroyChildren(_car.gameObject, trigger.name);

        // Drop and destroy new wheel on player's hand
        GameObject obj = player.GrabbedObj;
        player.DropObject(obj);
        _car.NetworkHandler.DestroyNetworkObjMaster(obj);
    }

    private void HandleReplaceTireInteractionCanceled(PlayerController player, Trigger trigger)
    {
        _passedCheck = false;
    }

    #endregion
}
