// Assets/Scripts/HUD/RelicUIManager.cs

using UnityEngine;
using UnityEngine.UI;

public class RelicUIManager : MonoBehaviour
{
    public static RelicUIManager Instance;

    [Header("Drag your RelicSlot prefab here (must have an 'Icon' child with Image + RelicTooltip)")]
    public GameObject relicSlotPrefab;

    [Header("Drag the 'Content' Transform under Canvas→RelicUI here")]
    public Transform contentParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }


    /// <param name="relicName">e.g. "Green Gem"</param>
    /// <param name="relicDescription">e.g. "Whenever you take damage, gain 5 mana"</param>
    /// <param name="spriteIndex">Index used by your sprite manager to pick the correct icon</param>
    public void AddRelicIcon(string relicName, string relicDescription, int spriteIndex)
    {
        if (relicSlotPrefab == null || contentParent == null)
        {
            Debug.LogError("[RelicUIManager] relicSlotPrefab or contentParent is not assigned in the Inspector.");
            return;
        }

        GameObject newSlot = Instantiate(relicSlotPrefab, contentParent);

        Transform iconTF = newSlot.transform.Find("Icon");
        if (iconTF == null)
        {
            Debug.LogError("[RelicUIManager] Could not find child 'Icon' on the spawned RelicSlot prefab!");
            return;
        }

        Image img = iconTF.GetComponent<Image>();
        if (img != null)
        {
            GameManager.Instance.spellIconManager.PlaceSprite(spriteIndex, img);
        }
        else
        {
            Debug.LogWarning("[RelicUIManager] 'Icon' child has no Image component.");
        }

        RelicTooltip tooltipComp = iconTF.GetComponent<RelicTooltip>();
        if (tooltipComp != null)
        {
            tooltipComp.relicName = relicName;
            tooltipComp.relicDescription = relicDescription;
            Debug.Log($"[RelicUIManager] Assigned tooltip.relicName='{relicName}', relicDescription='{relicDescription}'");
        }
        else
        {
            Debug.LogWarning("[RelicUIManager] 'Icon' child is missing RelicTooltip component!");
        }
    }
    public void ClearAllRelicIcons()
    {
        if (contentParent == null) return;
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}
