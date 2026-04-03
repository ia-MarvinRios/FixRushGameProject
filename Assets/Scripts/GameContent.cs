using UnityEngine;

[CreateAssetMenu(fileName = "GameContent", menuName = "Scriptable Objects/GameContent")]
public class GameContent : ScriptableObject
{
    [Header("Available Cosmetics")]
    [SerializeField] internal Cosmetic[] Bodies;
    [SerializeField] internal Cosmetic[] Hats;
}
