﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SpellUI : MonoBehaviour
{
    public GameObject icon;
    public RectTransform cooldown;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;
    public Spell spell;

    private int slotIndex = -1;
    float lastText;
    const float UPDATE_DELAY = 1f;

    void Awake()
    {
        // Auto-find the cooldown bar if left unassigned:
        if (cooldown == null)
        {
            var allRects = GetComponentsInChildren<RectTransform>();
            cooldown = allRects
                .FirstOrDefault(rt => rt.name.ToLower().Contains("cool"));
            if (cooldown == null)
                Debug.LogError($"[{name}] SpellUI: no child with 'cool' in its name!");
            else
                Debug.Log($"[{name}] SpellUI bound cooldown to '{cooldown.name}'");
        }

        // Auto-find manacost and damage with improved logging:
        if (manacost == null)
        {
            manacost = GetComponentsInChildren<TextMeshProUGUI>(true) // Include inactive objects
                .FirstOrDefault(t => t.name.ToLower().Contains("mana"));
            if (manacost == null)
                Debug.LogError($"[{name}] SpellUI: no child with 'mana' in its name! Please check the naming of the mana cost UI element.");
            else
                Debug.Log($"[{name}] SpellUI bound manacost to '{manacost.name}'");
        }
        if (damage == null)
        {
            damage = GetComponentsInChildren<TextMeshProUGUI>(true) // Include inactive objects
                .FirstOrDefault(t => t.name.ToLower().Contains("dmg") || t.name.ToLower().Contains("damage"));
            if (damage == null)
                Debug.LogError($"[{name}] SpellUI: no child with 'dmg' or 'damage' in its name! Please check the naming of the damage UI element.");
            else
                Debug.Log($"[{name}] SpellUI bound damage to '{damage.name}'");
        }
        if (icon == null)
        {
            var imgGO = transform
                .GetComponentsInChildren<Image>(true) // Include inactive objects
                .Select(i => i.gameObject)
                .FirstOrDefault(go => go.name.ToLower().Contains("icon"));
            if (imgGO != null) icon = imgGO;
            else Debug.LogWarning($"[{name}] SpellUI: could not auto-find Icon");
        }
    }

    public void Initialize(int index)
    {
        slotIndex = index;
    }

    void Update()
    {
        if (spell == null) return;

        // Update text once per second
        if (Time.time > lastText + UPDATE_DELAY)
        {
            if (manacost != null)
            {
                manacost.text = Mathf.RoundToInt(spell.Mana).ToString();
            }
            else
            {
                Debug.LogWarning($"[{name}] SpellUI: manacost is null, cannot update mana cost for slot {slotIndex}");
            }

            if (damage != null)
            {
                damage.text = Mathf.RoundToInt(spell.Damage).ToString();
            }
            else
            {
                Debug.LogWarning($"[{name}] SpellUI: damage is null, cannot update damage for slot {slotIndex}");
            }

            lastText = Time.time;
        }

        // Update cooldown bar
        if (cooldown != null)
        {
            float elapsed = Time.time - spell.lastCast;
            float pct = elapsed >= spell.Cooldown ? 0f : 1f - (elapsed / spell.Cooldown);
            cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48f * pct);
        }

        // Update icon image
        if (icon != null)
        {
            var img = icon.GetComponent<Image>();
            if (GameManager.Instance?.spellIconManager != null)
            {
                GameManager.Instance.spellIconManager.PlaceSprite(spell.IconIndex, img);
            }
        }
    }

    public void SetSpell(Spell s)
    {
        spell = s;
        if (spell != null && icon != null)
        {
            var img = icon.GetComponent<Image>();
            if (GameManager.Instance?.spellIconManager != null)
            {
                GameManager.Instance.spellIconManager.PlaceSprite(spell.IconIndex, img);
            }
        }

        // Update the Tooltip component on this same GameObject (or child):
        var tooltip = GetComponent<Tooltip>();
        if (tooltip != null)
        {
            // build whatever string you want—e.g. display name + modifiers
            string msg = spell?.DisplayName ?? "";
            // if you want to list modifiers, you could loop through them here
            tooltip.message = msg;
        }
    }

    public void DropSpell()
    {
        try
        {
            // Validate we have a valid slot index
            if (slotIndex < 0)
            {
                Debug.LogWarning($"[SpellUI] Cannot drop spell - invalid slot index: {slotIndex}");
                return;
            }

            // Find the player controller safely
            PlayerController playerController = null;

            // First try to get it from GameManager
            if (GameManager.Instance?.player != null)
            {
                playerController = GameManager.Instance.player.GetComponent<PlayerController>();
            }

            // If that fails, search for it in the scene
            if (playerController == null)
            {
                playerController = Object.FindFirstObjectByType<PlayerController>();
            }

            if (playerController == null)
            {
                Debug.LogError("[SpellUI] Cannot find PlayerController in scene");
                return;
            }

            // Validate spellcaster exists
            if (playerController.spellcaster == null)
            {
                Debug.LogError("[SpellUI] PlayerController has no SpellCaster component");
                return;
            }

            // Ensure the spells list is large enough
            var spells = playerController.spellcaster.spells;
            if (spells == null)
            {
                Debug.LogError("[SpellUI] SpellCaster spells list is null");
                return;
            }

            // Extend the list if needed
            while (spells.Count <= slotIndex)
            {
                spells.Add(null);
            }

            // Now safely drop the spell
            Debug.Log($"[SpellUI] Dropping spell from slot {slotIndex}: {spells[slotIndex]?.DisplayName ?? "null"}");

            spells[slotIndex] = null;

            // Clear this UI
            spell = null;

            // Immediately deactivate this UI element
            gameObject.SetActive(false);

            // Update the UI safely
            try
            {
                // Try SpellUIContainer first
                SpellUIContainer container = Object.FindFirstObjectByType<SpellUIContainer>();
                if (container != null)
                {
                    container.UpdateSpellUIs();
                }
                else
                {
                    // Fallback to PlayerController method
                    playerController.UpdateSpellUI();
                }
            }
            catch (System.Exception uiEx)
            {
                Debug.LogError($"[SpellUI] Error updating spell UI: {uiEx.Message}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SpellUI] Error in DropSpell: {ex.Message}\n{ex.StackTrace}");
        }
    }
}