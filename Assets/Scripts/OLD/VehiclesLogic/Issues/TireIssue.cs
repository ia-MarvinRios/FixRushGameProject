using FixRushGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FixRushGame
{
    [System.Serializable]
    public class TireIssue : FixRushGame.IIssue
    {
        public FixRushGame.IssueType Type => FixRushGame.IssueType.Tires;
        public bool IsFixed { get; private set; }

        private enum Phase
        {
            Gato,
            ChangeTires,
        }
        Phase _currentPhase = Phase.Gato;

        FixRushGame.Car _car;
        List<GameObject> _objects;
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

        // Constructor
        public TireIssue(FixRushGame.Car car)
        {
            _car = car;
            _objects = new List<GameObject>();
            SetUpCar();

            FixRushGame.Interactable.OnInteract += HandleInteraction;
        }

        /// <summary>
        /// Releases resources and detaches event handlers associated with the current instance.
        /// </summary>
        /// <remarks>Call this method to reset the internal state and prevent memory leaks by removing event
        /// subscriptions. After calling this method, the instance should not be used until reinitialized.</remarks>
        public void CleanUp()
        {
            _car = null;
            CleanUpObjects();

            FixRushGame.Interactable.OnInteract -= HandleInteraction;
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
            yield return new WaitUntil(() => IsFixed);
        }

        void SetUpCar()
        {
            // Instantiate and set up position and parent
            GameObject o = new GameObject("CarTriggerFront");
            o.transform.SetParent(_car.transform);
            o.transform.localPosition = new Vector3(0, 0, _car.Collider.size.z / 3);

            GameObject u = new GameObject("CarTriggerBack");
            u.transform.SetParent(_car.transform);
            u.transform.localPosition = new Vector3(0, 0, -_car.Collider.size.z / 3);

            // Add necesary components
            o.AddComponent<FixRushGame.Interactable>().SetInteractable(FixRushGame.InteractionType.Simple, 0f, _car.Collider.size.z / 3);
            u.AddComponent<FixRushGame.Interactable>().SetInteractable(FixRushGame.InteractionType.Simple, 0f, _car.Collider.size.z / 3);

            // Add object to the list
            _objects.Add(o);
            _objects.Add(u);
        }

        void SetUpTires()
        {
            if (_tiresAlreadySet) return;

            foreach (FixRushGame.WheelRoot root in _car.WheelRoots)
            {
                // Instantiate and set up position and parent
                GameObject c = new GameObject("WheelFixTrigger");
                c.transform.SetParent(_car.transform);
                c.transform.localPosition = root.Position;

                // Add necesary components
                c.AddComponent<FixRushGame.Interactable>().SetInteractable(FixRushGame.InteractionType.Still, 2.5f, 0.7f);
                c.AddComponent<Wheel>().SetUp(0, root.LinkedGatoRootID, root.Position, false);

                // Add object to the list
                _objects.Add(c);

                _tiresAlreadySet = true;
            }
        }

        void SetUpEmptyWheelRoot(FixRushGame.WheelRoot root)
        {
            // Instantiate and set up position and parent
            GameObject c = new GameObject("EmptyWheelTrigger");
            c.transform.SetParent(_car.transform);
            c.transform.localPosition = root.Position;

            // Add necesary components
            c.AddComponent<FixRushGame.Interactable>().SetInteractable(FixRushGame.InteractionType.Simple, 1f, 0.7f);
            c.AddComponent<EmptyWheelRoot>().SetUp(0, root.LinkedGatoRootID, root.Position);

            // Add object to the list
            _objects.Add(c);
        }

        void TireInteraction(FixRushGame.Interactable obj, FixRushGame.AuxPlayer entity)
        {
            switch (obj.name)
            {
                // Remove Tire Logic
                case "WheelFixTrigger":

                    if (!obj.TryGetComponent(out Wheel wheel))
                        return;

                    if (wheel.GatoID < 0 || wheel.GatoID + 1 > _car.GatoRoots.Length) { Debug.Log("This wheel is not associated with an active Gato Root."); return; }

                    if (_car.GatoRoots[wheel.GatoID].Object != null)
                    {
                        if (entity.GrabbedObj == null) return;
                        if (!entity.GrabbedObj.CompareTag("Wrench")) return;

                        entity.GrabbedObj.GetComponent<Pickable>().Drop();

                        GameObject item = Object.Instantiate(
                                GlobalItems.Instance.Items[0],
                                wheel.transform.position,
                                Quaternion.identity
                            );

                        item.GetComponent<Pickable>()
                            .PickUp(item.GetComponent<FixRushGame.Interactable>(), entity);

                        _objects.Remove(obj.gameObject);
                        Object.Destroy(obj.gameObject);

                        SetUpEmptyWheelRoot(new FixRushGame.WheelRoot { LinkedGatoRootID = wheel.GatoID, Position = obj.transform.localPosition });
                    }

                    break;

                // Logic for fixing the tire
                case "EmptyWheelTrigger":

                    if (entity.GrabbedObj == null) return;
                    if (!entity.GrabbedObj.CompareTag("NewWheel")) return;

                    Object.Destroy(entity.GrabbedObj); // This is just for now

                    Debug.Log("Trying to fix tire...");

                    _objects.Remove(obj.gameObject);
                    Object.Destroy(obj.gameObject);

                    if (_allTiresFixed)
                    {
                        IsFixed = true;
                        CleanUp();
                        Debug.Log("All tires fixed!");
                    }

                    break;
            }
        }

        public void HandleInteraction(FixRushGame.Interactable obj, FixRushGame.AuxPlayer entity)
        {
            if (!obj.transform.IsChildOf(_car.transform))
                return;

            if (!_objects.Contains(obj.gameObject))
                return;

            GatoChecker();

            switch (_currentPhase)
            {
                case Phase.Gato:
                    _car.PlaceGato(entity, _car.GetNearestGatoRootIndex(obj.transform.position));
                    SetUpTires();

                    _currentPhase = Phase.ChangeTires;
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
}
