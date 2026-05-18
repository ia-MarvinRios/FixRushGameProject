using Photon.Pun;
using UnityEngine;

public class WaterHose : Interactable
{
    [Header("Water Hose Settings")]
    [SerializeField] private Transform _bucketRoot;

    private bool _done = false;
    private bool _isInteracting = false;

    public override void Interact(PlayerController player)
    {
        if(_done)
        {
            // Set bucket to full and release
            Bucket bucket = player.GrabbedObj.GetComponent<Bucket>();
            bucket.IsFull = true;
            photonView.RPC(
                nameof(RPC_GetReleaseBucket),
                RpcTarget.All,
                false,
                player.GetComponent<PhotonView>().ViewID
            );

            // Interaction
            _isInteracting = false;
        }
    }

    public override void CancelInteraction(PlayerController player)
    {
        // Release bucket
        if (player.GrabbedObj != null)
        {
            if (player.GrabbedObj.GetComponent<Bucket>() == null) { return; }

            photonView.RPC(
                nameof(RPC_GetReleaseBucket),
                RpcTarget.All,
                false,
                player.GetComponent<PhotonView>().ViewID
            );
        }

        _done          = false;
        _isInteracting = false;

        // UI and Audio
        AudioManager.Instance.StopAllFX();
        InGameUI.Instance.StopTaskProgress(false);
    }

    public override void InteractionStarted(PlayerController player)
    {
        if (player.GrabbedObj == null)
        {
            InGameUI.Instance.ShowHint("You need a bucket to refill it", 2f);
            return;
        }

        if (player.GrabbedObj.GetComponent<Bucket>() == null)
        {
            InGameUI.Instance.ShowHint("You need a bucket to refill it", 2f);
            return;
        }

        if (_isInteracting)
        {
            InGameUI.Instance.ShowHint("Refilling...", 2f);
            return;
        }

        _done          = true;
        _isInteracting = true;

        photonView.RPC(
            nameof(RPC_GetReleaseBucket), 
            RpcTarget.All, 
            true, 
            player.GetComponent<PhotonView>().ViewID
        );

        // UI and Audio
        AudioManager.Instance.PlaySoundByName("FillWater");
        InGameUI.Instance.StartTaskProgress(HoldTime);
    }

    private void GetReleaseBucket(bool get, PlayerController player)
    {
        if (get)
        {
            player.GrabbedObj.transform.parent = _bucketRoot;
            player.GrabbedObj.transform.position = _bucketRoot.position;
        }
        else
        {
            player.GrabbedObj.transform.parent = player.ObjRoot;
            player.GrabbedObj.transform.position = player.ObjRoot.position;
        }
    }

    #region RPCs

    [PunRPC]
    private void RPC_GetReleaseBucket(bool get, int playerViewID)
    {
        PlayerController player = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        GetReleaseBucket(get, player);
    }

    #endregion
}