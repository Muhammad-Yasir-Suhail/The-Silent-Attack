using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

   
    public void LoadCharacterSelect()
    {
        SceneManager.LoadScene("Character Select");
    }

    
    public void LoadWeaponSelect()
    {
        SceneManager.LoadScene("Weapon Select");
    }

    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("Level Select");
    }

   
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}