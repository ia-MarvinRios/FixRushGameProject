using Photon.Pun;
using UnityEngine;

public class ScriptsToIgnore : MonoBehaviourPun
{
    [SerializeField] internal MonoBehaviour[] _scriptsToIgnore;

    private void Awake()
    {
        if (_scriptsToIgnore != null && !photonView.IsMine)
        {
            foreach (var script in _scriptsToIgnore)
            {
                script.enabled = false;
            }
        }
    }
}
