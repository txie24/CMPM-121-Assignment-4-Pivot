using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;
    public TextMeshProUGUI textcomponent;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;        
        }
    }
    void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void SetAndShowToolTip(string message)
    {
        gameObject.SetActive(true);
        textcomponent.text = message;
    }
    public void HideToolTip()
    {
        gameObject.SetActive(false);
        textcomponent.text = string.Empty;
    }
}
