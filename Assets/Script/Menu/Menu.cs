using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Metadata;

public class Menu : MonoBehaviour
{
    private GameObject Title;
    private GameObject StageMenu;

    private Image backgroundImage;
    private Image background2Image;

    void Start()
    {
        // 타이틀 배경과 스테이지 메뉴 배경이 위아래로 연속되게 배치
        Title = GameObject.Find("Title");
        Title.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        Title.GetComponent<RectTransform>().anchorMax = Vector2.one;
        Title.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        Title.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        Title.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        StageMenu = GameObject.Find("StageMenu");
        StageMenu.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        StageMenu.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 2f);
        StageMenu.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        StageMenu.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        StageMenu.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.5f);

        backgroundImage = GameObject.Find("Title")?.transform.Find("Background")?.gameObject.GetComponent<Image>();
        background2Image = GameObject.Find("StageMenu")?.transform.Find("Background2")?.gameObject.GetComponent<Image>();

        if (backgroundImage != null)
        {
            SetFullScreen(backgroundImage);
        }
        else
        {
            Debug.Log("background 못 찾음");
        }
        if (background2Image != null)
        {
            SetFullScreen(background2Image);
        }
        else
        {
            Debug.Log("background2 못 찾음");
        }
    }

    private void SetFullScreen(Image img)
    {
        RectTransform rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

}
