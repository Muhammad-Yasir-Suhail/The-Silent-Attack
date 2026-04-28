using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSelectManager : MonoBehaviour
{
    [Header("Weapon Buttons - Drag the BUTTON component from each panel!")]
    public Button weapon1Button;
    public Button weapon2Button;
    public Button weapon3Button;
    
    [Header("Navigation")]
    public Button backButton;
    
    [Header("Selected Display (Optional)")]
    public Text selectedWeaponText;
    
    [Header("Weapon Panels (Optional - for visual feedback)")]
    public GameObject weapon1Panel;
    public GameObject weapon2Panel;
    public GameObject weapon3Panel;
    
    private int selectedWeaponIndex = 0;
    private GameObject[] weaponPanels;
    
    void Start()
    {
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Check for EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("?? NO EVENTSYSTEM FOUND! Weapon panels won't work. Add: Right-click Hierarchy ? UI ? Event System");
        }
        
        // Put all panels in array
        weaponPanels = new GameObject[] { weapon1Panel, weapon2Panel, weapon3Panel };
        
        // Setup button listeners - SIMPLE AND RELIABLE!
        if (weapon1Button != null)
        {
            weapon1Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectWeapon(0);
            });
            Debug.Log("Weapon 1 Button connected!");
        }
        else
        {
            Debug.LogError("Weapon 1 Button is NOT assigned in Inspector!");
        }
        
        if (weapon2Button != null)
        {
            weapon2Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectWeapon(1);
            });
            Debug.Log("Weapon 2 Button connected!");
        }
        else
        {
            Debug.LogError("Weapon 2 Button is NOT assigned in Inspector!");
        }
        
        if (weapon3Button != null)
        {
            weapon3Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectWeapon(2);
            });
            Debug.Log("Weapon 3 Button connected!");
        }
        else
        {
            Debug.LogError("Weapon 3 Button is NOT assigned in Inspector!");
        }
        
        // Setup navigation buttons
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoBack();
            });
        }
        
        // Load saved weapon
        selectedWeaponIndex = PlayerPrefs.GetInt("SelectedWeapon", 0);
        UpdateWeaponDisplay();
        
        Debug.Log("========== WEAPON SELECT START ==========");
        Debug.Log("PlayerPrefs 'SelectedWeapon' = " + PlayerPrefs.GetInt("SelectedWeapon", -999));
        Debug.Log("PlayerPrefs 'SelectedCharacter' = " + PlayerPrefs.GetInt("SelectedCharacter", -999));
        Debug.Log("Currently saved weapon: " + (selectedWeaponIndex + 1) + " (Index: " + selectedWeaponIndex + ")");
        Debug.Log("=========================================");
    }
    
    
    public void SelectWeapon(int weaponIndex)
    {
        selectedWeaponIndex = weaponIndex;
        
        Debug.Log("======== WEAPON SELECTION START ========");
        Debug.Log("Selecting Weapon: " + (weaponIndex + 1));
        
        // Save to PlayerPrefs
        PlayerPrefs.SetInt("SelectedWeapon", weaponIndex);
        PlayerPrefs.Save();
        
        // Verify immediately
        int savedValue = PlayerPrefs.GetInt("SelectedWeapon", -999);
        Debug.Log("Saved value to PlayerPrefs: " + savedValue);
        Debug.Log("Verification: " + (savedValue == weaponIndex ? "SUCCESS" : "FAILED"));
        Debug.Log("=======================================");
        
        UpdateWeaponDisplay();
        
        // Load next scene with delay
        Invoke("LoadLevelSelect", 0.5f);
    }
    
    void LoadLevelSelect()
    {
        Debug.Log("Loading Level Select. Final check - SelectedWeapon: " + PlayerPrefs.GetInt("SelectedWeapon"));
        SceneManager.LoadScene(4);
    }
    
    void UpdateWeaponDisplay()
    {
        // Update selected text
        if (selectedWeaponText != null)
        {
            // Try to get weapon name from panel's Text component
            string weaponName = "Weapon " + (selectedWeaponIndex + 1);
            if (weaponPanels[selectedWeaponIndex] != null)
            {
                Text nameText = weaponPanels[selectedWeaponIndex].GetComponentInChildren<Text>();
                if (nameText != null)
                    weaponName = nameText.text;
            }
            selectedWeaponText.text = weaponName + " Selected";
        }
        
        // Highlight selected weapon (scale up)
        for (int i = 0; i < weaponPanels.Length; i++)
        {
            if (weaponPanels[i] != null)
            {
                weaponPanels[i].transform.localScale = (i == selectedWeaponIndex) 
                    ? new Vector3(1.1f, 1.1f, 1.1f) 
                    : Vector3.one;
            }
        }
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene(5); // Character Select
    }
}
