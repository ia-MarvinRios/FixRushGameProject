using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class NpcShop : MonoBehaviourPun, AIExtension.IQueueAgent
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private BoxCollider _collider; // Agrega un BoxCollider al prefab

    // --- IQueueAgent ---
    public int QueueIndex { get; set; }
    public Vector3 Size => _collider.size; // AIExtension necesita esto para calcular espacios

    public NavMeshAgent Agent => _agent;
    public ItemData RequestedItem { get; private set; }

    private void Awake()
    {
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();

        if (_collider == null)
            _collider = GetComponent<BoxCollider>();
    }

    public void AssignRequest(ItemData item)
    {
        RequestedItem = item;
        Debug.Log($"[NpcShop] NPC requesting: {item?.ItemName}");
    }

    public void OnServed()
    {
        Debug.Log($"[NpcShop] NPC served!");
    }
}