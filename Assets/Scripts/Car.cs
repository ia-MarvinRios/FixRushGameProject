using FixRush;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Car : Vehicle
{
    internal bool IsJacked = false;

    public override void InitializeVehicle()
    {
        StartCoroutine(FixCarCoroutine());
    }

    public void OnInteracted(PlayerController player)
    {
        Debug.Log($"[Car] {player.FocusedObj} {player}");
        //CurrentIssue.HandleInteraction(player.FocusedObj, player);
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
