
using UnityEngine;
using UnityEngine.EventSystems;

public class RelicTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("These fields get populated in code when the icon is instantiated")]
    public string relicName;
    [TextArea] public string relicDescription;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (RelicTooltipManager.Instance != null)
            RelicTooltipManager.Instance.SetAndShow(relicName, relicDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[RelicTooltip] OnPointerExit fired.");
        if (RelicTooltipManager.Instance != null)
            RelicTooltipManager.Instance.HideToolTip();
    }
}
