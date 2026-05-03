using UnityEngine;

public class DetecZone : MonoBehaviour
{
    public GameObject panelUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            panelUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            panelUI.SetActive(false);
        }
    }
}
