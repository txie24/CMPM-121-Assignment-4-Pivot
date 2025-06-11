using UnityEngine;
using TMPro;

public class AchievementPopup : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    public void Show(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
        gameObject.SetActive(true);
        Invoke(nameof(Hide), 4f);
    }

    void Hide() => gameObject.SetActive(false);
}
