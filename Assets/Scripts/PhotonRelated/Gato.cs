using FixRushGame;
using UnityEngine;

public class Gato : MonoBehaviour
{
    public int GatoRootID = -1;

    public void SetUpGato(Car car, int gatoRootID)
    {
        Interactable interactable = GetComponent<Interactable>();
        interactable.Pickable.ReleaseOwnership();
        interactable.Active = false;

        GatoRootID = gatoRootID;
    }

    public void PickUpGato(AuxPlayer entity)
    {
        Interactable interactable = GetComponent<Interactable>();
        interactable.Pickable.RequestPickUpObj(entity);
        interactable.Active = true;

        GatoRootID = -1;

        Debug.Log("[Gato] Gato has been picked up!");
    }
}
