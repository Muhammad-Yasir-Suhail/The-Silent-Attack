using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Panels - Just drag the panel GameObject!")]
    public GameObject level1Panel;
    public GameObject level2Panel;
    public GameObject level3Panel;
    
    [Header("Navigation")]
    public Button backButton;
    
    private GameObject[] levelPanels;
    
    void Start()
    {
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Put all panels in array
        levelPanels = new GameObject[] { level1Panel, level2Panel, level3Panel };
        
        // Setup level panels and check if unlocked
        SetupLevelPanel(level1Panel, 1, 0); // Level 1 always unlocked (no requirement)
        SetupLevelPanel(level2Panel, 2, 1); // Level 2 requires Level 1 completed
        SetupLevelPanel(level3Panel, 3, 2); // Level 3 requires Level 2 completed
        
        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoBack();
            });
        }
    }
    
    void SetupLevelPanel(GameObject panel, int levelSceneIndex, int requiredLevel)
    {
        if (panel == null) return;
        
        // Check if level is unlocked
        bool isUnlocked = IsLevelUnlocked(requiredLevel);
        
        // Find Image and Text components in panel
        Image levelImage = panel.GetComponentInChildren<Image>();
        Text levelText = panel.GetComponentInChildren<Text>();
        
        // Find locked overlay (child with name containing "lock")
        GameObject lockedOverlay = FindLockedOverlay(panel);
        
        if (isUnlocked)
        {
            // Level is unlocked - add click listener
            AddClickListener(panel, levelSceneIndex);
            
            // Hide locked overlay if exists
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);
            
            // Normal colors
            if (levelImage != null)
                levelImage.color = Color.white;
            
            if (levelText != null)
                levelText.color = Color.white;
        }
        else
        {
            // Level is locked
            // Show locked overlay if exists
            if (lockedOverlay != null)
                lockedOverlay.SetActive(true);
            
            // Darken/gray out
            if (levelImage != null)
                levelImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            if (levelText != null)
                levelText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
    
    GameObject FindLockedOverlay(GameObject panel)
    {
        // Look for child with name containing "lock" (case insensitive)
        foreach (Transform child in panel.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.ToLower().Contains("lock"))
                return child.gameObject;
        }
        return null;
    }
    
    void AddClickListener(GameObject panel, int levelSceneIndex)
    {
        if (panel == null) return;
        
        // Add EventTrigger component if not present
        EventTrigger trigger = panel.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = panel.AddComponent<EventTrigger>();
        
        // Create click event
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { LoadLevel(levelSceneIndex); });
        trigger.triggers.Add(entry);
        
        // Make sure panel has an Image component for raycast
        if (panel.GetComponent<Image>() == null)
        {
            Image img = panel.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.01f); // Nearly invisible but still catches clicks
        }
    }
    
    bool IsLevelUnlocked(int requiredLevel)
    {
        // Level 1 (requiredLevel = 0) is always unlocked
        if (requiredLevel == 0)
            return true;
        
        // Check if previous level is completed
        int levelCompleted = PlayerPrefs.GetInt("Level" + requiredLevel + "Completed", 0);
        return levelCompleted == 1;
    }
    
    public void LoadLevel(int levelNumber)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        Debug.Log("Loading Level " + levelNumber);
        SceneManager.LoadScene(levelNumber);
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene(0); // Main Menu
    }
    
    // Call this when player completes a level
    public static void UnlockNextLevel(int completedLevelNumber)
    {
        PlayerPrefs.SetInt("Level" + completedLevelNumber + "Completed", 1);
        PlayerPrefs.Save();
        Debug.Log("Level " + completedLevelNumber + " completed! Next level unlocked.");
    }
}
