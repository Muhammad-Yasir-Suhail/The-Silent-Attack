using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel;
    
    private bool isPaused = false;
    public GameObject canvas;
    
    void Update()
    {
        // Check if Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void PauseGame()
    {
        if (pausePanel != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();
            
            pausePanel.SetActive(true);
            canvas.SetActive(false);
            Time.timeScale = 0f; // Freeze game
            isPaused = true;
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Pause music
            if (AudioManager.Instance != null)
                AudioManager.Instance.PauseMusic();
        }

    }
    
    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();
            
            pausePanel.SetActive(false);
            canvas.SetActive(true);
            Time.timeScale = 1f; // Resume game
            isPaused = false;
            
            // Hide cursor (for FPS gameplay)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Resume music
            if (AudioManager.Instance != null)
                AudioManager.Instance.ResumeMusic();
        }
    }
}
