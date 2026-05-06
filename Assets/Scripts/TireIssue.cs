using FixRush;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireIssue : IIssue
{
    private Car _car;
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

    public void CleanUp()
    {
        _triggers.Clear();

        // Hide UI Panel
        InGameUI.Instance.ShowTaskPanel(false, IIssue.Type.Tires);
    }

    private void SetUp()
    {
        _counter = _car.TiresFront.Length + _car.TiresRear.Length;
        Debug.Log($"[TireIssue] Counter set to {_counter}");

        foreach (GameObject tire in _car.TiresFront)
        {
            CreateRemoveTireTrigger(tire.transform);
        }

        foreach (GameObject tire in _car.TiresRear)
        {
            CreateRemoveTireTrigger(tire.transform);
        }
    }

    private void CreateRemoveTireTrigger(Transform parent)
    {
        // Create trigger
        _tempTrigger = GameObject.Instantiate(_car.TriggerPrefab).GetComponent<Trigger>().Set(
            1f,
            1f,
            HandleTireInteraction,
            IInteractable.Type.Simple
        );

        _tempTrigger.transform.SetParent(parent);
        _tempTrigger.transform.localPosition = Vector3.zero;
        _tempTrigger.gameObject.name = "RemoveTireTrigger";

        _triggers.Add(_tempTrigger, parent.gameObject);
    }

    public IEnumerator FixingCoroutine()
    {
        // Show UI Panel
        InGameUI.Instance.ShowTaskPanel(true, IIssue.Type.Tires);

        yield return new WaitUntil(() => IsFixed);
    }

    public void HandleTireInteraction(PlayerController player, Trigger trigger)
    {
        if (trigger.name == "RemoveTireTrigger")
        {
            _counter--;

            // Deactivate tire
            _triggers.TryGetValue(trigger, out GameObject tire);
            if (tire != null)
            {
                tire.SetActive(false);
            }

            _triggers.Remove(trigger);
            GameObject.Destroy(trigger.gameObject);

            if (_counter <= 0)
            {
                IsFixed = true;
            }
        }
    }
}
