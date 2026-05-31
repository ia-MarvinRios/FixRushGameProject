using UnityEngine;
using Photon.Pun;
using TMPro;

public class SunNetworkHandler : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Light _sun;
    [SerializeField] private TMP_Text _time;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos
            stream.SendNext(_sun.color);
            stream.SendNext(_time.text);
        }
        else
        {
            // Recibir datos
            _sun.color = (Color)stream.ReceiveNext();
            _time.text = (string)stream.ReceiveNext();
        }
    }
}
