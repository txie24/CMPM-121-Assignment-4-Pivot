using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    Debug.Log("[AudioManager] Instance not found, creating new AudioManager");
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Background Music")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("UI Sound Effects")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;

    [Header("Spell Shooting Sounds")]
    public AudioClip arcaneBoltShoot;
    public AudioClip arcaneSprayShoot;
    public AudioClip arcaneBlastShoot;
    public AudioClip magicMissileShoot;
    public AudioClip railgunShoot;
    public AudioClip arcaneBurstShoot;

    [Header("Combat Sound Effects")]
    public AudioClip playerGotHit;
    public AudioClip hitEnemy;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;

    private Dictionary<string, AudioClip> spellSounds;
    private bool musicEnabled = true;
    private bool sfxEnabled = true;
    private bool isInitialized = false;
    private bool isQuitting = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Debug.LogWarning("[AudioManager] Duplicate AudioManager found, destroying duplicate");
            Destroy(gameObject);
            return;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
        CleanupAudioManager();
    }

    void Initialize()
    {
        if (isInitialized) return;

        Debug.Log("[AudioManager] Initializing AudioManager");
        CreateAudioSources();
        InitializeSpellSounds();
        LoadAudioSettings();
        isInitialized = true;
        Debug.Log("[AudioManager] AudioManager fully initialized!");

        StartCoroutine(DelayedEventSubscription());
    }

    void CreateAudioSources()
    {
        Debug.Log("[AudioManager] Creating AudioSources");

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.priority = 64;
        musicSource.volume = musicVolume; // Remove masterVolume multiplication

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.priority = 128;
        sfxSource.volume = sfxVolume; // Remove masterVolume multiplication

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.priority = 200;
        uiSource.volume = uiVolume; // Remove masterVolume multiplication

        Debug.Log("[AudioManager] All AudioSources created successfully!");
    }

    void InitializeSpellSounds()
    {
        Debug.Log("[AudioManager] Initializing spell sounds dictionary");

        spellSounds = new Dictionary<string, AudioClip>
        {
            { "Arcane Bolt", arcaneBoltShoot },
            { "Arcane Spray", arcaneSprayShoot },
            { "Arcane Blast", arcaneBlastShoot },
            { "Magic Missile", magicMissileShoot },
            { "Railgun", railgunShoot },
            { "Arcane Burst", arcaneBurstShoot }
        };

        Debug.Log("[AudioManager] Spell sound assignments:");
        foreach (var kvp in spellSounds)
        {
            bool hasSound = kvp.Value != null;
            Debug.Log($"  - '{kvp.Key}' -> {(hasSound ? $"ASSIGNED {kvp.Value.name}" : "NULL")}");
        }

        int assignedCount = 0;
        foreach (var kvp in spellSounds)
        {
            if (kvp.Value != null) assignedCount++;
        }

        Debug.Log($"[AudioManager] {assignedCount}/{spellSounds.Count} spell sounds assigned");

        if (assignedCount == 0)
        {
            Debug.LogWarning("[AudioManager] NO SPELL SOUNDS ASSIGNED! Please drag audio clips to the AudioManager inspector!");
        }
    }

    IEnumerator DelayedEventSubscription()
    {
        yield return new WaitForSeconds(0.1f);
        SubscribeToEvents();
        Debug.Log("[AudioManager] Event subscriptions complete");
    }

    void SubscribeToEvents()
    {
        try
        {
            if (EventBus.Instance != null)
            {
                EventBus.Instance.OnDamage += OnDamageDealt;
                Debug.Log("[AudioManager] Subscribed to EventBus.OnDamage");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AudioManager] Error subscribing to events: {e.Message}");
        }
    }

    void UnsubscribeFromEvents()
    {
        try
        {
            if (EventBus.Instance != null)
                EventBus.Instance.OnDamage -= OnDamageDealt;
        }
        catch (System.Exception e)
        {
            // Silently ignore errors during cleanup
            if (enableDebugLogs && !isQuitting)
                Debug.LogWarning($"[AudioManager] Error unsubscribing from events: {e.Message}");
        }
    }

    void CleanupAudioManager()
    {
        if (isQuitting) return; // Avoid double cleanup

        Debug.Log("[AudioManager] Cleaning up AudioManager");

        UnsubscribeFromEvents();

        // Stop and cleanup audio sources
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource = null;
        }
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource = null;
        }
        if (uiSource != null)
        {
            uiSource.Stop();
            uiSource = null;
        }

        // Clear the singleton reference
        if (_instance == this)
        {
            _instance = null;
        }

        isInitialized = false;
        Debug.Log("[AudioManager] Cleanup complete");
    }

    void OnDestroy()
    {
        CleanupAudioManager();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (isQuitting) return;

        if (pauseStatus && musicSource != null)
        {
            musicSource.Pause();
        }
        else if (!pauseStatus && musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (isQuitting) return;

        if (!hasFocus && musicSource != null)
        {
            musicSource.Pause();
        }
        else if (hasFocus && musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    #region Public Methods

    // SFX Methods - Uses SFX Volume Only
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (isQuitting || !isInitialized)
        {
            if (!isQuitting && enableDebugLogs) Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            if (!isQuitting) Initialize();
            if (isQuitting) return;
        }

        if (enableDebugLogs && !isQuitting) Debug.Log($"[AudioManager] PlaySFX called - SFX Enabled: {sfxEnabled}, Clip: {(clip != null ? clip.name : "NULL")}");

        if (!sfxEnabled)
        {
            if (enableDebugLogs && !isQuitting) Debug.Log("[AudioManager] SFX disabled, not playing sound");
            return;
        }

        if (clip == null)
        {
            if (enableDebugLogs && !isQuitting) Debug.LogWarning("[AudioManager] Clip is null, cannot play sound");
            return;
        }

        if (sfxSource == null)
        {
            if (!isQuitting) Debug.LogError("[AudioManager] sfxSource is null, recreating AudioSources");
            if (!isQuitting) CreateAudioSources();
            if (sfxSource == null || isQuitting)
            {
                if (!isQuitting) Debug.LogError("[AudioManager] Still can't create sfxSource!");
                return;
            }
        }

        // SFX volume is controlled ONLY by sfxVolume (separate from music)
        float finalVolume = sfxVolume * volumeMultiplier;
        sfxSource.PlayOneShot(clip, finalVolume);

        if (enableDebugLogs && !isQuitting) Debug.Log($"[AudioManager] SUCCESS: Played SFX: {clip.name} at volume {finalVolume:F2} ({Mathf.RoundToInt(sfxVolume * 100)}%)");
    }

    public void PlaySpellSFX(string spellName)
    {
        if (isQuitting) return;

        if (enableDebugLogs) Debug.Log($"[AudioManager] PlaySpellSFX called with: '{spellName}'");

        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (string.IsNullOrEmpty(spellName))
        {
            Debug.LogWarning("[AudioManager] Spell name is null or empty");
            return;
        }

        string baseSpellName = ExtractBaseSpellName(spellName);

        if (enableDebugLogs) Debug.Log($"[AudioManager] Looking for spell sound: '{spellName}' -> base: '{baseSpellName}'");

        if (spellSounds != null && spellSounds.TryGetValue(baseSpellName, out AudioClip clip))
        {
            if (clip != null)
            {
                PlaySFX(clip);
                Debug.Log($"[AudioManager] SUCCESS: Found and playing spell sound for: {baseSpellName} -> {clip.name}");
            }
            else
            {
                if (enableDebugLogs) Debug.LogWarning($"[AudioManager] Spell sound for '{baseSpellName}' is assigned but clip is NULL");
            }
        }
        else
        {
            if (enableDebugLogs) Debug.LogWarning($"[AudioManager] No sound mapping found for spell: {baseSpellName}");
        }
    }

    // UI Sound Methods - Uses SFX Volume
    public void PlayButtonClick()
    {
        if (isQuitting) return;
        PlaySFX(buttonClick);  // This will use SFX volume
    }

    public void PlayButtonHover()
    {
        if (isQuitting) return;
        PlaySFX(buttonHover);  // This will use SFX volume
    }

    // Music Methods - Uses Music Volume Only
    public void PlayMainMenuMusic()
    {
        if (isQuitting) return;

        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (mainMenuMusic == null)
        {
            Debug.Log("[AudioManager] mainMenuMusic is null");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] musicSource is null, recreating AudioSources");
            CreateAudioSources();
        }

        Debug.Log("[AudioManager] Playing Main Menu Music");
        musicSource.clip = mainMenuMusic;
        musicSource.volume = musicVolume;  // Only use music volume
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (isQuitting) return;

        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (gameplayMusic == null)
        {
            Debug.Log("[AudioManager] gameplayMusic is null");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] musicSource is null, recreating AudioSources");
            CreateAudioSources();
        }

        Debug.Log("[AudioManager] Playing Gameplay Music");
        musicSource.clip = gameplayMusic;
        musicSource.volume = musicVolume;  // Only use music volume
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isActiveAndEnabled && !isQuitting)
        {
            Debug.Log("[AudioManager] Stopping music");
            musicSource.Stop();
        }
    }

    // Volume Update Method
    public void UpdateMusicVolume()
    {
        if (musicSource != null && musicSource.isActiveAndEnabled && !isQuitting)
        {
            // Music volume is controlled ONLY by musicVolume (not masterVolume for separation)
            musicSource.volume = musicVolume;
            Debug.Log($"[AudioManager] Updated music volume to: {musicSource.volume:F2} ({Mathf.RoundToInt(musicVolume * 100)}%)");
        }
    }

    // Enable/Disable Methods
    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        Debug.Log($"[AudioManager] Music enabled set to: {enabled}");

        if (!enabled && musicSource != null && !isQuitting)
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Music disabled - stopped music");
        }
        else if (enabled)
        {
            Debug.Log("[AudioManager] Music enabled");
            // Optionally restart music here if needed
        }
    }

    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        Debug.Log($"[AudioManager] SFX enabled set to: {enabled}");
    }

    #endregion

    #region Event Handlers

    void OnDamageDealt(Vector3 position, Damage damage, Hittable target)
    {
        if (isQuitting) return;

        try
        {
            // Check if AudioManager is still valid before playing sounds
            if (this == null || !isActiveAndEnabled) return;

            if (target.team == Hittable.Team.PLAYER)
            {
                PlaySFX(playerGotHit);  // Uses SFX volume
            }
            else if (target.team == Hittable.Team.MONSTERS)
            {
                PlaySFX(hitEnemy);  // Uses SFX volume
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs && !isQuitting)
                Debug.LogError($"[AudioManager] Error in OnDamageDealt: {e.Message}");
        }
    }

    #endregion

    #region Utility Methods

    string ExtractBaseSpellName(string fullSpellName)
    {
        if (string.IsNullOrEmpty(fullSpellName)) return "";

        string[] modifiers = {
            "doubled", "split", "damage-amplified", "speed-amplified",
            "chaotic", "homing", "knockback", "bounce", "hasted", "piercing"
        };

        string baseName = fullSpellName.Trim();
        foreach (string modifier in modifiers)
        {
            baseName = baseName.Replace(" " + modifier, "").Trim();
        }

        return baseName;
    }

    void SaveAudioSettings()
    {
        if (isQuitting) return;

        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.6f);
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        Debug.Log($"[AudioManager] Loaded audio settings - Music: {Mathf.RoundToInt(musicVolume * 100)}%, SFX: {Mathf.RoundToInt(sfxVolume * 100)}%");
    }

    #endregion
}