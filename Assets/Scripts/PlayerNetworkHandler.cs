using Photon.Pun;
using UnityEngine;

public class PlayerNetworkHandler : MonoBehaviourPun
{
    internal bool PhotonViewIsMine => photonView.IsMine;
}
