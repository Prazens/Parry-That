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
    public float slideDuration; // ȭ�� ��ȯ �ð�

    private Vector2 MenuStartPos;
    private Vector2 StageMenuStartPos;

    private Image Sword;
    private Image Title;
    private GameObject imsi;
    private Image rt_imsi;

    private bool GoStageMenu = false;
    public static bool SwordUpEnd = false;
    public bool MouseControl;   // Inspector â���� ����
    public static bool TitlePassed = false;
    public bool isPossibleStage = true;

    public GameObject GameController;

    public GameObject TitleTextObj;
    public TextMeshProUGUI TitleText;
    private Color TitleText_originalColor;

    private AudioSource AudioSource;
    public AudioClip SwordUpSound;
    public AudioClip StageSelectSound;

    [SerializeField] private StageMenu stageMenu;

    void Start()
    {

        Application.targetFrameRate = 120; // ������ 120 ����

        menuPanel = GameObject.Find("Title").GetComponent<RectTransform>();
        nextPanel = GameObject.Find("StageMenu").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        Title = GameObject.Find("Img_Title").GetComponent<Image>();
        imsi = GameObject.Find("imsi");
        rt_imsi = imsi.GetComponent<Image>();
        AudioSource = GetComponent<AudioSource>();

        GameController = GameObject.Find("GameController");
        TitleTextObj = GameObject.Find("TitleText");
        TitleText = TitleTextObj.GetComponent<TextMeshProUGUI>();

        MenuStartPos = menuPanel.anchoredPosition;
        StageMenuStartPos = nextPanel.anchoredPosition;

        if (TitleText != null)
        {
            TitleText_originalColor = TitleText.color;
        }

        if (TitlePassed)  // ������������ ������ �� �������� �޴�â ���·� ��ġ ����
        {
            ChangePanelPosition(MenuStartPos, StageMenuStartPos);
            ChangeSwordPosition();
            GoStageMenu = true;
        }
        Debug.Log($"{TitlePassed}");
    }

    void Update()   // ���� �������� �ϸ� Ÿ��Ʋ ȭ�鿡�� �������� ���� ȭ������ ��ȯ
    {
        if (!GoStageMenu || SwordUpEnd)
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

        if (TitleText != null)
        {
            float alpha = (Mathf.Sin(Time.time * 1f) * 0.35f + 0.65f);
            TitleText.color = new Color(TitleText_originalColor.r, TitleText_originalColor.g, TitleText_originalColor.b, alpha);
        }

        // ���� �÷��� ������ �������� ����
        int[] possibleStages = { 0, 1, 2, 3 };
        isPossibleStage = possibleStages.Contains(StageMenu.currentIndex);

        // �ӽ��ڵ� for imsi
        rt_imsi.rectTransform.anchoredPosition = Sword.rectTransform.anchoredPosition;
        // if (1 <= StageMenu.currentIndex && StageMenu.currentIndex <= 100) imsi.SetActive(true);
        // else imsi.SetActive(false);

        imsi.SetActive(false);  // imsi
        if (StageMenu.currentIndex == 4) imsi.SetActive(true);

        //testScene
        // 마우스 클릭 체크
        if (Input.GetMouseButtonDown(0))
        {
            RegisterClick();
        }

        // 터치 입력 체크 (모바일)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    RegisterClick();
                }
            }
        }
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
                    else
                    {
                        AudioSource.clip = StageSelectSound;
                        AudioSource.Play();
                        stageMenu.SelectStage();
                    }
                    // GameController.GetComponent<GameController>().StartStage();
                }
                if (!GoStageMenu)
                {
                    OnSwipeUp();
                    LogoFadeOut();
                    AudioSource.clip = SwordUpSound;
                    AudioSource.Play();
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
                        else
                        {
                            AudioSource.clip = StageSelectSound;
                            AudioSource.Play();
                            stageMenu.SelectStage();
                        }
                        // GameController.GetComponent<GameController>().StartStage();
                    }
                    if (!GoStageMenu)
                    {
                        OnSwipeUp();
                        LogoFadeOut();
                        AudioSource.clip = SwordUpSound;
                        AudioSource.Play();
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
        if (!TitlePassed)   // ó�� �������� ��
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


    // testScene

    // 필요한 클릭 수와 시간 창을 설정
    public int requiredClicks = 5;
    public float timeWindow = 1f;

    // 클릭 또는 터치가 발생한 시간을 저장할 리스트
    private List<float> clickTimes = new List<float>();

    void RegisterClick()
    {
        float currentTime = Time.time;
        clickTimes.Add(currentTime);

        // 1초(timeWindow)보다 오래된 클릭 기록은 제거합니다.
        clickTimes.RemoveAll(time => currentTime - time > timeWindow);

        // 1초 안에 필요한 클릭 수 이상이면 testScene으로 전환합니다.
        if (clickTimes.Count >= requiredClicks)
        {
            Debug.Log("롱노트 테스트맵 이동중...");
            SceneManager.LoadScene("testScene");
        }
    }
}
