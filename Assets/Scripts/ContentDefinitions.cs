using UnityEngine;

public class PlayerData
{
    internal string Uid {  get; private set; }
    public string PlayerName { get; private set; }
    public bool IsReady { get; private set; }
    public int BodyID { get; private set; }
    public int HatID { get; private set; }
    public PlayerData(string userID, string playerName, bool isReady, int bodyID, int hatID)
    {
        Uid = userID;
        PlayerName = playerName;
        IsReady = isReady;
        BodyID = bodyID;
        HatID = hatID;
    }
}

public class RoomData
{
    public string Name;
    public int PlayerCount;
    public int MaxPlayers;
}

[System.Serializable]
public struct Cosmetic
{
    public string Name;
    public GameObject Prefab;
}
