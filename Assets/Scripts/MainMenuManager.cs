using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button levelSelectButton;
    public Button characterSelectButton;
    public Button weaponSelectButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public Button settingsBackButton;
    
    void Start()
    {
        // Make sure game is running normally
        Time.timeScale = 1f;
        
        // Show cursor in main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Hide panels
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Setup main menu buttons with sound
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                PlayGame();
            });
        }
        
        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoToLevelSelect();
            });
        }
        
        if (characterSelectButton != null)
        {
            characterSelectButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoToCharacterSelect();
            });
        }
        
        if (weaponSelectButton != null)
        {
            weaponSelectButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoToWeaponSelect();
            });
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                OpenSettings();
            });
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                QuitGame();
            });
        }
        
        // Setup settings back button
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                CloseSettings();
            });
        }
    }
    
    public void PlayGame()
    {
        // Load first level (Level 1)
        SceneManager.LoadScene(1);
    }
    
    public void GoToLevelSelect()
    {
        SceneManager.LoadScene(4); // Level Select scene
    }
    
    public void GoToCharacterSelect()
    {
        SceneManager.LoadScene(5); // Character Select scene
    }
    
    public void GoToWeaponSelect()
    {
        SceneManager.LoadScene(7); // Weapon Select scene
    }
    
    public void OpenSettings()
    {
        // Set return panel for settings navigation
        if (mainMenuPanel != null)
        {
            SettingsManager.SetReturnPanel(mainMenuPanel);
            mainMenuPanel.SetActive(false);
        }
        else
        {
            SettingsManager.SetReturnPanel("MainMenuPanel");
        }
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
