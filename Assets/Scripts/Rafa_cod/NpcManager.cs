using FixRush;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcManager : MonoBehaviourPun
{
    public static NpcManager Instance { get; private set; }

    [Header("NPC Spawner")]
    [SerializeField, Range(1, 10)] int _maxInstances = 5;
    [SerializeField] GameObject[] _npcPrefabs;
    [SerializeField] Vector3 _spawnPoint;
    [SerializeField] Vector3 _waitPoint;
    [SerializeField] Vector3 _endPoint;
    [SerializeField] float _waitPointOffset = 1f;
    [SerializeField, Range(0.1f, 5f)] float _spawnInterval = 2f;
    [SerializeField] Transform _counterPoint;

    [Header("NPC Requests")]
    [SerializeField] ItemData[] _availableItems;

    NavMeshAgent _leader;
    WaitForSeconds _spawnIntervalWaitTime;

    bool _waitingForItem = false;
    NpcShop _currentNpcAtCounter;

    internal List<GameObject> ActiveNpcs { get; private set; }

    // -----------------------------------------------------------------------
    #region UNITY

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

        StartCoroutine(SpawnNpcs());
    }

    #endregion

    // -----------------------------------------------------------------------
    #region REQUEST DATA

    public ItemData GetItemByIndex(int index)
    {
        if (_availableItems == null) return null;
        if (index < 0 || index >= _availableItems.Length) return null;

        return _availableItems[index];
    }

    private void AssignRandomRequest(NpcShop npc)
    {
        if (_availableItems != null && _availableItems.Length > 0)
        {
            int itemIndex = Random.Range(0, _availableItems.Length);
            ItemData item = _availableItems[itemIndex];

            npc.AssignRequest(item, itemIndex);
        }
        else
        {
            npc.AssignRequest(null, -1);
            Debug.LogWarning("[NpcManager] No hay items configurados en _availableItems.");
        }
    }

    #endregion

    // -----------------------------------------------------------------------
    #region SPAWN & QUEUE

    private IEnumerator SpawnNpcs()
    {
        while (true)
        {
            if (ActiveNpcs.Count < _maxInstances)
            {
                int prefabIndex = Random.Range(0, _npcPrefabs.Length);

                GameObject obj = PhotonNetwork.InstantiateRoomObject(
                    _npcPrefabs[prefabIndex].name,
                    _spawnPoint,
                    Quaternion.identity
                );

                if (obj != null)
                    SetupSpawnedNpc(obj);

                yield return _spawnIntervalWaitTime;
            }

            if (ActiveNpcs.Count > 0 && !_waitingForItem)
            {
                NavMeshAgent firstAgent = ActiveNpcs[0].GetComponent<NavMeshAgent>();
                TryMoveToCounter(firstAgent);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void SetupSpawnedNpc(GameObject obj)
    {
        NpcShop npc = obj.GetComponent<NpcShop>();

        if (npc == null)
        {
            Debug.LogWarning("[NpcManager] El prefab no tiene NpcShop.");
            PhotonNetwork.Destroy(obj);
            return;
        }

        AssignRandomRequest(npc);

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
    }

    private void TryMoveToCounter(NavMeshAgent agent)
    {
        if (agent == null || _counterPoint == null || _waitingForItem) return;

        float distToWait = Vector3.Distance(agent.transform.position, _waitPoint);

        bool nearWaitPoint = distToWait < 2f;
        bool notMoving = !agent.pathPending &&
                         agent.remainingDistance <= agent.stoppingDistance + 0.1f;

        if (nearWaitPoint || notMoving)
        {
            _waitingForItem = true;
            agent.SetDestination(_counterPoint.position);

            Debug.Log("[NpcManager] Mandando NPC al counter...");

            StartCoroutine(SetUpForServing(agent));
        }
    }

    private IEnumerator SetUpForServing(NavMeshAgent agent)
    {
        if (agent == null)
        {
            _waitingForItem = false;
            yield break;
        }

        yield return new WaitUntil(() => !agent.pathPending);

        yield return new WaitUntil(() =>
            agent.remainingDistance <= agent.stoppingDistance + 0.5f
        );

        agent.isStopped = true;
        agent.transform.position = _counterPoint.position;
        agent.transform.rotation = _counterPoint.rotation;

        if (!agent.TryGetComponent(out NpcShop npc))
        {
            _waitingForItem = false;
            yield break;
        }

        _currentNpcAtCounter = npc;

        if (npc.RequestedItem == null)
        {
            yield return new WaitForSeconds(2f);
            MoveNpcToEndPoint(npc);
            yield break;
        }

        npc.SetAtCounter(true);

        Debug.Log($"[NpcManager] NPC en counter esperando: {npc.RequestedItem.ItemName}");
    }

    #endregion

    // -----------------------------------------------------------------------
    #region NPC REMOVAL

    public void MoveNpcToEndPoint(NpcShop npc)
    {
        if (npc == null) return;

        StartCoroutine(MoveToEndPointCoroutine(npc));
    }

    private IEnumerator MoveToEndPointCoroutine(NpcShop npc)
    {
        NavMeshAgent agent = npc.Agent;

        agent.isStopped = false;
        agent.SetDestination(_endPoint);

        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.1f
        );

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

    private List<NavMeshAgent> ObjectsToAgentsList(List<GameObject> objects)
    {
        List<NavMeshAgent> agents = new();

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