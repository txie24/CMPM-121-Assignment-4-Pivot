using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string message;  

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager._instance != null)
            TooltipManager._instance.SetAndShowToolTip(message);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager._instance != null)
            TooltipManager._instance.HideToolTip();
    }
}
