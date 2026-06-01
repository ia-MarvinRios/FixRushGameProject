using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GarbajeCollector : MonoBehaviourPun
{
    public static GarbajeCollector Instance { get; private set; }

    private class Trash
    {
        internal GameObject Object;
        internal float DestroyAt;

        internal Trash(GameObject obj, float destroyAt)
        {
            Object = obj;
            DestroyAt = destroyAt;
        }
    }

    private readonly List<Trash> _collectorQueue = new List<Trash>();
    private readonly WaitForSeconds _wait = new WaitForSeconds(0.1f);

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) { enabled = false;  return; }
        Instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(DestructionCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void QueueDestruction(GameObject obj, float delay)
    {
        _collectorQueue.Add(new Trash(obj, Time.time + delay));
    }

    private IEnumerator DestructionCoroutine()
    {
        while (true)
        {
            for (int i = _collectorQueue.Count - 1; i >= 0; i--)
            {
                Trash trash = _collectorQueue[i];

                if (trash.Object == null)
                {
                    _collectorQueue.RemoveAt(i);
                    continue;
                }

                if (Time.time >= trash.DestroyAt)
                {
                    PhotonNetwork.Destroy(trash.Object);
                    _collectorQueue.RemoveAt(i);
                }
            }

            yield return _wait;
        }
    }
}
