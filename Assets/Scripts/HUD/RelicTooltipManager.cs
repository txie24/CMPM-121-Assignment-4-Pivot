
using UnityEngine;
using TMPro;

public class RelicTooltipManager : MonoBehaviour
{
    public static RelicTooltipManager Instance;

    [Header("Drag the root tooltip panel GameObject here (e.g. RelicTooltipBox)")]
    public GameObject tooltipBox;

    [Header("Drag the TextMeshProUGUI child that displays the relic's NAME")]
    public TextMeshProUGUI nameText;

    [Header("Drag the TextMeshProUGUI child that displays the relic's DESCRIPTION")]
    public TextMeshProUGUI descText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (tooltipBox != null)
            tooltipBox.SetActive(false);
    }

    private void Update()
    {
        if (tooltipBox != null && tooltipBox.activeSelf)
        {
            tooltipBox.transform.position = Input.mousePosition;
        }
    }


    public void SetAndShow(string relicName, string relicDescription)
    {
        if (tooltipBox == null || nameText == null || descText == null)
        {
            Debug.LogError("[RelicTooltipManager] One or more references are not assigned in the Inspector: " +
                           $"tooltipBox={tooltipBox}, nameText={nameText}, descText={descText}");
            return;
        }

        nameText.text = relicName;
        descText.text = relicDescription;
        tooltipBox.SetActive(true);
    }

    public void HideToolTip()
    {
        if (tooltipBox != null)
            tooltipBox.SetActive(false);
    }
}
