

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RelicUI : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("Your RelicSlot prefab must have a child GameObject named 'Icon' with an Image on it.")]
    public GameObject relicSlotPrefab;
    [Tooltip("Drag in the 'Content' RectTransform under RelicUI (the HLayoutGroup).")]
    public Transform contentParent;

    private List<GameObject> activeIcons = new List<GameObject>();
    private List<string> displayedRelicIDs = new List<string>();

    public static RelicUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        if (contentParent == null)
        {
            var t = transform.Find("Content");
            if (t != null)
                contentParent = t;
            else
                Debug.LogError("RelicUI: Couldn't find a child named 'Content'!");
        }
    }

    void Start()
    {
        ClearAllRelics();
    }

    public void AddRelic(Relic relic)
    {
        if (relic == null)
        {
            Debug.LogError("RelicUI: AddRelic was passed null");
            return;
        }
        if (displayedRelicIDs.Contains(relic.Name))
        {
            Debug.LogWarning($"RelicUI: '{relic.Name}' is already on the bar");
            return;
        }
        CreateIconFor(relic);
    }


    public void RemoveRelic(string relicName)
    {
        int idx = displayedRelicIDs.IndexOf(relicName);
        if (idx >= 0 && idx < activeIcons.Count)
        {
            Destroy(activeIcons[idx]);
            activeIcons.RemoveAt(idx);
            displayedRelicIDs.RemoveAt(idx);
        }
    }


    public void ClearAllRelics()
    {
        foreach (var go in activeIcons)
            if (go != null)
                Destroy(go);

        activeIcons.Clear();
        displayedRelicIDs.Clear();
    }


    private void CreateIconFor(Relic relic)
    {
        var iconTemplate = relicSlotPrefab.transform.Find("Icon");
        if (iconTemplate == null)
        {
            Debug.LogError("RelicUI: relicSlotPrefab is missing a child named 'Icon'");
            return;
        }

        GameObject iconGO = Instantiate(iconTemplate.gameObject, contentParent);
        iconGO.name = $"RelicIcon_{relic.Name}";

        var img = iconGO.GetComponent<Image>();
        var mgr = GameManager.Instance?.relicIconManager;
        Sprite spr = mgr != null ? mgr.Get(relic.SpriteIndex) : null;
        if (img == null || spr == null)
        {
            Debug.LogError($"RelicUI: couldn't get Image or sprite for '{relic.Name}'");
            Destroy(iconGO);
            return;
        }
        img.sprite = spr;
        img.color = Color.white;
        img.preserveAspect = true;

        var tooltipComp = iconGO.GetComponent<RelicTooltip>();
        if (tooltipComp != null)
        {
            tooltipComp.relicName = relic.Name;

            tooltipComp.relicDescription = relic.EffectData.description;
        }
        else
        {
            Debug.LogWarning($"RelicUI: Icon for '{relic.Name}' is missing a RelicTooltip component!");
        }

        var btn = iconGO.GetComponent<Button>() ?? iconGO.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => Debug.Log($"Clicked relic {relic.Name}"));

        activeIcons.Add(iconGO);
        displayedRelicIDs.Add(relic.Name);

        Debug.Log($"RelicUI: added '{relic.Name}'");
    }
}
