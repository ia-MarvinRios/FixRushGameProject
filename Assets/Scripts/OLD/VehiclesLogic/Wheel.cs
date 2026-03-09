using UnityEngine;

public class Wheel : EmptyWheelRoot
{
    public bool NewTire = false;

    /// <summary>
    /// Sets up a bad wheel from a gatoRoot ID and it's worldPosition.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="Pos"></param>
    public void SetUp(int wId, int gId, Vector3 Pos, bool isNewWheel)
    {
        SetUp(wId, gId, Pos);

        NewTire = isNewWheel;
    }
}
