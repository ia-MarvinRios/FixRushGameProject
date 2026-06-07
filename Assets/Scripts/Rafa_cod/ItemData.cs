using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "FixRush/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string ItemName;
    public Sprite Icon;
    public GameObject Prefab;
}