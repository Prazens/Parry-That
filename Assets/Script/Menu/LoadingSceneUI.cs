using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneUI : MonoBehaviour
{
    [SerializeField] private RectTransform rt_slide;
    [SerializeField] private RectTransform rt_txt;
    void Start()
    {
        rt_slide = GameObject.Find("Slider").GetComponent<RectTransform>();
        rt_txt = GameObject.Find("Text (TMP)").GetComponent<RectTransform>();
        TextMeshProUGUI txt = GameObject.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        rt_slide.anchorMin = new Vector2(0.1f, 0.47f);
        rt_slide.anchorMax = new Vector2(0.9f, 0.53f);

        // rt_txt.anchoredPosition = new Vector2(0, 0.59f);
        rt_txt.anchorMin = new Vector2(0.3f, 0.53f);
        rt_txt.anchorMax = new Vector2(0.7f, 0.56f);

        txt.fontSize = 55 * (Screen.width / 1080);
        Debug.Log($"{txt.fontSize}");
    }

}
