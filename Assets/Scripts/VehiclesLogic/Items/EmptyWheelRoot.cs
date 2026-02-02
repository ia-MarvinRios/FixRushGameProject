using UnityEngine;

public class EmptyWheelRoot : MonoBehaviour
{
    public int GatoID = 0;
    public Vector3 Position = Vector3.zero;

    public void SetUp(int id, Vector3 Pos)
    {
        GatoID = id;
        Position = Pos;
    }
}
