// File: Assets/Scripts/Menus/ColorblindSettings.cs
using UnityEngine;

public class ColorblindSettings : MonoBehaviour
{
    public void OnToggleChanged(bool isOn)
    {
        // Print exactly what the Toggle passed us:
        Debug.Log($"[ColorblindSettings] Toggle clicked, isOn = {isOn}");

        PlayerPrefs.SetInt("ColorblindMode", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[ColorblindSettings] Saved ColorblindMode = {(isOn ? 1 : 0)}");
    }

    public static bool IsColorblindModeEnabled()
    {
        return PlayerPrefs.GetInt("ColorblindMode", 0) == 1;
    }
}
