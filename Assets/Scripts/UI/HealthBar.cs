// File: Assets/Scripts/Core/HealthBar.cs
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Drag the 'slider' child (fill bar) here")]
    public GameObject slider;
    // 'slider' should be the child GameObject that has its own SpriteRenderer,
    // originally tinted green in the inspector.

    private SpriteRenderer bgRenderer; // SpriteRenderer on THIS GameObject (originally red).
    private SpriteRenderer fgRenderer; // SpriteRenderer on the 'slider' child (originally green).
    private Hittable hp;
    private float oldPerc = -1f;

    // Keep track of the last known CB state so we can detect changes at runtime:
    private bool lastKnownCB = false;

    void Awake()
    {
        // 1) Grab the SpriteRenderer for the parent (background, originally red).
        bgRenderer = GetComponent<SpriteRenderer>();

        // 2) Grab the SpriteRenderer for the 'slider' child (originally green).
        if (slider != null)
            fgRenderer = slider.GetComponent<SpriteRenderer>();

        // 3) Read the saved ColorblindMode flag:
        lastKnownCB = ColorblindSettings.IsColorblindModeEnabled();

        // 4) Immediately apply the correct palette for background + fill:
        ApplyColorScheme(lastKnownCB);

        Debug.Log($"[HealthBar] Awake: ColorblindMode={lastKnownCB}");
    }

    void Update()
    {
        // 1) If PlayerPrefs flips mid‐game, reapply the palette:
        bool currentCB = ColorblindSettings.IsColorblindModeEnabled();
        if (currentCB != lastKnownCB)
        {
            lastKnownCB = currentCB;
            ApplyColorScheme(currentCB);
            Debug.Log($"[HealthBar] Update: ColorblindMode changed → {currentCB}");
        }

        // 2) If we have an assigned Hittable, update the slider size & position:
        if (hp != null)
        {
            float perc = hp.hp * 1.0f / hp.max_hp;
            if (Mathf.Abs(oldPerc - perc) > 0.01f)
            {
                UpdateSliderVisual(perc);
                oldPerc = perc;
            }
        }
    }

    /// <summary>
    /// Called by your spawning/initialization code so this bar knows which Hittable to track.
    /// </summary>
    public void SetHealth(Hittable hp)
    {
        this.hp = hp;
        float perc = hp.hp * 1.0f / hp.max_hp;
        UpdateSliderVisual(perc);
        oldPerc = perc;
    }

    /// <summary>
    /// Resizes & repositions the fill‐bar (slider) based on the health percentage (0 → 1).
    /// </summary>
    private void UpdateSliderVisual(float perc)
    {
        if (slider != null)
        {
            slider.transform.localScale = new Vector3(perc, 1f, 1f);
            slider.transform.localPosition = new Vector3(-(1f - perc) / 2f, 0f, 0f);
        }
    }

    /// <summary>
    /// Switch between (red background + green fill) or (orange background + blue fill).
    /// </summary>
    private void ApplyColorScheme(bool isCBOn)
    {
        if (isCBOn)
        {
            // CB palette: background = orange (#F5A623), fill = blue (#4A90E2)
            if (bgRenderer != null)
                bgRenderer.color = new Color32(0xF5, 0xA6, 0x23, 0xFF);
            if (fgRenderer != null)
                fgRenderer.color = new Color32(0x4A, 0x90, 0xE2, 0xFF);
        }
        else
        {
            // Default palette: background = red, fill = green
            if (bgRenderer != null)
                bgRenderer.color = Color.red;
            if (fgRenderer != null)
                fgRenderer.color = Color.green;
        }
    }
}
