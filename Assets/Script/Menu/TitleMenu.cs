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
    public float slideDuration; // ȭ�� ��ȯ �ð�

    private Vector2 MenuStartPos;
    private Vector2 StageMenuStartPos;

    private Image Sword;
    private Image Title;

    private bool GoStageMenu = false;
    public static bool SwordUpEnd = false;
    public bool MouseControl;   // Inspector â���� ����

    public GameObject GameController; 
    void Start()
    {
        menuPanel = GameObject.Find("Title").GetComponent<RectTransform>();
        nextPanel = GameObject.Find("StageMenu").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        Title = GameObject.Find("Img_Title").GetComponent<Image>();

        GameController = GameObject.Find("GameController");

        MenuStartPos = menuPanel.anchoredPosition;
        StageMenuStartPos = nextPanel.anchoredPosition;
    }

    void Update()   // ���� �������� �ϸ� Ÿ��Ʋ ȭ�鿡�� �������� ���� ȭ������ ��ȯ
    {
        if (!GoStageMenu)
        {
            if (MouseControl)   // ���콺 ����
            {
                MouseMove();
            }
            else    // ��ġ ����
            { 
                TouchMove();
            }
        }

        if (SwordUpEnd)
        {
            if (MouseControl)   // ���콺 ����
            {
                MouseMove();
            } 
            else    // ��ġ ����
            {
                TouchMove();
            }
        }
    }

    private void MouseMove()
    {
        if (Input.GetMouseButtonDown(0))        // ��ġ �������� �ٲ�� ��. ���� ���콺 ���۸� ����
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
                    LogoFadeOut();
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
                    if (GoStageMenu & SwordUpEnd)
                    {
                        GameController.GetComponent<GameController>().StartStage();
                    }
                    if (!GoStageMenu)
                    {
                        OnSwipeUp();
                        LogoFadeOut();
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
        RectTransform swordRect = Sword.GetComponent<RectTransform>();
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        Vector2 startPos = swordRect.anchoredPosition;
        float relativeHeight = canvasRect.rect.height ;

        Vector2 endPosUp = new Vector2(startPos.x, startPos.y + relativeHeight);
        Vector2 endPosDown = new Vector2(startPos.x, startPos.y + canvasRect.rect.height);

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



    [SerializeField] private float fadeDuration;

    private bool isFading = false;

    public void LogoFadeOut()
    {
        if (isFading) return;
        StartCoroutine(FadeEffect());
        Debug.Log("�Լ� ȣ�� �Ϸ�");
    }

    private IEnumerator FadeEffect()
    {
        if (Title == null) yield break;

        isFading = true;

        float elapsedTime = 0f;
        Debug.Log("���̵�ƿ� ����");
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

    // SwordUpEnd = true �Ǹ� Į�� ���Ʒ��� �ణ�� �յ� ���ٴϰ� �ִ� IDLE ���·�

    // 




}
