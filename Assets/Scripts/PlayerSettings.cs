using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("In-Game Settings")]
    public float MoveSpeed = 5f;
    public Vector3 SpawnPoint = new Vector3(0, 1f, 0);
    [Space(10)]
    [Header("Avatar")]
    public GameObject Hat;
    public GameObject Body;
    public Material SkinMaterial;
}
