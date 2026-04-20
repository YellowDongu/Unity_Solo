using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PitchUI : MonoBehaviour
{
    //===========================================
    // Methods
    //===========================================
    public void ChangeColor(Color next)
    {
        image.color = next;
        if(text != null)
            text.color = next;
    }

    public void ChangeText(int value)
    {
        if (text != null)
            text.text = Mathf.Abs(value).ToString();
    }

    public void CalibratePosition(float y) { rectTransform.anchoredPosition = new Vector2(0.0f, y); }

    //===========================================
    // Variable & GetSet Methods
    //===========================================

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform rectTransform;
}
