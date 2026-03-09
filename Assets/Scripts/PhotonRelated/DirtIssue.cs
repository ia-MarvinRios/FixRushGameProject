using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtIssue : IIssue
{
    public IssueType Type => IssueType.Dirty;

    public bool IsFixed { get; private set; }

    Vehicle _v;
    List<GameObject> _objects;

    public DirtIssue(Vehicle vehicle)
    {
        _v = vehicle;
        _objects = new List<GameObject>();
    }

    public void CleanUp()
    {
        _v = null;
        _objects.Clear();
    }

    public IEnumerator FixingCoroutine()
    {
        Debug.Log("COROUTINE WAS LOADED SUCCESSFULY!");
        SetUpVehicle();
        yield return new WaitUntil(() => IsFixed);
    }

    public void SetUpVehicle()
    {
        // Setup dirt issue related objects on the vehicle
        GameObject o = new GameObject("DirtFixTrigger");
        o.transform.SetParent(_v.transform);
        o.transform.localPosition = new Vector3(0, 0, 0);

        o.AddComponent<Interactable>().SetInteractable(InteractionType.Simple, 0f, _v.Collider.size.z / 2);

        _objects.Add(o);
    }

    public void HandleInteraction(Interactable obj, AuxPlayer entity)
    {
        if (!obj.transform.IsChildOf(_v.transform))
            return;

        if (!_objects.Contains(obj.gameObject))
            return;

        _v.VehicleNetwork.RequestResolveDirtIssue(_v);
    }

    internal void ResolveDirtIssue()
    {
        Interactable interactable = _v.GetComponentInChildren<Interactable>();
        Object.Destroy(interactable.gameObject);
        _objects.Remove(interactable.gameObject);

        IsFixed = true;
        CleanUp();
        Debug.Log("Dirt issue fixed!");
    }
}
