using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    private Vector2 startPos;
    public float swipeThreshold = 50f;

    public RectTransform menuPanel;
    public RectTransform nextPanel;
    public float slideDuration; // 화면 전환 시간

    private Vector2 MenuStartPos;
    private Vector2 StageMenuStartPos;

    private Image Sword;

    private bool GoStageMenu = false;
    private bool SwordUpEnd = false;
    public bool MouseControl = false;

    public GameObject GameController; 
    void Start()
    {
        menuPanel = GameObject.Find("Title").GetComponent<RectTransform>();
        nextPanel = GameObject.Find("StageMenu").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();

        GameController = GameObject.Find("GameController");

        MenuStartPos = menuPanel.anchoredPosition;
        StageMenuStartPos = nextPanel.anchoredPosition;
    }

    void Update()   // 위로 스와이프 하면 타이틀 화면에서 스테이지 선택 화면으로 전환
    {
        if (!GoStageMenu)
        {
            if (MouseControl)   // 마우스 조작
            {
                MouseMove();
            }
            else    // 터치 조작
            {
                TouchMove();
            }
        }

        if (SwordUpEnd)
        {
            if (MouseControl)   // 마우스 조작
            {
                MouseMove();
            }
            else    // 터치 조작
            {
                TouchMove();
            }
        }
    }

    private void MouseMove()
    {
        if (Input.GetMouseButtonDown(0))        // 터치 조작으로 바꿔야 함. 아직 마우스 조작만 구현
        {
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;
            float swipeDistance = startPos.y - endPos.y;
            if (swipeDistance < -swipeThreshold)
            {
                if (GoStageMenu & SwordUpEnd)
                {
                    GameController.GetComponent<GameController>().StartStage();
                }
                if (!GoStageMenu)
                {
                    OnSwipeUp();
                    GoStageMenu = true;
                }
            }
        }
    }

    private void TouchMove()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 endPos = touch.position;
                float swipeDistance = startPos.y - endPos.y;

                if (swipeDistance < -swipeThreshold)
                {
                    OnSwipeUp();
                    if (GoStageMenu & SwordUpEnd)
                    {
                        GameController.GetComponent<GameController>().StartStage();
                    }
                    if (!GoStageMenu)
                    {
                        OnSwipeUp();
                        GoStageMenu = true;
                    }
                }
            }
        }
    }

    public void OnSwipeUp()
    {
        StartCoroutine(SlidePanels(MenuStartPos, StageMenuStartPos));
        StartCoroutine(SwordUp());
    }

    private IEnumerator SlidePanels(Vector2 FirstPanel, Vector2 SecondPanel)
    {
        float elapsedTime = 0;
        Vector2 FirstPanelEnd = new Vector2(FirstPanel.x, FirstPanel.y - 2160);
        Vector2 SecondPanelEnd = new Vector2(SecondPanel.x, SecondPanel.y - 2160);

        while (elapsedTime < slideDuration)
        {
            menuPanel.anchoredPosition = Vector2.Lerp(FirstPanel, FirstPanelEnd, elapsedTime / slideDuration);
            nextPanel.anchoredPosition = Vector2.Lerp(SecondPanel, SecondPanelEnd, elapsedTime / slideDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        menuPanel.anchoredPosition = FirstPanelEnd;
        nextPanel.anchoredPosition = SecondPanelEnd;
    }

    private IEnumerator SwordUp()
    {
        Vector2 startPos = Sword.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPosUp = new Vector2(startPos.x, startPos.y + Screen.height);
        Vector2 endPosDown = new Vector2(startPos.x, startPos.y + Screen.height);

        float durationUp = 2f;
        float durationDown = 1f;
        float elapsedTime = 0;

        while (elapsedTime < durationUp)
        {
            float t = elapsedTime / durationUp;
            t = Mathf.SmoothStep(0, 1, t);
            Sword.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, endPosUp, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Sword.GetComponent<RectTransform>().anchoredPosition = endPosUp;

        elapsedTime = 0;
        while (elapsedTime < durationDown)
        {
            float t = elapsedTime / durationDown;
            t = Mathf.SmoothStep(0, 1, t);
            Sword.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(endPosUp, endPosDown, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Sword.GetComponent<RectTransform>().anchoredPosition = endPosDown;
        SwordUpEnd = true;
        Debug.Log("SwordUpEnd");
    }

    // SwordUpEnd = true 되면 칼이 위아래로 약간씩 둥둥 떠다니고 있는 IDLE 상태로

    // 




}
