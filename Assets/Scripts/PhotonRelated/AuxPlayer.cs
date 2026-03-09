using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class AuxPlayer : MonoBehaviour
{
    public readonly List<Interactable> FocusCandidates = new();
    public GameObject GrabbedObj { get; set; }
    public Interactable FocusedObj { get; set; }
    public PhotonView PhotonView { get; private set; }

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable)) {

            if (FocusCandidates.Contains(interactable) || interactable.Pickable?.IsPickedUp == true)
                return;

            FocusCandidates.Add(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable)) {
            FocusCandidates.Remove(interactable);
        }
    }
}
