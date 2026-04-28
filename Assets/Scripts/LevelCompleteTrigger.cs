using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("Level Complete UI")]
    public GameObject levelCompletePanel; // Assign your Level Complete UI Panel here
    
    [Header("Optional")]
    public bool pauseGameOnComplete = true; // Stop time when level completes
    
    private bool levelCompleted = false;
    
    void Start()
    {
        // Make sure the panel is hidden at start
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("LevelCompleteTrigger: No Level Complete Panel assigned!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Player (this script) entered a trigger - check if it's the level complete point
        if (!levelCompleted && other.CompareTag("LevelCompletePoint"))
        {
            CompleteLevelTrigger();
        }
    }
    
    void CompleteLevelTrigger()
    {
        levelCompleted = true;
        
        Debug.Log("Level Complete! Player reached the finish point.");
        
        // Show the level complete panel
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        
        // Pause the game
        if (pauseGameOnComplete)
        {
            Time.timeScale = 0f;
        }
        
        // Show cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Call the LevelComplete script to handle progression
        LevelComplete levelComplete = FindObjectOfType<LevelComplete>();
        if (levelComplete != null)
        {
            levelComplete.CompleteLevel();
        }
    }
}
