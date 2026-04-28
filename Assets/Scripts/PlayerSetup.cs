using UnityEngine;
using UnityEngine.UI;

// Add this to your Player GameObject in each level scene
public class PlayerSetup : MonoBehaviour
{
    [Header("Character Images")]
    public Image playerCharacterImage; // The UI Image that shows player character
    public Sprite character1Sprite;
    public Sprite character2Sprite;
    public Sprite character3Sprite;
    
    [Header("Weapons")]
    public GameObject pistol;   // Weapon 1
    public GameObject rifle;    // Weapon 2
    public GameObject sniper;   // Weapon 3
    
    void Start()
    {
        LoadSelectedCharacter();
        LoadSelectedWeapon();
    }
    
    void LoadSelectedCharacter()
    {
        // Get selected character from PlayerPrefs
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        
        Debug.Log("PlayerSetup: Loading Character " + (selectedCharacter + 1) + " (Index: " + selectedCharacter + ")");
        
        if (playerCharacterImage != null)
        {
            // Set the character image based on selection
            switch (selectedCharacter)
            {
                case 0:
                    playerCharacterImage.sprite = character1Sprite;
                    Debug.Log("PlayerSetup: Character 1 sprite applied");
                    break;
                case 1:
                    playerCharacterImage.sprite = character2Sprite;
                    Debug.Log("PlayerSetup: Character 2 sprite applied");
                    break;
                case 2:
                    playerCharacterImage.sprite = character3Sprite;
                    Debug.Log("PlayerSetup: Character 3 sprite applied");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("PlayerSetup: playerCharacterImage is NULL - cannot display character");
        }
    }
    
    void LoadSelectedWeapon()
    {
        // Get selected weapon from PlayerPrefs
        int selectedWeapon = PlayerPrefs.GetInt("SelectedWeapon", 0);
        
        Debug.Log("PlayerSetup: Loading Weapon " + (selectedWeapon + 1) + " (Index: " + selectedWeapon + ")");
        
        // Disable all weapons first
        if (pistol != null) pistol.SetActive(false);
        if (rifle != null) rifle.SetActive(false);
        if (sniper != null) sniper.SetActive(false);
        
        // Enable only the selected weapon
        switch (selectedWeapon)
        {
            case 0:
                if (pistol != null)
                {
                    pistol.SetActive(true);
                    Debug.Log("PlayerSetup: Pistol (Weapon 1) ACTIVATED");
                }
                else
                {
                    Debug.LogWarning("PlayerSetup: Pistol is NULL - cannot activate");
                }
                break;
            case 1:
                if (rifle != null)
                {
                    rifle.SetActive(true);
                    Debug.Log("PlayerSetup: Rifle (Weapon 2) ACTIVATED");
                }
                else
                {
                    Debug.LogWarning("PlayerSetup: Rifle is NULL - cannot activate");
                }
                break;
            case 2:
                if (sniper != null)
                {
                    sniper.SetActive(true);
                    Debug.Log("PlayerSetup: Sniper (Weapon 3) ACTIVATED");
                }
                else
                {
                    Debug.LogWarning("PlayerSetup: Sniper is NULL - cannot activate");
                }
                break;
        }
    }
}
