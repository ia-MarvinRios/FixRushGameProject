using FixRush;
using NUnit.Framework.Interfaces;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the NPC queue in the shop level.
/// NPCs arrive, line up, request a spare part (ItemData), wait for the player
/// to deliver it, then leave. Mirrors VManager's queue logic using AIExtension.
/// Only runs on the MasterClient (same as VManager).
/// </summary>
public class NpcManager : MonoBehaviourPun
{
    public static NpcManager Instance { get; private set; }

    [System.Serializable]
    public class ItemSpawnData
    {
        public ItemData item;
        public Transform spawnPoint;
    }

    [Header("NPC Spawner")]
    [Space(10)]
    [SerializeField, Range(1, 10)] int _maxInstances = 5;
    [SerializeField] GameObject[] _npcPrefabs;
    [SerializeField] Vector3 _spawnPoint;
    [SerializeField] Vector3 _waitPoint;
    [SerializeField] Vector3 _endPoint;
    [SerializeField] float _waitPointOffset = 1f;
    [SerializeField, Range(0.1f, 5f)] float _spawnInterval = 2f;
    [SerializeField] Transform _counterPoint;

    [Header("Item Spawning")]
    [SerializeField] ItemSpawnData[] _availableItems;

    // --- Estado interno ---
    NavMeshAgent _leader;
    Coroutine _spawnCoroutine;
    WaitForSeconds _spawnIntervalWaitTime;

    bool _waitingForItem = false;
    NpcShop _currentNpcAtCounter;
    internal List<GameObject> ActiveNpcs { get; private set; }

    // -----------------------------------------------------------------------
    #region UNITY CALLBACKS

    private void Awake()
    {
        Instance = this;

        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
            return;
        }

        ActiveNpcs = new List<GameObject>();
        _spawnIntervalWaitTime = new WaitForSeconds(_spawnInterval);
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Spawnear todos los items al inicio
        SpawnAllItems();

        _spawnCoroutine = StartCoroutine(SpawnNpcs());
    }

    #endregion

    // -----------------------------------------------------------------------
    #region ITEM SPAWNING INICIAL

    /// <summary>
    /// Al iniciar la escena, spawnea un item por cada ItemSpawnData configurado.
    /// </summary>
    void SpawnAllItems()
    {
        foreach (ItemSpawnData data in _availableItems)
        {
            if (data.item == null || data.item.Prefab == null || data.spawnPoint == null)
            {
                Debug.LogWarning("[NpcManager] ItemSpawnData incompleto, se omite.");
                continue;
            }

            SpawnItem(data.item, data.spawnPoint);
        }
    }

    /// <summary>
    /// Spawnea el prefab del item y lo registra en su ItemData.
    /// </summary>
    void SpawnItem(ItemData item, Transform spawnPoint)
    {
        if (item.IsAvailable)
        {
            Debug.Log($"[NpcManager] {item.ItemName} ya tiene una instancia activa, no se spawnea otro.");
            return;
        }

        GameObject instance = PhotonNetwork.InstantiateRoomObject(
            item.Prefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (instance != null)
        {
            item.RegisterInstance(instance);
            Debug.Log($"[NpcManager] Item spawneado: {item.ItemName}");
        }
    }

    /// <summary>
    /// Llamar cuando el jugador recoge un item del mundo.
    /// Limpia la instancia del ItemData y spawnea una nueva automaticamente.
    /// </summary>
    public void OnItemPickedUp(ItemData item)
    {
        if (item == null) return;

        // Limpiar referencia (el jugador ya lo tiene)
        item.UnregisterInstance();

        // Buscar el spawn point de este item y spawnear uno nuevo
        ItemSpawnData spawnData = GetSpawnDataForItem(item);

        if (spawnData != null)
            SpawnItem(item, spawnData.spawnPoint);
    }

    ItemSpawnData GetSpawnDataForItem(ItemData item)
    {
        foreach (ItemSpawnData data in _availableItems)
        {
            if (data.item == item)
                return data;
        }
        return null;
    }

    Transform GetSpawnPointForItem(ItemData item)
    {
        ItemSpawnData data = GetSpawnDataForItem(item);
        return data?.spawnPoint;
    }

    #endregion

    // -----------------------------------------------------------------------
    #region SPAWN & QUEUE LOOP

    private IEnumerator SpawnNpcs()
    {
        while (true)
        {
            if (ActiveNpcs.Count < _maxInstances)
            {
                Debug.Log("[NpcManager] Spawning NPC...");

                int prefabIndex = Random.Range(0, _npcPrefabs.Length);

                GameObject obj = PhotonNetwork.InstantiateRoomObject(
                    _npcPrefabs[prefabIndex].name,
                    _spawnPoint,
                    Quaternion.identity
                );

                if (obj == null)
                {
                    yield return _spawnIntervalWaitTime;
                    continue;
                }

                NpcShop npc = obj.GetComponent<NpcShop>();

                if (npc != null)
                {
                    if (_availableItems != null && _availableItems.Length > 0)
                    {
                        ItemSpawnData randomItemData = _availableItems[Random.Range(0, _availableItems.Length)];

                        if (randomItemData != null && randomItemData.item != null)
                            npc.AssignRequest(randomItemData.item);
                        else
                        {
                            npc.AssignRequest(null);
                            Debug.LogWarning("[NpcManager] ItemSpawnData sin item asignado.");
                        }
                    }
                    else
                    {
                        npc.AssignRequest(null);
                        Debug.LogWarning("[NpcManager] No hay items configurados.");
                    }
                }

                ActiveNpcs.Add(obj);
                npc.QueueIndex = ActiveNpcs.Count - 1;

                UpdateLeader();

                if (_leader != null && _leader.isOnNavMesh)
                {
                    AIExtension.RecalculateQueueFrom(
                        npc.QueueIndex,
                        _waitPointOffset,
                        _waitPoint,
                        _leader,
                        ObjectsToAgentsList(ActiveNpcs)
                    );
                }

                yield return _spawnIntervalWaitTime;
            }

            if (ActiveNpcs.Count > 0 && !_waitingForItem)
            {
                MoveFirstNpcToCounter(ActiveNpcs[0].GetComponent<NavMeshAgent>());
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    void MoveFirstNpcToCounter(NavMeshAgent agent)
    {
        if (AIExtension.HasReachedDestination(agent) && !_waitingForItem)
        {
            agent.SetDestination(_counterPoint.position);
            StartCoroutine(SetUpForServing(agent));
        }
    }

    IEnumerator SetUpForServing(NavMeshAgent agent)
    {
        if (agent == null)
        {
            Debug.LogWarning("[NpcManager] Agent es null.");
            yield break;
        }

        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));

        agent.isStopped = true;
        agent.transform.position = _counterPoint.position;
        agent.transform.rotation = _counterPoint.rotation;

        if (!agent.TryGetComponent(out NpcShop npc)) yield break;

        _currentNpcAtCounter = npc;

        // Verificar si el item pedido está disponible en el mundo
        if (npc.RequestedItem == null || !npc.RequestedItem.IsAvailable)
        {
            Debug.Log("[NpcManager] Item no disponible en el mundo. NPC se va en 2 segundos.");
            yield return new WaitForSeconds(2f);
            MoveNpcToEndPoint(npc);
            yield break;
        }

        // El item existe en el mundo, esperar que el jugador lo entregue
        _waitingForItem = true;
        Debug.Log($"[NpcManager] NPC esperando: {npc.RequestedItem.ItemName}");
    }

    #endregion

    // -----------------------------------------------------------------------
    #region ITEM DELIVERY

    /// <summary>
    /// Llamar cuando el jugador le entrega el item al NPC.
    /// </summary>
    public void OnItemDelivered()
    {
        if (_currentNpcAtCounter == null) return;

        // El item ya fue entregado, limpiar referencia si aun existiera
        if (_currentNpcAtCounter.RequestedItem != null)
            _currentNpcAtCounter.RequestedItem.UnregisterInstance();

        MoveNpcToEndPoint(_currentNpcAtCounter);
    }

    #endregion

    // -----------------------------------------------------------------------
    #region NPC REMOVAL

    public void MoveNpcToEndPoint(NpcShop npc)
    {
        StartCoroutine(MoveToEndPointCoroutine(npc));
    }

    IEnumerator MoveToEndPointCoroutine(NpcShop npc)
    {
        NavMeshAgent agent = npc.Agent;
        agent.isStopped = false;
        agent.SetDestination(_endPoint);

        yield return new WaitUntil(() => AIExtension.HasReachedDestination(agent));

        RemoveNpc(npc);
    }

    public void RemoveNpc(NpcShop npc)
    {
        int index = ActiveNpcs.IndexOf(npc.gameObject);

        if (index < 0) return;

        ActiveNpcs.RemoveAt(index);

        UpdateLeader();

        PhotonNetwork.Destroy(npc.gameObject);

        _waitingForItem = false;
        _currentNpcAtCounter = null;

        if (_leader != null && _leader.isOnNavMesh)
        {
            AIExtension.RecalculateQueueFrom(
                index,
                _waitPointOffset,
                _waitPoint,
                _leader,
                ObjectsToAgentsList(ActiveNpcs)
            );
        }
    }

    #endregion

    // -----------------------------------------------------------------------
    #region HELPERS

    public void UpdateLeader()
    {
        _leader = null;

        if (ActiveNpcs.Count == 0) return;

        NavMeshAgent agent = ActiveNpcs[^1].GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            _leader = agent;
    }

    List<NavMeshAgent> ObjectsToAgentsList(List<GameObject> objects)
    {
        List<NavMeshAgent> agents = new List<NavMeshAgent>();
        foreach (GameObject obj in objects)
            agents.Add(obj.GetComponent<NavMeshAgent>());
        return agents;
    }

    #endregion

    // -----------------------------------------------------------------------
    #region GIZMOS

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnPoint, Vector3.one);
        Gizmos.DrawWireCube(_waitPoint, Vector3.one);
        Gizmos.DrawWireCube(_endPoint, Vector3.one);

        if (_counterPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_counterPoint.position, 0.5f);
        }
    }

    #endregion
}