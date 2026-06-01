using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private static readonly Dictionary<ItemData, ItemSpawner> SpawnersByItem = new();

    [SerializeField] private ItemData _item;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _respawnDelay = 3f;

    private GameObject _currentItem;

    private void Awake()
    {
        if (_item != null)
            SpawnersByItem[_item] = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            SpawnItem();
    }

    public static void NotifyPickedUp(ItemData item)
    {
        if (item == null) return;

        if (SpawnersByItem.TryGetValue(item, out ItemSpawner spawner))
            spawner.StartRespawn();
    }

    private void SpawnItem()
    {
        if (_item == null || _item.Prefab == null || _spawnPoint == null)
        {
            Debug.LogWarning("[ItemSpawner] Configuracion incompleta.");
            return;
        }

        //  No spawnear si ya hay uno activo
        if (_currentItem != null)
            return;

        _currentItem = PhotonNetwork.InstantiateRoomObject(
            _item.Prefab.name,
            _spawnPoint.position,
            _spawnPoint.rotation
        );

        //  Resetear física para que no salga volando
        if (_currentItem != null && _currentItem.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

    }

    private void StartRespawn()
    {
        _currentItem = null;
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnItem();
    }
}