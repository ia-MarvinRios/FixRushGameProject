using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TireIssue : IIssue
{
    private enum Phase
    {
        Gato,
        ChangeTires,
    }

    public IssueType Type => IssueType.Tires;
    public bool IsFixed { get; private set; }

    Car _car;
    List<GameObject> _objects;
    Phase _currentPhase = Phase.Gato;

    private bool _tiresAlreadySet = false;
    private bool _allTiresFixed
    {
        get
        {
            foreach (GameObject obj in _objects)
            {
                if (obj.name == "WheelFixTrigger" || obj.name == "EmptyWheelTrigger")
                    return false;
            }
            return true;
        }
    }

    public TireIssue(Car car)
    {
        _car = car;
        _objects = new List<GameObject>();
    }

    public void CleanUp()
    {
        _car = null;
        CleanUpObjects();
    }
    void CleanUpObjects()
    {
        foreach (GameObject obj in _objects)
        {
            Object.Destroy(obj);
        }

        _objects.Clear();
    }

    public IEnumerator FixingCoroutine()
    {
        Debug.Log("COROUTINE WAS LOADED SUCCESSFULY!");
        SetUpCar();
        yield return new WaitUntil(() => IsFixed);
    }

    public void SetUpCar()
    {
        // Instantiate and set up position and parent
        GameObject o = new GameObject("CarTriggerFront");
        o.transform.SetParent(_car.transform);
        o.transform.localPosition = new Vector3(0, 0, _car.Collider.size.z / 3);

        GameObject u = new GameObject("CarTriggerBack");
        u.transform.SetParent(_car.transform);
        u.transform.localPosition = new Vector3(0, 0, -_car.Collider.size.z / 3);

        // Add necesary components
        o.AddComponent<Interactable>().SetInteractable(InteractionType.Simple, 0f, _car.Collider.size.z / 3);
        u.AddComponent<Interactable>().SetInteractable(InteractionType.Simple, 0f, _car.Collider.size.z / 3);

        // Add object to the list
        _objects.Add(o);
        _objects.Add(u);
    }
    
    internal void SetUpTires(List<int> wID)
    {
        _currentPhase = Phase.ChangeTires;
        if (_tiresAlreadySet) return;

        foreach (WheelRoot root in _car.WheelRoots)
        {
            // Instantiate and set up position and parent
            GameObject c = new GameObject("WheelFixTrigger");
            c.transform.SetParent(_car.transform);
            c.transform.localPosition = root.Position;

            // Add necesary components
            c.AddComponent<Interactable>().SetInteractable(InteractionType.Still, 2.5f, 0.7f);
            c.AddComponent<Wheel>().SetUp(wID[^1], root.LinkedGatoRootID, root.Position, false);

            wID.Remove(wID[^1]);

            // Add object to the list
            _objects.Add(c);

            _tiresAlreadySet = true;
        }
    }

    internal void SetUpEmptyWheelRoot(List<int> wID, int gatoID, Vector3 pos)
    {
        WheelRoot root = new WheelRoot
        {
            LinkedGatoRootID = gatoID,
            Position = pos
        };

        // Instantiate and set up position and parent
        GameObject c = new GameObject("EmptyWheelTrigger");
        c.transform.SetParent(_car.transform);
        c.transform.localPosition = root.Position;

        // Add necesary components
        c.AddComponent<Interactable>().SetInteractable(InteractionType.Simple, 1f, 0.7f);
        c.AddComponent<EmptyWheelRoot>().SetUp(wID[^1], root.LinkedGatoRootID, root.Position);

        wID.Remove(wID[^1]);

        // Add object to the list
        _objects.Add(c);
    }

    void TireInteraction(Interactable obj, AuxPlayer entity)
    {
        switch (obj.name)
        {
            // If wanted to remove gato
            case "CarTriggerFront":
                RemoveGato(obj, entity);
                break;
            case "CarTriggerBack":
                RemoveGato(obj, entity);
                break;

            // Remove Tire Logic
            case "WheelFixTrigger":

                if (!obj.TryGetComponent(out Wheel wheel))
                    return;

                if (wheel.GatoID < 0 || wheel.GatoID + 1 > _car.GatoRoots.Length) { Debug.Log("This wheel is not associated with an active Gato Root."); return; }

                if (_car.GatoRoots[wheel.GatoID].Object != null)
                {
                    if (entity.GrabbedObj == null) return;
                    if (!entity.GrabbedObj.CompareTag("Wrench"))
                    {
                        Debug.Log("You need a wrench to remove the wheel.");
                        return;
                    }

                    entity.GrabbedObj.GetComponent<Pickable>().Drop();

                    GameObject item = _car.CarNetwork.SpawnObject(
                        "BadWheel",
                        wheel.transform.position,
                        Quaternion.identity
                    );

                    Debug.Log(item.GetComponent<Pickable>() != null);
                    item.GetComponent<Pickable>()
                        .RequestPickUpObj(entity);

                    _car.CarNetwork.RequestRemoveOldWheel(_car, wheel.WheelID);

                    _car.CarNetwork.RequestSetUpEmptyWheelRoot(
                        wheel.GatoID, 
                        obj.transform.localPosition, 
                        _car
                    );
                }

                break;

            // Logic for fixing the tire
            case "EmptyWheelTrigger":

                if (entity.GrabbedObj == null || !_car.IsGatoed) return;
                if (!entity.GrabbedObj.CompareTag("NewWheel")) return;

                _car.CarNetwork.DestroyObject(entity.GrabbedObj); // This is just for now

                Debug.Log("Trying to fix tire...");

                _objects.Remove(obj.gameObject);
                Object.Destroy(obj.gameObject);

                if (_allTiresFixed)
                {
                    ResolveTireIssue();
                }

                break;
        }
    }

    internal void RemoveWheel(int wheelID)
    {
        Wheel wheel = _objects.Find(obj => obj.name == "WheelFixTrigger" && obj.GetComponent<Wheel>().WheelID == wheelID)?.GetComponent<Wheel>();

        _objects.Remove(wheel.gameObject);
        Object.Destroy(wheel.gameObject);
    }
    internal void RemoveGato(Interactable obj, AuxPlayer entity)
    {
        _car.CarNetwork.RequestRemoveGato(_car.GetNearestGatoRootIndex(obj.transform.position), _car, entity);
    }
    internal void ResolveTireIssue()
    {
        IsFixed = true;
        Debug.Log("All tires fixed!");
    }

    public void HandleInteraction(Interactable obj, AuxPlayer entity)
    {
        if (!obj.transform.IsChildOf(_car.transform))
            return;

        if (!_objects.Contains(obj.gameObject))
            return;

        GatoChecker();

        switch (_currentPhase)
        {
            case Phase.Gato:
                _car.CarNetwork.RequestPlaceGato(entity, _car.GetNearestGatoRootIndex(obj.transform.position), _car);
                _car.CarNetwork.RequestSetUpTires(_car);
                break;

            case Phase.ChangeTires:
                TireInteraction(obj, entity);
                break;
        }
    }

    void GatoChecker()
    {
        if (!_car.IsGatoed)
        {
            _currentPhase = Phase.Gato;
            return;
        }

        else return;
    }
}
