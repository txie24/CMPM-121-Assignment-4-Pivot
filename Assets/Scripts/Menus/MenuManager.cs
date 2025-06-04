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
    [Header("Assign these in the Inspector")]
    public GameObject mainMenu;    
    public GameObject settingsMenu; 
    public GameObject achievementsMenu; 
    public GameObject creditsMenu;

    void Start()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        achievementsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }


    public void ShowSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void ShowAchievements()
    {
        mainMenu.SetActive(false);
        achievementsMenu.SetActive(true);
    }

    public void ShowCredits()
    {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }


    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        achievementsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

}
