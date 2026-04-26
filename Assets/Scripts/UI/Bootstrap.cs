using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider _loadingBar;
    [SerializeField, Range(0.5f, 5f)] private float _loadingDuration = 1f;

    private void Start()
    {
        // Ensure that the PhotonManager is instantiated before any other scripts try to access it
        if (PhotonManager.Instance == null)
        {
            Debug.Log($"{nameof(Bootstrap)}: PhotonManager is null. Exiting game...");
            Application.Quit();
            return;
        }

        // Set the loading bar to 0
        _loadingBar.value = 0f;

        StartCoroutine(LoadBar());
    }

    private IEnumerator LoadBar()
    {
        float startTime = Time.time;

        while (Time.time < startTime + _loadingDuration)
        {
            float t = (Time.time - startTime) / _loadingDuration;
            _loadingBar.value = t;

            yield return null;
        }

        _loadingBar.value = 1f;

        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
