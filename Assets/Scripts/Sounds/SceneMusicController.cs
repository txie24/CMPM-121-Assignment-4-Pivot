// Assets/Scripts/Sounds/SceneMusicController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"[SceneMusicController] Scene started: {SceneManager.GetActiveScene().name}");
        Invoke(nameof(PlaySceneMusic), 0.1f);
    }

    void PlaySceneMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[SceneMusicController] AudioManager.Instance is null!");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Start")
        {
            Debug.Log("[SceneMusicController] Playing Main Menu Music");
            AudioManager.Instance.PlayMainMenuMusic();
        }
        else if (currentScene == "Main")
        {
            Debug.Log("[SceneMusicController] Playing Gameplay Music");
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    void OnDestroy()
    {
        // No need to stop music here; let the AudioManager handle transitions.
    }
}