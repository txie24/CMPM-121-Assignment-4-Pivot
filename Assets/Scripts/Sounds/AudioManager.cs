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
        musicSource.volume = musicVolume * masterVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.priority = 128;
        sfxSource.volume = sfxVolume * masterVolume;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.priority = 200;
        uiSource.volume = uiVolume * masterVolume;

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
            Debug.LogError("[AudioManager] NO SPELL SOUNDS ASSIGNED! Please drag audio clips to the AudioManager inspector!");
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
            if (enableDebugLogs)
                Debug.LogWarning($"[AudioManager] Error unsubscribing from events: {e.Message}");
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();

        // Stop all audio sources before destroying
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

        if (_instance == this)
            _instance = null;
    }

    void OnApplicationPause(bool pauseStatus)
    {
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

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (enableDebugLogs) Debug.Log($"[AudioManager] PlaySFX called - SFX Enabled: {sfxEnabled}, Clip: {(clip != null ? clip.name : "NULL")}");

        if (!sfxEnabled)
        {
            if (enableDebugLogs) Debug.Log("[AudioManager] SFX disabled, not playing sound");
            return;
        }

        if (clip == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[AudioManager] ERROR: Clip is null, cannot play sound");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("[AudioManager] sfxSource is null, recreating AudioSources");
            CreateAudioSources();
            if (sfxSource == null)
            {
                Debug.LogError("[AudioManager] Still can't create sfxSource!");
                return;
            }
        }

        float finalVolume = sfxVolume * masterVolume * volumeMultiplier;
        sfxSource.PlayOneShot(clip, finalVolume);

        if (enableDebugLogs) Debug.Log($"[AudioManager] SUCCESS: Played SFX: {clip.name} at volume {finalVolume:F2}");
    }

    public void PlaySpellSFX(string spellName)
    {
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
                Debug.LogWarning($"[AudioManager] ERROR: Spell sound for '{baseSpellName}' is assigned but clip is NULL");
            }
        }
        else
        {
            Debug.LogWarning($"[AudioManager] ERROR: No sound mapping found for spell: {baseSpellName}");

            Debug.Log("[AudioManager] Available spell sounds:");
            foreach (var kvp in spellSounds)
            {
                Debug.Log($"  - '{kvp.Key}' -> {(kvp.Value != null ? kvp.Value.name : "NULL")}");
            }
        }
    }

    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        Debug.Log($"[AudioManager] SFX enabled set to: {enabled}");
        SaveAudioSettings();
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayButtonHover()
    {
        PlaySFX(buttonHover);
    }

    // === NEW MUSIC METHODS ===
    public void PlayMainMenuMusic()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (!musicEnabled || mainMenuMusic == null)
        {
            Debug.Log("[AudioManager] Music disabled or mainMenuMusic is null");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] musicSource is null, recreating AudioSources");
            CreateAudioSources();
        }

        Debug.Log("[AudioManager] Playing Main Menu Music");
        musicSource.clip = mainMenuMusic;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[AudioManager] Not initialized yet, initializing now");
            Initialize();
        }

        if (!musicEnabled || gameplayMusic == null)
        {
            Debug.Log("[AudioManager] Music disabled or gameplayMusic is null");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] musicSource is null, recreating AudioSources");
            CreateAudioSources();
        }

        Debug.Log("[AudioManager] Playing Gameplay Music");
        musicSource.clip = gameplayMusic;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isActiveAndEnabled)
        {
            Debug.Log("[AudioManager] Stopping music");
            musicSource.Stop();
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        Debug.Log($"[AudioManager] Music enabled set to: {enabled}");

        if (!enabled && musicSource != null)
        {
            musicSource.Stop();
        }

        SaveAudioSettings();
    }

    #endregion

    #region Event Handlers

    void OnDamageDealt(Vector3 position, Damage damage, Hittable target)
    {
        try
        {
            // Check if AudioManager is still valid before playing sounds
            if (this == null || !isActiveAndEnabled) return;

            if (target.team == Hittable.Team.PLAYER)
            {
                PlaySFX(playerGotHit);
            }
            else if (target.team == Hittable.Team.MONSTERS)
            {
                PlaySFX(hitEnemy);
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
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

        Debug.Log($"[AudioManager] Loaded audio settings - SFX Enabled: {sfxEnabled}");
    }

    #endregion
}