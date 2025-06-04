using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorblindToggleSync : MonoBehaviour
{
    private Toggle _toggle;
    private const string PREF_KEY = "ColorblindMode";

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        // If no key exists yet, explicitly set it to 0 (off).
        if (!PlayerPrefs.HasKey(PREF_KEY))
        {
            PlayerPrefs.SetInt(PREF_KEY, 0);
            PlayerPrefs.Save();
            Debug.Log($"[ToggleSync] No saved key found; initializing {PREF_KEY} = 0");
        }

        // 1) Read saved value (0 or 1) from PlayerPrefs
        bool wasCBOn = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;

        // 2) Set Toggle’s state WITHOUT notifying any listeners
        _toggle.SetIsOnWithoutNotify(wasCBOn);
        Debug.Log($"[ToggleSync] Awake: set Toggle.isOn = {wasCBOn}");

        // 3) Add the listener so that user clicks call our method
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        // Called only when user actually clicks the Toggle
        PlayerPrefs.SetInt(PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[ToggleSync] User clicked Toggle → isOn = {isOn}. Saved {PREF_KEY} = {(isOn ? 1 : 0)}");
    }
}
