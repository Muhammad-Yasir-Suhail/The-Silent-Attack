using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    // Call this when player finishes a level
    public void CompleteLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        
        // Unlock next level
        UnlockNextLevel(currentLevel);
        
        // Go to level select or next level
        GoToLevelSelect();
    }
    
    public void GoToNextLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        
        // Unlock next level
        UnlockNextLevel(currentLevel);
        
        // Load next level directly
        SceneManager.LoadScene(currentLevel + 1);
    }
    
    public void GoToLevelSelect()
    {
        SceneManager.LoadScene(4); // Level Select scene
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0); // Main Menu
    }
    
    void UnlockNextLevel(int completedLevelNumber)
    {
        PlayerPrefs.SetInt("Level" + completedLevelNumber + "Completed", 1);
        PlayerPrefs.Save();
        Debug.Log("Level " + completedLevelNumber + " completed! Next level unlocked.");
    }
}
