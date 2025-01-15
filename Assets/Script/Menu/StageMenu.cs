using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMenu : MonoBehaviour
{
    Image imgStage1;
    void Start()
    {
        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        imgStage1 = GameObject.Find("Img_Stage1").GetComponent<Image>(); ;

        imgHistoryRect.anchorMin = new Vector2(0, 0.75f);
        imgHistoryRect.anchorMax = new Vector2(1, 1);

        imgHistoryRect.offsetMin = Vector2.zero;
        imgHistoryRect.offsetMax = Vector2.zero;

        imgHistoryRect.pivot = new Vector2(0.5f, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (TitleMenu.SwordUpEnd)
        {
            imgStage1.rectTransform.Rotate(0, 0, 2f * Time.deltaTime);
        }
    }
}
