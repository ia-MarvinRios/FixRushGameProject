using FixRushGame;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] GameObject[] _iconPrefabs;

    public void SetUpCard(FixRushGame.IssueType[] issues)
    {
        foreach (FixRushGame.IssueType issue in issues)
        {
            switch (issue)
            {
                case FixRushGame.IssueType.Tires:
                    Instantiate(_iconPrefabs[0], transform);
                    break;
                case FixRushGame.IssueType.Dirty:
                    Instantiate(_iconPrefabs[1], transform);
                    break;
            }
        }
    }
}
