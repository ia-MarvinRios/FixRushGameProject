using FixRush;
using System.Collections;
using UnityEngine;

public class DirtIssue : IIssue
{
    private Vehicle _vehicle;

    public DirtIssue(Vehicle v)
    {
        _vehicle = v;
    }

    IIssue.Type IIssue.IssueType => IIssue.Type.Dirty;
    public bool IsFixed { get; private set; } = false;

    public void CleanUp()
    {

        // Hide UI panel
        _vehicle.NetworkHandler.SyncTaskPanel(false);
    }

    public IEnumerator FixingCoroutine()
    {
        // Show UI panel
        _vehicle.NetworkHandler.SyncTaskPanel(true, IIssue.Type.Dirty);

        yield return new WaitForSeconds(1f);
        _vehicle.NetworkHandler.SyncDirtAlpha(0);
        IsFixed = true;
    }

    public void HandleInteraction(IInteractable obj, PlayerController player)
    {
        return;
    }
}