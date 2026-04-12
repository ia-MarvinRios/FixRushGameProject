using Photon.Pun;
using UnityEngine;
using FixRush;

public class vNetworkHandler : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // get th int issueIDs from the instantiation data int array
        IIssue.Type[] Issues;

        object[] data = photonView.InstantiationData;
        if (data == null || data.Length == 0)
            return;

        int[] issueIds = (int[])data[0];
        Issues = new IIssue.Type[issueIds.Length];

        for (int i = 0; i < issueIds.Length; i++)
        {
            Issues[i] = (IIssue.Type)issueIds[i];
        }

        Vehicle v = GetComponent<Vehicle>();
        v.Issues = Issues;
        //v.IsFixed = (bool)data[1];
    }
}
