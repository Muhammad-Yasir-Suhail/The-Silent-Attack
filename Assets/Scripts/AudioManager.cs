using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;
    
    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    
    [Header("Weapon Sounds - AK47")]
    public AudioClip ak47ShootSound;
    public AudioClip ak47ReloadSound;
    
    [Header("Weapon Sounds - Pistol")]
    public AudioClip pistolShootSound;
    public AudioClip pistolReloadSound;
    
    [Header("Weapon Sounds - Sniper")]
    public AudioClip sniperShootSound;
    public AudioClip sniperReloadSound;
    
    [Header("Enemy Weapon Sounds")]
    public AudioClip enemyShootSound;
    
    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources
        InitializeAudioSources();
        
        // Load volume settings
        LoadVolumeSettings();
        
        // Play menu music
        PlayMusic(menuMusic);
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void InitializeAudioSources()
    {
        // Music source (existing AudioSource or create new)
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        
        // SFX source for weapon sounds
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        
        // UI source for button clicks
        GameObject uiObj = new GameObject("UISource");
        uiObj.transform.SetParent(transform);
        uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
    }
    
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        ApplyVolumeSettings();
    }
    
    public void ApplyVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
        
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
        
        if (uiSource != null)
            uiSource.volume = masterVolume * sfxVolume; // UI uses SFX volume
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Main Menu":
            case "Character Select":
            case "Weapon Select":
            case "Level Select":
                PlayMusic(menuMusic);
                break;
            case "level1":
                PlayMusic(level1Music);
                break;
            case "level2":
                PlayMusic(level2Music);
                break;
            case "level3":
                PlayMusic(level3Music);
                break;
            default:
                StopMusic();
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }
    
    // ========== UI SOUNDS ==========
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && uiSource != null)
            uiSource.PlayOneShot(buttonClickSound);
    }
    
    public void PlayButtonHover()
    {
        if (buttonHoverSound != null && uiSource != null)
            uiSource.PlayOneShot(buttonHoverSound, 0.5f);
    }
    
    // ========== WEAPON SOUNDS ==========
    public void PlayWeaponShoot(WeaponType weaponType)
    {
        AudioClip shootClip = null;
        
        switch (weaponType)
        {
            case WeaponType.AK47:
                shootClip = ak47ShootSound;
                break;
            case WeaponType.Pistol:
                shootClip = pistolShootSound;
                break;
            case WeaponType.Sniper:
                shootClip = sniperShootSound;
                break;
        }
        
        if (shootClip != null && sfxSource != null)
            sfxSource.PlayOneShot(shootClip);
    }
    
    public void PlayWeaponReload(WeaponType weaponType)
    {
        AudioClip reloadClip = null;
        
        switch (weaponType)
        {
            case WeaponType.AK47:
                reloadClip = ak47ReloadSound;
                break;
            case WeaponType.Pistol:
                reloadClip = pistolReloadSound;
                break;
            case WeaponType.Sniper:
                reloadClip = sniperReloadSound;
                break;
        }
        
        if (reloadClip != null && sfxSource != null)
            sfxSource.PlayOneShot(reloadClip);
    }
    
    public void PlayEnemyShoot()
    {
        if (enemyShootSound != null && sfxSource != null)
            sfxSource.PlayOneShot(enemyShootSound, 0.7f);
    }
    
    // ========== VOLUME CONTROL ==========
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
}

// Weapon type enum for easy weapon identification
public enum WeaponType
{
    AK47,
    Pistol,
    Sniper
}