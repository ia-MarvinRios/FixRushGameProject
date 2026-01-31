using UnityEngine;

public class GlobalItems : MonoBehaviour
{
    public static GlobalItems Instance { get; private set; }

    public GameObject[] Items;

    private void Awake()
    {
        Instance = this;
    }
}
