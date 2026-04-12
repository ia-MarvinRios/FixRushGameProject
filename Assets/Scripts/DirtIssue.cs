using FixRush;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtIssue : IIssue
{
    public IIssue.Type IssueType => IIssue.Type.Dirty;

    public bool IsFixed { get; private set; } = false;

    public void CleanUp()
    {
        return;
    }

    public IEnumerator FixingCoroutine()
    {
        yield break;
    }

    public void HandleInteraction(IInteractable obj, PlayerController player)
    {
        return;
    }
}