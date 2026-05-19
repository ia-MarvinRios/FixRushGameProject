using UnityEngine;

/// <summary>
/// ScriptableObject that defines a spare part / item that NPCs can request in the shop.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "FixRush/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string ItemName;
    public Sprite Icon;
    public GameObject Prefab;

    [HideInInspector]
    public GameObject SpawnedInstance; // El que está activo en el mundo ahora mismo

    public bool IsAvailable => SpawnedInstance != null;

    public void RegisterInstance(GameObject instance)
    {
        SpawnedInstance = instance;
    }

    public void UnregisterInstance()
    {
        SpawnedInstance = null;
    }
}