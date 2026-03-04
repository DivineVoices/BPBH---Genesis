using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{

    // Switch to a scene by name
    public void SwitchScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneSwitcher: No scene name provided!");
            return;
        }

        // Optional: check if the scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"SceneSwitcher: Scene \"{sceneName}\" not found in build settings!");
        }
    }

    public void ReloadScene()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void QuitGame()
    {
        Debug.Log("SceneSwitcher: Quitting game...");
        Application.Quit();
    }

}
