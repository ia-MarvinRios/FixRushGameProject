using System.Collections;

public enum IssueType
{
    Tires,
    Dirty,
    //Engine,
}

public interface IIssue
{
    public IssueType Type { get; }
    public bool IsFixed { get; }

    abstract void CleanUp();
    abstract IEnumerator FixingCoroutine();
    abstract void HandleInteraction(Interactable obj, AuxPlayer entity);
}