using UnityEngine;

public class EmptyWheelRoot : MonoBehaviour
{
    public int WheelID = 0;
    public int GatoID = 0;
    public Vector3 Position = Vector3.zero;

    public void SetUp(int wId, int gId, Vector3 Pos)
    {
        WheelID = wId;
        GatoID = gId;
        Position = Pos;
    }
}
