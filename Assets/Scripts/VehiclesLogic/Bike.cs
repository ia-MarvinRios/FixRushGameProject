using FixRush;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(vNetworkHandler))]
public class Bike : Vehicle
{
    [Header("Bike References")]
    [SerializeField] internal GameObject TireFront;
    [SerializeField] internal GameObject TireRear;

    public override void InitializeVehicle()
    {
        base.InitializeVehicle();

        StartCoroutine(FixBikeCoroutine());
    }

    private IEnumerator FixBikeCoroutine()
    {
        foreach (var i in IssueTypes)
        {
            switch (i)
            {
                default:
                    yield return new WaitForSeconds(1);
                    break;

                /*
                case IIssue.Type.Tires:
                    CurrentIssue = new TireIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    CurrentIssue.CleanUp();
                    break;
                */

                case IIssue.Type.Dirty:
                    CurrentIssue = new DirtIssue(this);
                    yield return StartCoroutine(CurrentIssue.FixingCoroutine());
                    CurrentIssue.CleanUp();
                    break;
            }
        }

        CurrentIssue = null;
        Fix();

        MoveCarToEndPoint(this);
    }

    private void MoveCarToEndPoint(Vehicle vehicle)
    {
        NetworkHandler.MoveCarToEndPoint(vehicle);
    }
}
