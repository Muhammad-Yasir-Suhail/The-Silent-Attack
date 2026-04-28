using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public void switchScene(int sceneNo)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneNo);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EnablePause(GameObject panel)
    {
        Time.timeScale = 0;
        panel.SetActive(true);
        
    }

    public void GameOverPanel(GameObject Panel)
    {
        Time.timeScale = 0;
        Panel.SetActive(true);
    }
    

}
