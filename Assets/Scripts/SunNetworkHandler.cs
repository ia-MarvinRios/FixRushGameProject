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
            stream.SendNext(GameManager.Instance.LevelCoutdownStep);
            stream.SendNext(_time.text);
        }
        else
        {
            // Recibir datos
            float value = (float)stream.ReceiveNext();

            _sun.color = GameManager.Instance._skyColorGradient.Evaluate(value);
            _time.text = (string)stream.ReceiveNext();
        }
    }
}
