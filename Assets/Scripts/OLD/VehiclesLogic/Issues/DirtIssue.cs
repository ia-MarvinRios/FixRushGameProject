using FixRushGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FixRushGame
{
    public class DirtIssue : FixRushGame.IIssue
    {
        public FixRushGame.IssueType Type => FixRushGame.IssueType.Dirty;

        public bool IsFixed { get; private set; }

        FixRushGame.Vehicle _v;
        List<GameObject> _objects;

        public DirtIssue(FixRushGame.Vehicle vehicle)
        {
            _v = vehicle;
            _objects = new List<GameObject>();

            SetUpVehicle();

            FixRushGame.Interactable.OnInteract += HandleInteraction;
        }

        public void CleanUp()
        {
            _v = null;
            _objects.Clear();

            FixRushGame.Interactable.OnInteract -= HandleInteraction;
        }

        public IEnumerator FixingCoroutine()
        {
            Debug.Log("COROUTINE WAS LOADED SUCCESSFULY!");
            yield return new WaitUntil(() => IsFixed);
        }

        void SetUpVehicle()
        {
            // Setup dirt issue related objects on the vehicle
            GameObject o = new GameObject("DirtFixTrigger");
            o.transform.SetParent(_v.transform);
            o.transform.localPosition = new Vector3(0, 0, 0);

            o.AddComponent<FixRushGame.Interactable>().SetInteractable(FixRushGame.InteractionType.Simple, 0f, _v.Collider.size.z / 2);

            _objects.Add(o);
        }

        public void HandleInteraction(FixRushGame.Interactable obj, FixRushGame.AuxPlayer entity)
        {
            if (!obj.transform.IsChildOf(_v.transform))
                return;

            if (!_objects.Contains(obj.gameObject))
                return;

            Object.Destroy(obj.gameObject);
            _objects.Remove(obj.gameObject);

            IsFixed = true;
            CleanUp();
            Debug.Log("Dirt issue fixed!");
        }
    }
}
