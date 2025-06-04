using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorblindToggleSync : MonoBehaviour
{
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        // 1) Read saved value (0 or 1) from PlayerPrefs
        bool wasCBOn = PlayerPrefs.GetInt("ColorblindMode", 0) == 1;

        // 2) Set Toggle’s state WITHOUT notifying any listeners
        _toggle.SetIsOnWithoutNotify(wasCBOn);
        Debug.Log($"[ToggleSync] Awake: set Toggle.isOn = {wasCBOn}");

        // 3) Now add the listener so that user clicks call our method
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        // Called only when user actually clicks the Toggle
        PlayerPrefs.SetInt("ColorblindMode", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[ToggleSync] User clicked Toggle → isOn = {isOn}. Saved ColorblindMode = {(isOn ? 1 : 0)}");
    }
}
