// File: Assets/Scripts/Menus/ColorblindToggleSync.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorblindToggleSync : MonoBehaviour
{
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        // Read PlayerPrefs and set the Toggle’s isOn accordingly.
        bool wasCBOn = ColorblindSettings.IsColorblindModeEnabled();
        _toggle.isOn = wasCBOn;
    }
}
