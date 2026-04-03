using UnityEngine;

public class RemotePlayerUI : MonoBehaviour
{
    public string Uid = string.Empty;
    public int BodyID = -1;
    public int HatID = -1;

    internal void UpdateUI(PlayerData data)
    {
        Uid = data.Uid;
        BodyID = data.BodyID;
        HatID = data.HatID;
    }
}