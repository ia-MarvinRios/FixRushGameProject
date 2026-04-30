using FixRush;
using System.Collections;
using UnityEngine;

public class TireIssue : IIssue
{
    private Car _car;

    public TireIssue(Car car)
    {
        _car = car;
    }

    public IIssue.Type IssueType => IIssue.Type.Tires;

    public bool IsFixed { get; private set; } = false;

    public void CleanUp()
    {
        // Hide UI panel
        _car.NetworkHandler.SyncTaskPanel(false);

        // Audio
        AudioManager.Instance.PlaySoundByName("TaskCompleted");
    }

    public IEnumerator FixingCoroutine()
    {
        // Show UI panel
        InGameUI.Instance.ShowTaskPanel(true, IIssue.Type.Tires);

        yield return new WaitForSeconds(1f);
        IsFixed = true;
    }

    public void HandleInteraction(IInteractable obj, PlayerController player)
    {
        return;
    }
}
