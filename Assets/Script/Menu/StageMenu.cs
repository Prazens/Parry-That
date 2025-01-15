using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();

        imgHistoryRect.anchorMin = new Vector2(0, 0.75f);
        imgHistoryRect.anchorMax = new Vector2(1, 1);

        imgHistoryRect.offsetMin = Vector2.zero;
        imgHistoryRect.offsetMax = Vector2.zero;

        imgHistoryRect.pivot = new Vector2(0.5f, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
