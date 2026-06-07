using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private static readonly Dictionary<ItemData, ItemSpawner> SpawnersByItem = new();

    [Header("Item Spawn")]
    [SerializeField] private ItemData _item;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _respawnDelay = 3f;

    private GameObject _currentItem;
    private Coroutine _respawnCoroutine;

    // -----------------------------------------------------------------------
    #region UNITY

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_item != null)
            SpawnersByItem[_item] = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SpawnItem();
    }

    private void OnDestroy()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_item != null && SpawnersByItem.TryGetValue(_item, out ItemSpawner spawner) && spawner == this)
            SpawnersByItem.Remove(_item);
    }

    #endregion

    // -----------------------------------------------------------------------
    #region RESPAWN API

    public static void NotifyPickedUp(ItemData item)
    {
        if (item == null) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (SpawnersByItem.TryGetValue(item, out ItemSpawner spawner))
            spawner.StartRespawn();
    }

    #endregion

    // -----------------------------------------------------------------------
    #region SPAWN LOGIC

    private void SpawnItem()
    {
        if (_item == null || _item.Prefab == null || _spawnPoint == null)
        {
            Debug.LogWarning("[ItemSpawner] Configuracion incompleta.");
            return;
        }

        if (_currentItem != null) return;

        _currentItem = PhotonNetwork.InstantiateRoomObject(
            _item.Prefab.name,
            _spawnPoint.position,
            _spawnPoint.rotation
        );

        if (_currentItem != null && _currentItem.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void StartRespawn()
    {
        _currentItem = null;

        if (_respawnCoroutine != null)
            StopCoroutine(_respawnCoroutine);

        _respawnCoroutine = StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(_respawnDelay);

        _respawnCoroutine = null;
        SpawnItem();
    }

    #endregion
}