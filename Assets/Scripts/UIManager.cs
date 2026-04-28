using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button levelsButton;
    public Button mainMenuButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Button gameOverRestartButton;
    public Button gameOverMainMenuButton;
    
    [Header("Settings")]
    public GameObject settingsPanel;
    public Button settingsBackButton;
    
    private bool isPaused = false;
    
    void Start()
    {
        // Initialize panels
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Setup pause menu buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        
        if (levelsButton != null)
            levelsButton.onClick.AddListener(GoToLevelSelect);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Setup game over buttons
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(RestartLevel);
        
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(GoToMainMenu);
        
        // Setup settings back button
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
        
        // Make sure game is running
        Time.timeScale = 1f;
    }
    
    void Update()
    {
        // Pause with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    // ========== PAUSE MENU FUNCTIONS ==========
    
    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        Time.timeScale = 0f; // Freeze game
        isPaused = true;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        Time.timeScale = 1f; // Resume game
        isPaused = false;
        
        // Hide cursor (for FPS gameplay)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Reset time
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(4); // Level Select scene
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main Menu scene
    }
    
    public void OpenSettings()
    {
        // Set return panel for settings navigation
        if (pausePanel != null)
        {
            SettingsManager.SetReturnPanel(pausePanel);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // ========== GAME OVER FUNCTIONS ==========
    
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
