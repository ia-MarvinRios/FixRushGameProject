using FixRush;
using System.Collections;
using UnityEngine;

public class Car : Vehicle
{
    [Header("Car References")]
    [SerializeField] private Transform _jackRootFront;
    [SerializeField] private GameObject[] _tiresFront;
    [SerializeField] private Transform _jackRootRear;
    [SerializeField] private GameObject[] _tiresRear;

    internal bool IsJacked = false;

    public override void InitializeVehicle()
    {
        StartCoroutine(FixCarCoroutine());
    }

    private IEnumerator FixCarCoroutine()
    {
        foreach (var i in IssueTypes)
        {
            switch (i)
            {
                default:
                    yield return new WaitForSeconds(1);
                    break;

                case IIssue.Type.Tires:
                    CurrentIssue = new TireIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    yield return new WaitUntil(() => !IsJacked);
                    CurrentIssue.CleanUp();
                    break;

                case IIssue.Type.Dirty:
                    CurrentIssue = new DirtIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    CurrentIssue.CleanUp();
                    break;
            }
        }

        CurrentIssue = null;

        MoveCarToEndPoint(this);
        yield return new WaitUntil(() => IsFixed);
    }

    private void MoveCarToEndPoint(Vehicle vehicle)
    {
        NetworkHandler.MoveCarToEndPoint(vehicle);
    }

}
