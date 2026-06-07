using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("In-Game Settings")]
    public string Nickname = string.Empty;
    public float MoveSpeed = 5f;
    public float Money = 0f;
    [Space(10)]
    [Header("Avatar")]
    public int Hat;
    public int Body;
    public Color SkinColor;
}

