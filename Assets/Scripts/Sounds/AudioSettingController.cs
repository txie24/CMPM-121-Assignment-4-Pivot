using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider musicVolumeSlider;    // Controls ONLY background music (0-100 range)
    public Slider sfxVolumeSlider;      // Controls spells, UI sounds, combat sounds (0-100 range)

    [Header("Colorblind Toggle")]
    public Toggle colorblindToggle;

    void Start()
    {
        SetupSliders();
        LoadSettings();
        AddSliderListeners();
    }

    void SetupSliders()
    {
        // Set slider ranges (0 to 100, where 100 = 100%)
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 100f;
            musicVolumeSlider.wholeNumbers = true; // Optional: make it snap to whole numbers
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 100f;
            sfxVolumeSlider.wholeNumbers = true; // Optional: make it snap to whole numbers
        }
    }

    void LoadSettings()
    {
        // Load Music Volume (default 70%, stored as 0.7, displayed as 70)
        float musicVolumeNormalized = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float musicVolumeDisplay = musicVolumeNormalized * 100f; // Convert 0.7 to 70
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolumeDisplay;

        // Load SFX Volume (default 80%, stored as 0.8, displayed as 80)
        float sfxVolumeNormalized = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        float sfxVolumeDisplay = sfxVolumeNormalized * 100f; // Convert 0.8 to 80
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolumeDisplay;

        // Load Colorblind Mode
        bool colorblindMode = PlayerPrefs.GetInt("ColorblindMode", 0) == 1;
        if (colorblindToggle != null)
            colorblindToggle.SetIsOnWithoutNotify(colorblindMode);

        // Apply loaded settings to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicVolume = musicVolumeNormalized;
            AudioManager.Instance.sfxVolume = sfxVolumeNormalized;
            AudioManager.Instance.UpdateMusicVolume();
        }

        Debug.Log($"[AudioSettings] Loaded settings - Music: {musicVolumeDisplay}%, SFX: {sfxVolumeDisplay}%");
    }

    void AddSliderListeners()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
        {
            // Use onValueChanged for real-time volume updates (silent)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Add pointer event listeners for test sound on release
            var eventTrigger = sfxVolumeSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = sfxVolumeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Play test sound when mouse is released
            var pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { PlaySFXTestSound(); });
            eventTrigger.triggers.Add(pointerUpEntry);
        }

        if (colorblindToggle != null)
            colorblindToggle.onValueChanged.AddListener(OnColorblindToggleChanged);
    }

    // Called when Music Volume slider changes (value is 0-100)
    public void OnMusicVolumeChanged(float sliderValue)
    {
        // Convert slider value (0-100) to normalized value (0-1)
        float normalizedValue = sliderValue / 100f;

        Debug.Log($"[AudioSettings] Music volume changed to: {Mathf.RoundToInt(sliderValue)}% (normalized: {normalizedValue:F2})");

        // Update AudioManager music volume with normalized value
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicVolume = normalizedValue;
            AudioManager.Instance.UpdateMusicVolume();
        }

        // Save normalized value to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", normalizedValue);
        PlayerPrefs.Save();
    }

    // Called when SFX Volume slider changes (value is 0-100)
    // This updates volume in real-time but doesn't play test sound
    public void OnSFXVolumeChanged(float sliderValue)
    {
        // Convert slider value (0-100) to normalized value (0-1)
        float normalizedValue = sliderValue / 100f;

        Debug.Log($"[AudioSettings] SFX volume changed to: {Mathf.RoundToInt(sliderValue)}% (normalized: {normalizedValue:F2})");

        // Update AudioManager SFX volume with normalized value
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxVolume = normalizedValue;
        }

        // Save normalized value to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", normalizedValue);
        PlayerPrefs.Save();
    }

    // Called when mouse is released from SFX slider - plays test sound once
    private void PlaySFXTestSound()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log($"[AudioSettings] Playing SFX test sound at volume: {AudioManager.Instance.sfxVolume:F2}");
            AudioManager.Instance.PlayButtonClick();
        }
    }

    // Helper method to play test sound with a small delay
    private System.Collections.IEnumerator PlayTestSoundDelayed()
    {
        yield return new WaitForEndOfFrame();
        if (AudioManager.Instance != null)
        {
            Debug.Log($"[AudioSettings] Playing test sound at SFX volume: {AudioManager.Instance.sfxVolume:F2}");
            AudioManager.Instance.PlayButtonClick();
        }
    }

    // Called when Colorblind Mode toggle changes
    public void OnColorblindToggleChanged(bool isOn)
    {
        Debug.Log($"[AudioSettings] Colorblind mode changed to: {isOn}");

        PlayerPrefs.SetInt("ColorblindMode", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Optional: Reset all settings to default
    public void ResetToDefaults()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 70f;  // 70% (displayed as 70 on slider)

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 80f;    // 80% (displayed as 80 on slider)

        if (colorblindToggle != null)
            colorblindToggle.isOn = false;

        Debug.Log("[AudioSettings] Reset all settings to defaults");
    }
}