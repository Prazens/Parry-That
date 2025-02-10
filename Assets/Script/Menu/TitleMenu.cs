using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

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
    private Image Title;
    private GameObject imsi;
    private Image rt_imsi;

    private bool GoStageMenu = false;
    public static bool SwordUpEnd = false;
    public bool MouseControl;   // Inspector 창에서 설정
    public static bool TitlePassed = false;
    public bool isPossibleStage = true;

    public GameObject GameController;

    public GameObject TitleTextObj;
    public TextMeshProUGUI TitleText;
    private Color TitleText_originalColor;

    [SerializeField] private StageMenu stageMenu;

    void Start()
    {
        menuPanel = GameObject.Find("Title").GetComponent<RectTransform>();
        nextPanel = GameObject.Find("StageMenu").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        Title = GameObject.Find("Img_Title").GetComponent<Image>();
        imsi = GameObject.Find("imsi");
        rt_imsi = imsi.GetComponent<Image>();

        GameController = GameObject.Find("GameController");
        TitleTextObj = GameObject.Find("TitleText");
        TitleText = TitleTextObj.GetComponent<TextMeshProUGUI>();

        MenuStartPos = menuPanel.anchoredPosition;
        StageMenuStartPos = nextPanel.anchoredPosition;

        if (TitleText != null)
        {
            TitleText_originalColor = TitleText.color;
        }

        if (TitlePassed)  // 스테이지에서 나왔을 때 스테이지 메뉴창 상태로 위치 설정
        {
            ChangePanelPosition(MenuStartPos, StageMenuStartPos);
            ChangeSwordPosition();
            GoStageMenu = true;
        }
        Debug.Log($"{TitlePassed}");
    }

    void Update()   // 위로 스와이프 하면 타이틀 화면에서 스테이지 선택 화면으로 전환
    {
        if (!GoStageMenu || SwordUpEnd)
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

        if (TitleText != null)
        {
            float alpha = (Mathf.Sin(Time.time * 1f) * 0.35f + 0.65f);
            TitleText.color = new Color(TitleText_originalColor.r, TitleText_originalColor.g, TitleText_originalColor.b, alpha);
        }

        // 현재 플레이 가능한 스테이지 제한
        int[] possibleStages = { 0, 1, 2 };
        isPossibleStage = possibleStages.Contains(StageMenu.currentIndex);

        // 임시코드 for imsi
        rt_imsi.rectTransform.anchoredPosition = Sword.rectTransform.anchoredPosition;
        if (1 <= StageMenu.currentIndex && StageMenu.currentIndex <= 100) imsi.SetActive(true);
        else imsi.SetActive(false);

        imsi.SetActive(false);  // imsi
    }

    private void MouseMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;
            float swipeDistance = startPos.y - endPos.y;
            if (swipeDistance < -swipeThreshold)
            {
                if (GoStageMenu & SwordUpEnd & isPossibleStage)
                {
                    if (Mathf.Abs(startPos.x - endPos.x) >= stageMenu.threshold) return;
                    else stageMenu.SelectStage();
                    // GameController.GetComponent<GameController>().StartStage();
                }
                if (!GoStageMenu)
                {
                    OnSwipeUp();
                    LogoFadeOut();
                    GoStageMenu = true;
                    TitlePassed = true;
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
                    if (GoStageMenu & SwordUpEnd)
                    {
                        if (Mathf.Abs(startPos.x - endPos.x) >= stageMenu.threshold) return;
                        else stageMenu.SelectStage();
                        // GameController.GetComponent<GameController>().StartStage();
                    }
                    if (!GoStageMenu)
                    {
                        OnSwipeUp();
                        LogoFadeOut();
                        GoStageMenu = true;
                        TitlePassed = true;
                    }
                }
            }
        }
    }

    public void OnSwipeUp()
    {
        TitleTextObj.SetActive(false);
        if (!TitlePassed)   // 처음 실행했을 때
        {
            StartCoroutine(SlidePanels(MenuStartPos, StageMenuStartPos));
            StartCoroutine(SwordUp());
        }
    }

    private IEnumerator SlidePanels(Vector2 FirstPanel, Vector2 SecondPanel)
    {
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float elapsedTime = 0;
        Vector2 FirstPanelEnd = new Vector2(FirstPanel.x, FirstPanel.y - canvasRect.rect.height);
        Vector2 SecondPanelEnd = new Vector2(SecondPanel.x, SecondPanel.y - canvasRect.rect.height);

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

    private void ChangePanelPosition(Vector2 FirstPanel, Vector2 SecondPanel)
    {
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 FirstPanelEnd = new Vector2(FirstPanel.x, FirstPanel.y - canvasRect.rect.height);
        Vector2 SecondPanelEnd = new Vector2(SecondPanel.x, SecondPanel.y - canvasRect.rect.height);
        menuPanel.anchoredPosition = FirstPanelEnd;
        nextPanel.anchoredPosition = SecondPanelEnd;
    }

    private IEnumerator SwordUp()
    {
        RectTransform swordRect = Sword.GetComponent<RectTransform>();
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 startPos = swordRect.anchoredPosition;
        float relativeHeight = canvasRect.rect.height ;

        Vector2 endPosUp = new Vector2(startPos.x, startPos.y + relativeHeight);
        Vector2 endPosDown = new Vector2(startPos.x, startPos.y + canvasRect.rect.height - 100);

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
        swordRect.anchoredPosition = endPosUp;

        elapsedTime = 0;
        while (elapsedTime < durationDown)
        {
            float t = elapsedTime / durationDown;
            t = Mathf.SmoothStep(0, 1, t);
            Sword.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(endPosUp, endPosDown, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        swordRect.anchoredPosition = endPosDown;
        SwordUpEnd = true;
        Debug.Log("SwordUpEnd");
    }

    private void ChangeSwordPosition()
    {
        RectTransform swordRect = Sword.GetComponent<RectTransform>();
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 endPosDown = new Vector2(startPos.x, startPos.y + canvasRect.rect.height + 400);
        swordRect.anchoredPosition = endPosDown;
        SwordUpEnd = true;
    }



    [SerializeField] private float fadeDuration;

    private bool isFading = false;

    public void LogoFadeOut()
    {
        if (isFading) return;
        StartCoroutine(FadeEffect());
        Debug.Log("함수 호출 완료");
    }

    private IEnumerator FadeEffect()
    {
        if (Title == null) yield break;

        isFading = true;

        float elapsedTime = 0f;
        Debug.Log("페이드아웃 시작");
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0f);

        isFading = false;
    }

    private void SetAlpha(float alpha)
    {
        if (Title != null)
        {
            Color color = Title.color;
            color.a = alpha;
            Title.color = color;
        }
    }

    // SwordUpEnd = true 되면 칼이 위아래로 약간씩 둥둥 떠다니고 있는 IDLE 상태로

    // 




}
