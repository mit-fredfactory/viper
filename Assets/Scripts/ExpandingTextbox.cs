using UnityEngine;
using TMPro;

public class ExpandingTextbox : MonoBehaviour
{
    RectTransform RT;
    TMP_Text text;
    
    void Start()
    {
        RT = GetComponent<RectTransform>();
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        RT.sizeDelta = new Vector2(RT.rect.width, 25.5f * text.textInfo.lineCount);
    }
}
