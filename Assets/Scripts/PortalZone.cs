using UnityEngine;

public class PortalZone : MonoBehaviour
{
    public GameObject lobbyPanel;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entró algo: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Es el player");
            lobbyPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lobbyPanel.SetActive(false);
        }
    }
}
