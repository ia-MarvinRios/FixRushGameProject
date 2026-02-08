using Photon.Pun;
using UnityEngine;

public class Generator : MonoBehaviourPun, IInteractable
{
    [SerializeField] GameObject _object;

    public void OnInteracted(AuxPlayer entity)
    {
        Debug.Log("Generator interacted");
        GenerateObject(entity);
    }

    void GenerateObject(AuxPlayer entity)
    {
        GameObject item = PhotonNetwork.Instantiate(
            _object.name, 
            entity.transform.position, 
            Quaternion.identity
        );

        item.GetComponent<Pickable>().RequestPickUpObj(entity);
    }
}
