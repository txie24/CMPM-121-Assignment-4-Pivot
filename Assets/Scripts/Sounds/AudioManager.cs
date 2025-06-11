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
                // Try to find existing AudioManager first
                _instance = FindFirstObjectByType<AudioManager>();

                if (_instance == null)
                {
                    // Auto-create AudioManager if it doesn't exist
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

    // PRIVATE - AudioSources created automatically
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;

    private Dictionary<string, AudioClip> spellSounds;
    private bool musicEnabled = true;
    private bool sfxEnabled = true;
    private bool isInitialized = false;

    void Awake()
    {
        // Handle singleton properly
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

        // Subscribe to events after initialization
        StartCoroutine(DelayedEventSubscription());
    }

    void CreateAudioSources()
    {
        Debug.Log("[AudioManager] Creating AudioSources");

        // MUSIC SOURCE
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.priority = 64;
        musicSource.volume = musicVolume * masterVolume;

        // SFX SOURCE  
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.priority = 128;
        sfxSource.volume = sfxVolume * masterVolume;

        // UI SOURCE
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.priority = 200;
        uiSource.volume = uiVolume * masterVolume;

        Debug.Log("[AudioManager] All AudioSources created successfully!");
    }

    void InitializeSpellSounds()
    {
        Debug.Log("[AudioManager] Initializing spell sounds dictionary");

        // Use the inspector-assigned clips directly
        spellSounds = new Dictionary<string, AudioClip>
        {
            { "Arcane Bolt", arcaneBoltShoot },
            { "Arcane Spray", arcaneSprayShoot },
            { "Arcane Blast", arcaneBlastShoot },
            { "Magic Missile", magicMissileShoot },
            { "Railgun", railgunShoot },
            { "Arcane Burst", arcaneBlastShoot } // Use arcane blast sound for arcane burst
        };

        // Debug what sounds are assigned
        Debug.Log("[AudioManager] Spell sound assignments:");
        foreach (var kvp in spellSounds)
        {
            bool hasSound = kvp.Value != null;
            Debug.Log($"  - '{kvp.Key}' -> {(hasSound ? $"ASSIGNED {kvp.Value.name}" : "NULL")}");
        }

        // Count how many sounds are properly assigned
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
            Debug.LogWarning($"[AudioManager] Error unsubscribing from events: {e.Message}");
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
        if (_instance == this)
            _instance = null;
    }

    #region Public Methods

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        // Ensure we're initialized
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

        // Extract base spell name (remove modifiers)
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

            // List all available spell sounds for debugging
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

    #endregion

    #region Event Handlers

    void OnDamageDealt(Vector3 position, Damage damage, Hittable target)
    {
        try
        {
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

    #region Test Methods

    [ContextMenu("Test Railgun Sound")]
    public void TestRailgunSound()
    {
        Debug.Log("[AudioManager] === TESTING RAILGUN SOUND ===");
        Debug.Log($"[AudioManager] isInitialized: {isInitialized}");
        Debug.Log($"[AudioManager] railgunShoot assigned: {railgunShoot != null}");
        Debug.Log($"[AudioManager] sfxEnabled: {sfxEnabled}");
        Debug.Log($"[AudioManager] sfxSource: {sfxSource != null}");

        if (!isInitialized)
        {
            Debug.Log("[AudioManager] Not initialized, initializing now");
            Initialize();
        }

        if (railgunShoot != null)
        {
            PlaySFX(railgunShoot);
            Debug.Log("[AudioManager] Test: Playing railgun sound directly");
        }
        else
        {
            Debug.LogError("[AudioManager] Test: Railgun sound not assigned in inspector!");
        }
    }

    [ContextMenu("Test Spell Sound")]
    public void TestSpellSound()
    {
        Debug.Log("[AudioManager] === TESTING SPELL SOUND ===");
        PlaySpellSFX("Railgun");
    }

    [ContextMenu("List All Assignments")]
    public void ListAllAssignments()
    {
        Debug.Log("[AudioManager] === CURRENT AUDIO ASSIGNMENTS ===");
        Debug.Log($"buttonHover: {(buttonHover != null ? buttonHover.name : "NULL")}");
        Debug.Log($"buttonClick: {(buttonClick != null ? buttonClick.name : "NULL")}");
        Debug.Log($"arcaneBoltShoot: {(arcaneBoltShoot != null ? arcaneBoltShoot.name : "NULL")}");
        Debug.Log($"arcaneSprayShoot: {(arcaneSprayShoot != null ? arcaneSprayShoot.name : "NULL")}");
        Debug.Log($"arcaneBlastShoot: {(arcaneBlastShoot != null ? arcaneBlastShoot.name : "NULL")}");
        Debug.Log($"magicMissileShoot: {(magicMissileShoot != null ? magicMissileShoot.name : "NULL")}");
        Debug.Log($"railgunShoot: {(railgunShoot != null ? railgunShoot.name : "NULL")}");
        Debug.Log($"playerGotHit: {(playerGotHit != null ? playerGotHit.name : "NULL")}");
        Debug.Log($"hitEnemy: {(hitEnemy != null ? hitEnemy.name : "NULL")}");
    }

    [ContextMenu("Force Initialize")]
    public void ForceInitialize()
    {
        Debug.Log("[AudioManager] FORCE INITIALIZING");
        isInitialized = false;
        Initialize();
    }

    #endregion
}