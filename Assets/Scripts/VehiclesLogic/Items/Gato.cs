using System.Collections;
using UnityEngine;

public class Gato : MonoBehaviour
{
    public int GatoRootID = -1;

    public void SetUpGato(Car car, int gatoRootID)
    {
        GatoRootID = gatoRootID;
        StartCoroutine(CheckPicked(car));
    }
    IEnumerator CheckPicked(Car c)
    {
        Debug.Log("Checking if picked...");
        yield return new WaitUntil(() =>
        {
            if (TryGetComponent(out Pickable pickable))
            {
                return !pickable.IsPickable;
            }
            return false;
        }
        );

        c.RemoveGato(GatoRootID);
        GatoRootID = -1;

        Debug.Log("Gato has been picked up!");
    }
}
