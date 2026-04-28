using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    
    [Header("Sensitivity Settings")]
    public Slider mouseSensitivitySlider;
    public Text sensitivityValueText;
    
    [Header("Navigation")]
    public Button backButton;
    public Button applyButton;
    
    // Track which panel opened settings for return navigation
    private static GameObject returnToPanel = null;
    public static string returnToPanelName = "MainMenu";
    
    void Start()
    {
        LoadSettings();
        
        // Setup sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        
        // Setup dropdowns/toggles
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        
        // Setup buttons
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);
        
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToPreviousPanel);
    }
    
    void LoadSettings()
    {
        // Load audio
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        // Load graphics
        if (qualityDropdown != null)
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
        
        // Load sensitivity
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            UpdateSensitivityText(mouseSensitivitySlider.value);
        }
    }
    
    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
        else
        {
            AudioListener.volume = value;
        }
    }
    
    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
    
    void OnSensitivityChanged(float value)
    {
        UpdateSensitivityText(value);
    }
    
    void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F1");
    }
    
    void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    public void ApplySettings()
    {
        // Save all settings
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        
        if (mouseSensitivitySlider != null)
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
        
        if (qualityDropdown != null)
            PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
        
        PlayerPrefs.Save();
        
        Debug.Log("Settings saved!");
    }
    
    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = 1f;
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 0.7f;
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 1f;
        
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = 2f;
        
        if (qualityDropdown != null)
            qualityDropdown.value = 2;
        
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = true;
        
        ApplySettings();
    }
    
    // Return to the panel that opened settings
    void ReturnToPreviousPanel()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        // Close settings panel (this gameObject's parent or the panel containing this script)
        GameObject settingsPanel = transform.parent != null ? transform.parent.gameObject : gameObject;
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        // Enable the panel that opened settings - try reference first, then name
        if (returnToPanel != null)
        {
            returnToPanel.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(returnToPanelName))
        {
            GameObject returnPanel = GameObject.Find(returnToPanelName);
            if (returnPanel != null)
                returnPanel.SetActive(true);
        }
    }
    
    // Static method to set which panel to return to (by reference)
    public static void SetReturnPanel(GameObject panel)
    {
        returnToPanel = panel;
        if (panel != null)
            returnToPanelName = panel.name;
    }
    
    // Static method to set which panel to return to (by name)
    public static void SetReturnPanel(string panelName)
    {
        returnToPanelName = panelName;
        returnToPanel = null; // Clear reference when using name
    }
}
