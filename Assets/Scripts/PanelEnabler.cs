using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelEnabler : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject gameoverPanel;
    public SceneManager sceneManager;
    //public GameObject[] images;
    //public GameObject pauseButton;
    //public GameObject joystick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseButton()
    {
        Time.timeScale = 0;
        //for (int i = 0; i < images.Length; i++)
        //{
        //    images[i].SetActive(false);
        //}

        pausePanel.SetActive(true);
    }

    public void Gameover()
    {
        gameoverPanel.SetActive(true);
    }

    public void ResumeButton()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void RestartButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenuButton(int sceneIndex)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneIndex);
    }

    public void NextButton(int sceneIndex)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneIndex);
    }

}
