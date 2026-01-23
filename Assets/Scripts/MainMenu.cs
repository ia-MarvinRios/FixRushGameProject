using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GoToScene(string name) { SceneManager.LoadScene(name, LoadSceneMode.Single); }
}
