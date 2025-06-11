using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    [Header("Scene Settings")]
    public bool isMainMenuScene = false;
    public bool isGameplayScene = false;

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

        if (isMainMenuScene)
        {
            Debug.Log("[SceneMusicController] Playing Main Menu Music");
            AudioManager.Instance.PlayMainMenuMusic();
        }
        else if (isGameplayScene)
        {
            Debug.Log("[SceneMusicController] Playing Gameplay Music");
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
}