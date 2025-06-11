using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSoundController : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    void Start()
    {
        Debug.Log($"[ButtonSound] Added to button: {gameObject.name}");
    }

    // When mouse hovers over button
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[ButtonSound] HOVER on {gameObject.name}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    // When button is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ButtonSound] CLICK on {gameObject.name}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}