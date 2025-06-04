using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
