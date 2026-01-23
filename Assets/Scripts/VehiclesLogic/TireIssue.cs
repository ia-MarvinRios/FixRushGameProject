using FixRushGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TireIssue : IIssue
{
    public IssueType Type => IssueType.Tires;
    public bool IsFixed { get; private set; }

    Car _car;
    List<GameObject> _objects;

    public TireIssue(Car car)
    {
        _car = car;
        _objects = new List<GameObject>();
        SetUpTires();

        Interactable.OnInteract += HandleInteraction;
    }

    public void CleanUp()
    {
        _car = null;
        _objects.Clear();

        Interactable.OnInteract -= HandleInteraction;
    }

    public IEnumerator FixingCoroutine()
    {
        Debug.Log("COROUTINE WAS LOADED SUCCESSFULY!");
        yield return new WaitForSeconds(1);
    }

    void SetUpTires()
    {
        foreach (var pos in _car.WheelRoots)
        {
            GameObject c = new GameObject("WheelFixTrigger");
            c.transform.SetParent(_car.transform);
            c.transform.localPosition = pos;

            c.AddComponent<Interactable>().SetInteractable(InteractionType.Simple, 0, 0.7f);
            _objects.Add(c);
        }
    }

    void HandleInteraction(GameObject obj, GameObject entity)
    {
        Debug.Log("Reparando...");
    }
}
