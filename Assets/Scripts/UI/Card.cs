using FixRushGame;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] GameObject[] _iconPrefabs;

    public void SetUpCard(IssueType[] issues)
    {
        foreach (IssueType issue in issues)
        {
            switch (issue)
            {
                case IssueType.Tires:
                    Instantiate(_iconPrefabs[0], transform);
                    break;
                case IssueType.Dirty:
                    Instantiate(_iconPrefabs[1], transform);
                    break;
            }
        }
    }
}
