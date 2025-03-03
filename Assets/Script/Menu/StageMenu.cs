using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;

public class StageMenu : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image[] StageImgSet;
    Image Sword;
    [SerializeField] private GameObject[] Stars;

    private float elapsedTime = 0f;
    // 스테이지 점수
    DatabaseManager theDatabase;
    [SerializeField] TextMeshProUGUI txtStageName;
    [SerializeField] TextMeshProUGUI txtStageScore;

    private bool EnableStageMenuText = true;
    GameObject StageMenuTextObj;
    TextMeshProUGUI StageMenuText;
    private Color StageMenuText_originalColor;
    float idleTime = 0f;
    bool isFadingIn = true;
    float fadeInTimer = 0f;
    private float fadeInStartTime = 0f;

    [SerializeField] private GameObject BlackOverlayObj;
    private Image BlackOverlay;

    private string[] StageName = {"0.Tutorial", "1.The First Beat", "2.Echoing Strikes", "3.Beat Master", "4.Final Encore/BOSS" };

    [SerializeField] private GameObject SettingCanvas;
    [SerializeField] private GameObject SettingBackGround;

    void Start()
    {
        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        StageMenuTextObj = GameObject.Find("StageMenuText");
        StageMenuText = StageMenuTextObj.GetComponent<TextMeshProUGUI>();
        StageMenuTextObj.SetActive(false);
        if (StageMenuText != null)
        {
            StageMenuText_originalColor = StageMenuText.color;
        }

        theDatabase = FindObjectOfType<DatabaseManager>();

        imgHistoryRect.anchorMin = new Vector2(0, 0.75f);
        imgHistoryRect.anchorMax = new Vector2(1, 1);
        imgHistoryRect.offsetMin = Vector2.zero;
        imgHistoryRect.offsetMax = Vector2.zero;
        imgHistoryRect.pivot = new Vector2(0.5f, 1);

        BlackOverlay = BlackOverlayObj.GetComponent<Image>();
        RectTransform BlackOverlayRT = BlackOverlay.GetComponent<RectTransform>();
        BlackOverlayRT.anchorMin = new Vector2(0, 0);
        BlackOverlayRT.anchorMax = new Vector2(1, 1);
        Color originalOverlayColor = BlackOverlay.color;
        BlackOverlay.color = new Color (originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);

        // 스테이지에서 나왔을 때 현재 인덱스를 그 스테이지로 설정
        currentIndex = SceneLinkage.StageLV;

        SettingCanvas.GetComponent<Canvas>().sortingOrder = 10;
        SettingBackGround.SetActive(false);

        PPInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (TitleMenu.SwordUpEnd)
        {
            // CD 회전
            StageImgSet[currentIndex].rectTransform.Rotate(0, 0, 1.7f * Time.deltaTime);  

            // 칼 둥둥 떠다니는 느낌
            elapsedTime += Time.deltaTime;
            float newY = Sword.rectTransform.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            Sword.rectTransform.anchoredPosition = new Vector2(Sword.rectTransform.anchoredPosition.x, newY);
        }


        if (TitleMenu.SwordUpEnd & EnableStageMenuText)
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            if (idleTime >= 5f) // 5초 이상 입력 없으면 활성화
            {
                StageMenuTextObj.SetActive(true);
                EnableStageMenuText = false;
            }
        }

        if (StageMenuText != null & !EnableStageMenuText)
        {
            if (isFadingIn)
            {
                // 처음 페이드 인 (0 → 1)
                fadeInTimer += Time.deltaTime / 2f; // 2초 동안 페이드 인
                float alpha = Mathf.Clamp01(fadeInTimer);
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);

                if (fadeInTimer >= 1f)
                {
                    isFadingIn = false;
                    fadeInStartTime = Time.time;
                }
            }
            else
            {
                float elapsedTime = Time.time - fadeInStartTime;
                float alpha = (Mathf.Sin(elapsedTime * 1f + Mathf.PI / 3) * 0.35f + 0.65f);
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);
            }
        }


        // 임시로 update에 구현
        txtStageScore.text = string.Format("{0:#,##0}", theDatabase.score[currentIndex]);
        txtStageName.text = StageName[currentIndex];
        txtStageName.enableWordWrapping = false;  // 자동 줄 바꿈 해제
        txtStageName.overflowMode = TextOverflowModes.Overflow;  // 글자가 넘쳐도 계속 표시
        switch (theDatabase.star[currentIndex])
        {
            case 0:
                Stars[0].SetActive(true);
                Stars[1].SetActive(false);
                Stars[2].SetActive(false);
                Stars[3].SetActive(false);
                break;
            case 1:
                Stars[0].SetActive(false);
                Stars[1].SetActive(true);
                Stars[2].SetActive(false);
                Stars[3].SetActive(false);
                break;
            case 2:
                Stars[0].SetActive(false);
                Stars[1].SetActive(false);
                Stars[2].SetActive(true);
                Stars[3].SetActive(false);
                break;
            case 3:
                Stars[0].SetActive(false);
                Stars[1].SetActive(false);
                Stars[2].SetActive(false);
                Stars[3].SetActive(true);
                break;
            default:
                // Debug.Log("잘못된 데이터베이스 정보(Star)");
                break;
        }
        if (!SettingPanel.activeSelf && TitleMenu.SwordUpEnd)
        {
            SettingIcon.SetActive(true);
        }

    }

    public void SelectStage()
    {
        StartCoroutine(SelectStageCoroutine());
    }
    public IEnumerator SelectStageCoroutine()
    {
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        BlackOverlayObj.SetActive(true);
        Vector2 startPosition = Sword.rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, canvasRect.rect.height * 1.5f);
        float elapsedTime = 0f;
        float duration = 1f; // 애니메이션 지속 시간

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            Sword.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            float overlayAlpha = (t > 0.8f) ? 1f : Mathf.Clamp01(t * 1.3f);
            BlackOverlay.color = new Color(0f, 0f, 0f, overlayAlpha);

            yield return null; // 다음 프레임까지 대기
        }

        SceneLinkage.StageLV = currentIndex;
        SceneManager.LoadScene("Loading");
    }

    // 여기서부터 좌우 스와이프 관련 코드
    [Header("Stage Objects")]
    public List<RectTransform> stageObjects;
    public static int currentIndex = 0;

    public float threshold = 270f;
    private float swipeSpeed = 0.7f;   // 감도
    private float transitionTime = 0.3f; // 애니메이션 시간

    private float screenWidth;
    private bool isDragging = false;

    private float minScale = 0.8f;            // 양옆일 때 최소 스케일
    private float maxScale = 1.0f;            // 중앙일 때 최대 스케일
    private float distanceToFullDark = 600f;  // 중앙에서 이만큼 떨어지면 어둡게
    private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    private Color brightColor = new Color(1f, 1f, 1f, 1f);

    private void Awake()
    {
        screenWidth = Screen.width;

        // 모든 오브젝트를 "현재 인덱스" 기준으로 자리 배치
        UpdateStagePositions();

        // 크기/색상 보정
        UpdateScaleAndColor();

        // "현재, 양옆"만 켜고, 나머지 끔, 없어도 되는 함수
        ActivateOnlyRelevantObjects();
    }

    private void ActivateOnlyRelevantObjects()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            stageObjects[i].gameObject.SetActive(false);
        }

        stageObjects[currentIndex].gameObject.SetActive(true);
        if (currentIndex - 1 >= 0)
            stageObjects[currentIndex - 1].gameObject.SetActive(true);
        if (currentIndex + 1 < stageObjects.Count)
            stageObjects[currentIndex + 1].gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        float deltaX = eventData.delta.x * swipeSpeed;

        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf)
                continue;

            stageObjects[i].anchoredPosition += new Vector2(deltaX, 0);
        }

        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float centerX = stageObjects[currentIndex].anchoredPosition.x;
        float effectiveThreshold = screenWidth * 0.25f; 
        if (Mathf.Abs(centerX) > effectiveThreshold) 
        {
            // 왼쪽 스와이프(centerX < 0) → currentIndex + 1
            if (centerX < 0)
            {
                if (currentIndex < stageObjects.Count - 1)
                {
                    currentIndex++;
                }
            }
            // 오른쪽 스와이프(centerX > 0) → currentIndex - 1
            else
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                }
            }
        }

        // 위치 보정(코루틴)
        StopAllCoroutines();
        StartCoroutine(SmoothMove());
    }


    private IEnumerator SmoothMove()
    {
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < stageObjects.Count; i++)
        {
            startPositions.Add(stageObjects[i].anchoredPosition);
        }
        Dictionary<int, Vector2> targetPos = CalculateTargetPositions();

        ActivateOnlyRelevantObjects();

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            for (int i = 0; i < stageObjects.Count; i++)
            {
                if (!stageObjects[i].gameObject.activeSelf)
                    continue;

                Vector2 sp = startPositions[i];
                Vector2 ep = targetPos[i];
                stageObjects[i].anchoredPosition = Vector2.Lerp(sp, ep, t);
            }

            UpdateScaleAndColor();
            yield return null;
        }

        foreach (var kv in targetPos)
        {
            if (stageObjects[kv.Key].gameObject.activeSelf)
            {
                stageObjects[kv.Key].anchoredPosition = kv.Value;
            }
        }

        UpdateScaleAndColor();
    }

    private Dictionary<int, Vector2> CalculateTargetPositions()
    {
        Dictionary<int, Vector2> result = new Dictionary<int, Vector2>();

        for (int i = 0; i < stageObjects.Count; i++)
        {
            int diff = i - currentIndex;

            Vector2 pos;
            if (diff == 0)
            {
                pos = Vector2.zero;
            }
            else if (diff == 1)
            {
                pos = new Vector2(screenWidth * 0.5f, 0); 
            }
            else if (diff == -1)
            {
                pos = new Vector2(-screenWidth * 0.5f, 0);
            }
            else if (diff > 1)
            {
                pos = new Vector2(screenWidth * 0.5f * diff, 0);
            }
            else
            {
                pos = new Vector2(-screenWidth * 0.5f * Mathf.Abs(diff), 0);
            }

            result[i] = pos;
        }

        return result;
    }


    private void UpdateScaleAndColor()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf)
                continue;

            float dist = Mathf.Abs(stageObjects[i].anchoredPosition.x);
            float factor = Mathf.Clamp01(dist / distanceToFullDark);

            // 스케일 보간
            float scaleVal = Mathf.Lerp(maxScale, minScale, factor);
            stageObjects[i].localScale = new Vector3(scaleVal, scaleVal, 1f);

            // 컬러 보간 
            Image img = stageObjects[i].GetComponent<Image>();
            if (img)
            {
                img.color = Color.Lerp(brightColor, darkColor, factor);
            }
        }
    }

    private void UpdateStagePositions()
    {
        var targets = CalculateTargetPositions();
        foreach (var kv in targets)
        {
            stageObjects[kv.Key].anchoredPosition = kv.Value;
        }
    }

    private bool settingOn = false;
    [SerializeField] private GameObject SettingPanel;
    [SerializeField] private GameObject SettingIcon;
    public void Setting()
    {
        settingOn = settingOn ? false : true;
        if (settingOn)
        {
            SettingBackGround.SetActive(true);
            SettingPanel.SetActive(true);
            SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value = (PlayerPrefs.GetFloat("musicOffset", 2f) - 2f) * 100;
            SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("masterVolume", 1f) * 20;
            SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("bgmVolume", 1f) * 20;
            SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("enemyVolume", 1f) * 20;
            SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("playerVolume", 1f) * 20;
            ChangeMusicOffset();
            ChangeMasterVolume();
            ChangeBGMVolume();
            ChangeEnemyVolume();
            ChangePlayerVolume();
        }
        else
        {
            SettingBackGround.SetActive(false);
            SettingPanel.SetActive(false);
        }
    }

    public void goTutorial()
    {
        // Debug.Log("ChangeMasterVolume");
        SceneLinkage.StageLV = 0;
        SceneManager.LoadScene("Tutorial");
    }

    public void PPInit()
    {
        // Debug.Log($"PPInit, {PlayerPrefs.GetInt("isPPInited", -1)}");
        if (PlayerPrefs.GetInt("isPPInited", 0) != 1)
        {
            PlayerPrefs.SetFloat("musicOffset", 2f);
            PlayerPrefs.SetFloat("masterVolume", 1f);
            PlayerPrefs.SetFloat("bgmVolume", 1f);
            PlayerPrefs.SetFloat("enemyVolume", 1f);
            PlayerPrefs.SetFloat("playerVolume", 1f);
            PlayerPrefs.SetInt("isPPInited", 1);
        }
    }

    public void ChangeMusicOffset()
    {
        // Debug.Log("ChangeMusicOffset");
        int sliderValue = (int)SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("musicOffset", sliderValue / 100f + 2f);
        SettingPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{((sliderValue >= 0) ? "+" : "")}{sliderValue}";
    }

    public void ChangeMasterVolume()
    {
        // Debug.Log("ChangeMasterVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("masterVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeBGMVolume()
    {
        // Debug.Log("ChangeBGMVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("bgmVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeEnemyVolume()
    {
        // Debug.Log("ChangeEnemyVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("enemyVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangePlayerVolume()
    {
        // Debug.Log("ChangePlayerVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("playerVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }
}
