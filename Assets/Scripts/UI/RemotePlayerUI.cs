using UnityEngine;
using FixRush;

public class RemotePlayerUI : MonoBehaviour
{
    public InfoPanel InfoPanel;
    public int ActorNumber = 0;
    public int BodyID = -1;
    public int HatID = -1;

    internal void UpdateUI(PlayerData data)
    {
        ActorNumber = data.ActorNumber;
        BodyID = data.BodyID;
        HatID = data.HatID;
    }
}