using Photon.Pun;
using UnityEngine;
using FixRush;
using NUnit.Framework;
using System.Collections.Generic;

public class PlayerNetworkHandler : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Game References")]
    [SerializeField] internal PlayerSettings PlayerSettings;
    [SerializeField] internal GameContent GameContent;

    internal bool PhotonViewIsMine => photonView.IsMine;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;
        string colorhex = (string)data[0];

        if (photonView.IsMine)
        {
            // Offline mode: 2 models, Online mode: 1 model
            int times = PhotonManager.Instance.OfflineMode ? 2 : 1;

            for (int i = 0; i < times; i++)
            {
                transform.position = PlayerSpawner.Instance.LevelData.Spawnpoints[i];

                // Load body and hat
                GameObject body = PhotonNetwork.Instantiate(
                    GameContent.Bodies[PlayerSettings.Body].Prefab.name,
                    transform.position,
                    Quaternion.identity
                );
                GameObject hat = PhotonNetwork.Instantiate(
                    GameContent.Hats[PlayerSettings.Hat].Prefab.name,
                    transform.position,
                    Quaternion.identity
                );

                // Skin Color
                photonView.RPC(
                    nameof(RPC_SyncSkinColor),
                    RpcTarget.All,
                    body.GetComponent<PhotonView>().ViewID,
                    colorhex
                );

                // Set body and hat as children of the player
                photonView.RPC(
                    nameof(RPC_SetCosmeticParent),
                    RpcTarget.All,
                    body.GetComponent<PhotonView>().ViewID,
                    hat.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    i + 1
                );
            }
        }
    }

    internal void PickUpRequest(GameObject obj)
    {
        if (!photonView.IsMine) { return; }

        int objViewID = obj.GetComponent<PhotonView>().ViewID;

        photonView.RPC(
            nameof(RPC_PickUpObj),
            RpcTarget.All,
            objViewID,
            photonView.ViewID
        );
    }

    internal void DropRequest(GameObject obj)
    {
        if (!photonView.IsMine) { return; }

        int objViewID = obj.GetComponent<PhotonView>().ViewID;

        photonView.RPC(
            nameof(RPC_DropObj),
            RpcTarget.All,
            objViewID,
            photonView.ViewID
        );
    }

    internal void SyncWorkParticles(bool show)
    {
        photonView.RPC(
            nameof(RPC_SyncWorkParticles),
            RpcTarget.All,
            photonView.ViewID,
            show
        );
    }


    #region RPCs

    [PunRPC]
    private void RPC_SetCosmeticParent(int bodyViewID, int hatViewID, int playerViewID, int modelIndex)
    {
        PhotonView bodyView = PhotonView.Find(bodyViewID);
        PhotonView hatView = PhotonView.Find(hatViewID);
        PhotonView playerView = PhotonView.Find(playerViewID);

        if (bodyView != null && hatView != null && playerView != null)
        {
            // If modelIndex is 1, parent to Model1, else parent to Model2 (for offline mode)
            Transform modelTransform = modelIndex == 1 ? 
                playerView.GetComponent<PlayerController>().Model1.transform : 
                playerView.GetComponent<PlayerController>().Model2.transform;

            bodyView.transform.SetParent(modelTransform);
            hatView.transform.SetParent(modelTransform);

            // Position the body and hat at the player's position
            bodyView.transform.localPosition = Vector3.zero;
            hatView.transform.localPosition = Vector3.zero;
        }
    }

    [PunRPC]
    private void RPC_SyncSkinColor(int bodyViewID, string colorhex)
    {
        GameObject body = PhotonView.Find(bodyViewID).gameObject;

        if (body.TryGetComponent(out MeshRenderer bodyMeshRenderer))
        {
            Material newMat = new Material(bodyMeshRenderer.material);

            ColorUtility.TryParseHtmlString("#" + colorhex, out Color color);

            newMat.color = color;

            bodyMeshRenderer.material = newMat;
        }
    }

    [PunRPC]
    private void RPC_PickUpObj(int objViewID, int playerViewID)
    {
        PhotonView objView = PhotonView.Find(objViewID);
        PhotonView playerView = PhotonView.Find(playerViewID);

        IPickupable objP = objView.GetComponent<IPickupable>();
        PlayerController pc = playerView.GetComponent<PlayerController>();

        if (objView != null && playerView != null)
        {
            objP.PickUp(pc);
        }
    }

    [PunRPC]
    private void RPC_DropObj(int objViewID, int playerViewID)
    {
        PhotonView objView = PhotonView.Find(objViewID);
        PhotonView playerView = PhotonView.Find(playerViewID);

        IPickupable objP = objView.GetComponent<IPickupable>();
        PlayerController pc = playerView.GetComponent<PlayerController>();

        if (objView != null && playerView != null)
        {
            objP.Drop(pc);
        }
    }

    [PunRPC]
    private void RPC_SyncWorkParticles(int playerViewID, bool show)
    {
        PhotonView.Find(playerViewID).GetComponent<PlayerController>().OnWorkParticle(show);
    }

    #endregion

}
