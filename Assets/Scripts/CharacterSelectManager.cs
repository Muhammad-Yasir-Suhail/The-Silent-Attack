using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character Buttons - Drag the BUTTON component from each panel!")]
    public Button character1Button;
    public Button character2Button;
    public Button character3Button;
    
    [Header("Navigation")]
    public Button backButton;
    
    [Header("Selected Display (Optional)")]
    public Text selectedCharacterText;
    
    [Header("Character Panels (Optional - for visual feedback)")]
    public GameObject character1Panel;
    public GameObject character2Panel;
    public GameObject character3Panel;
    
    private int selectedCharacterIndex = 0;
    private GameObject[] characterPanels;
    
    void Start()
    {
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Check for EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("?? NO EVENTSYSTEM FOUND! Character panels won't work. Add: Right-click Hierarchy ? UI ? Event System");
        }
        
        // Put all panels in array
        characterPanels = new GameObject[] { character1Panel, character2Panel, character3Panel };
        
        // Setup button listeners - SIMPLE AND RELIABLE!
        if (character1Button != null)
        {
            character1Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectCharacter(0);
            });
            Debug.Log("Character 1 Button connected!");
        }
        else
        {
            Debug.LogError("Character 1 Button is NOT assigned in Inspector!");
        }
        
        if (character2Button != null)
        {
            character2Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectCharacter(1);
            });
            Debug.Log("Character 2 Button connected!");
        }
        else
        {
            Debug.LogError("Character 2 Button is NOT assigned in Inspector!");
        }
        
        if (character3Button != null)
        {
            character3Button.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectCharacter(2);
            });
            Debug.Log("Character 3 Button connected!");
        }
        else
        {
            Debug.LogError("Character 3 Button is NOT assigned in Inspector!");
        }
        
        // Setup navigation buttons
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                GoBack();
            });
        }
        
        // Load saved character
        selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        UpdateCharacterDisplay();
        
        Debug.Log("========== CHARACTER SELECT START ==========");
        Debug.Log("PlayerPrefs 'SelectedCharacter' = " + PlayerPrefs.GetInt("SelectedCharacter", -999));
        Debug.Log("Currently saved character: " + (selectedCharacterIndex + 1) + " (Index: " + selectedCharacterIndex + ")");
        Debug.Log("============================================");
    }
    
    
    public void SelectCharacter(int characterIndex)
    {
        selectedCharacterIndex = characterIndex;
        
        Debug.Log("======== CHARACTER SELECTION START ========");
        Debug.Log("Selecting Character: " + (characterIndex + 1));
        
        // Save to PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.Save();
        
        // Verify immediately
        int savedValue = PlayerPrefs.GetInt("SelectedCharacter", -999);
        Debug.Log("Saved value to PlayerPrefs: " + savedValue);
        Debug.Log("Verification: " + (savedValue == characterIndex ? "SUCCESS" : "FAILED"));
        Debug.Log("==========================================");
        
        UpdateCharacterDisplay();
        
        // Load next scene with delay
        Invoke("LoadWeaponSelect", 0.5f);
    }
    
    void LoadWeaponSelect()
    {
        Debug.Log("Loading Weapon Select. Final check - SelectedCharacter: " + PlayerPrefs.GetInt("SelectedCharacter"));
        SceneManager.LoadScene(7);
    }
    
    void UpdateCharacterDisplay()
    {
        // Update selected text
        if (selectedCharacterText != null)
        {
            // Try to get character name from panel's Text component
            string charName = "Character " + (selectedCharacterIndex + 1);
            if (characterPanels[selectedCharacterIndex] != null)
            {
                Text nameText = characterPanels[selectedCharacterIndex].GetComponentInChildren<Text>();
                if (nameText != null)
                    charName = nameText.text;
            }
            selectedCharacterText.text = charName + " Selected";
        }
        
        // Highlight selected character (scale up)
        for (int i = 0; i < characterPanels.Length; i++)
        {
            if (characterPanels[i] != null)
            {
                characterPanels[i].transform.localScale = (i == selectedCharacterIndex) 
                    ? new Vector3(1.1f, 1.1f, 1.1f) 
                    : Vector3.one;
            }
        }
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene(0); // Main Menu
    }
}
