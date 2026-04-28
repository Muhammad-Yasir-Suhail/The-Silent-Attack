using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int damagePerHit = 25;
    public int currentHealth = 100;
    
    // 5 Health Images
    public GameObject healthImage100;
    public GameObject healthImage75;
    public GameObject healthImage50;
    public GameObject healthImage25;
    public GameObject healthImage0;
    
    public GameObject gameOverPanel;
    
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = 100;
        
        // Show 100% health at start
        if (healthImage100 != null) healthImage100.SetActive(true);
        if (healthImage75 != null) healthImage75.SetActive(false);
        if (healthImage50 != null) healthImage50.SetActive(false);
        if (healthImage25 != null) healthImage25.SetActive(false);
        if (healthImage0 != null) healthImage0.SetActive(false);
        
        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth = currentHealth - damage;
        
        // Hide all health images
        if (healthImage100 != null) healthImage100.SetActive(false);
        if (healthImage75 != null) healthImage75.SetActive(false);
        if (healthImage50 != null) healthImage50.SetActive(false);
        if (healthImage25 != null) healthImage25.SetActive(false);
        if (healthImage0 != null) healthImage0.SetActive(false);
        
        // Show correct health image
        if (currentHealth >= 100)
        {
            if (healthImage100 != null) healthImage100.SetActive(true);
        }
        else if (currentHealth >= 75)
        {
            if (healthImage75 != null) healthImage75.SetActive(true);
        }
        else if (currentHealth >= 50)
        {
            if (healthImage50 != null) healthImage50.SetActive(true);
        }
        else if (currentHealth >= 25)
        {
            if (healthImage25 != null) healthImage25.SetActive(true);
        }
        else
        {
            if (healthImage0 != null) healthImage0.SetActive(true);
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Try to find UIManager first
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowGameOver();
        }
        else
        {
            // Fallback to direct panel control
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
