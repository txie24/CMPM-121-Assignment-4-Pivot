// File: Assets/Scripts/Menus/ColorblindSettings.cs
using UnityEngine;

public class ColorblindSettings : MonoBehaviour
{
    /// <summary>
    /// Hook this method to your Toggle's OnValueChanged(Boolean) in Start→Settings.
    /// This writes 1 if the toggle is ON, or 0 if OFF.
    /// </summary>
    public void OnToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("ColorblindMode", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[ColorblindSettings] Saved ColorblindMode = {(isOn ? 1 : 0)}");
    }

    /// <summary>
    /// Basic helper: returns true if PlayerPrefs says colorblind=1, false otherwise.
    /// Used by HealthBar and also by our Toggle‐sync below.
    /// </summary>
    public static bool IsColorblindModeEnabled()
    {
        return PlayerPrefs.GetInt("ColorblindMode", 0) == 1;
    }

    /// <summary>
    /// If you need the raw int: 1 or 0 (not strictly necessary – you can always call IsColorblindModeEnabled()).
    /// </summary>
    public static int GetSavedColorblindMode()
    {
        return PlayerPrefs.GetInt("ColorblindMode", 0);
    }
}
