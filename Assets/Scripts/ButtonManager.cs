using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    private bool start;
    private LevelManager levelManager;
    private GameObject pauseMenu;
    private GameObject player;

    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex > 3)
        {
            pauseMenu = GameObject.Find("PauseMenu");
            player = GameObject.Find("Player");
        }

        Application.targetFrameRate = 60;

    }

    public void PlayButton()
    {
        if (SplashScreen.isFinished) SceneManager.LoadScene(1);
    }

    public void HighScoresButton()
    {
        if (SplashScreen.isFinished) SceneManager.LoadScene(2);
    }

    public void CreditsButton()
    {
        if (SplashScreen.isFinished) SceneManager.LoadScene(3);
    }

    public void ExitButton()
    {
        if (SplashScreen.isFinished) Application.Quit();
    }
    public void PauseGame()
    {
        if (pauseMenu.activeInHierarchy == false)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void ResetCurrentLevel()
    {
        player.SendMessage("ResetLevel");
    }

    public void DelayedLoadLevel(int level)
    {
        GameObject.Find("Level" + level).GetComponent<Animator>().SetTrigger("Expand");
        StartCoroutine(Load(level, .2f));
    }
    IEnumerator Load(int level, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        LoadLevel(level + (LevelManager.numberOfMenuScenes - 1));
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ReturnToLevelSelect()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
    }

    public void GoToNCSPlaylist()
    {
        Application.OpenURL("https://www.youtube.com/playlist?list=PLGXgNrVCYDVZaXQlykaYEa_SMCcuKOCW0");
    }

}
