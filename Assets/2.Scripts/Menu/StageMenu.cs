using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageMenu : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Stage UI")]
    [SerializeField] private Image[] StageImgSet;
    [SerializeField] private GameObject[] Stars;

    private Image Sword;
    private float elapsedTime = 0f;
    public float threshold = 270f;


    // Database
    private DatabaseManager theDatabase;
    [SerializeField] private TextMeshProUGUI txtStageName;
    [SerializeField] private TextMeshProUGUI txtStageScore;

    // Idle text
    private bool EnableStageMenuText = true;
    private GameObject StageMenuTextObj;
    private TextMeshProUGUI StageMenuText;
    private Color StageMenuText_originalColor;
    private float idleTime = 0f;
    private bool isFadingIn = true;
    private float fadeInTimer = 0f;
    private float fadeInStartTime = 0f;

    [Header("Fade Overlay")]
    [SerializeField] private GameObject BlackOverlayObj;
    private Image BlackOverlay;

    // UI 표시용 이름(현재 인덱스 기준)
    private readonly string[] StageName =
    {
        "Tutorial",
        "1.The First Beat",
        "2.Echoing Strikes",
        "3.Beat Master",
        "4.Final Encore (Normal)",
        "4.Final Encore (Hard)",
        "Epilogue"
    };

    [Header("Settings")]
    [SerializeField] private GameObject SettingCanvas;
    [SerializeField] private GameObject SettingBackGround;
    [SerializeField] private GameObject SettingPanel;
    [SerializeField] private GameObject SettingIcon;

    [Header("Mode Change")]
    [SerializeField] private GameObject modeChageButton;
    private static bool modeChgButtonAble = false;
    private static bool modeChgButtonEnable = false;
    [SerializeField] private Sprite normalButton;
    [SerializeField] private Sprite hardButton;
    [SerializeField] private GameObject modeButtonObj;

    // Swipe
    [Header("Stage Objects")]
    public List<RectTransform> stageObjects;
    public static int currentIndex = 1;

    private float swipeSpeed = 0.7f;
    private float transitionTime = 0.3f;
    private float screenWidth;

    private float minScale = 0.8f;
    private float maxScale = 1.0f;
    private float distanceToFullDark = 600f;
    private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    private Color brightColor = new Color(1f, 1f, 1f, 1f);

    private bool settingOn = false;

    private void Awake()
    {
        screenWidth = Screen.width;

        UpdateStagePositions();
        UpdateScaleAndColor();
        ActivateOnlyRelevantObjects();
    }

    private void Start()
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
        BlackOverlay.color = new Color(originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);

        // 모드 버튼 스프라이트 복원
        if (TitleMenu.TitlePassed)
        {
            modeButtonObj.GetComponent<Image>().sprite = SceneLinkage.isNormal ? normalButton : hardButton;
        }

        SettingCanvas.GetComponent<Canvas>().sortingOrder = 10;
        SettingBackGround.SetActive(false);

        PPInit();

        UpdateStagePositions();
        UpdateScaleAndColor();
        ActivateOnlyRelevantObjects();
    }

    private void Update()
    {
        if (TitleMenu.SwordUpEnd)
        {
            StageImgSet[currentIndex].rectTransform.Rotate(0, 0, 1.7f * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            float newY = Sword.rectTransform.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            Sword.rectTransform.anchoredPosition = new Vector2(Sword.rectTransform.anchoredPosition.x, newY);
        }

        // Idle text
        if (TitleMenu.SwordUpEnd & EnableStageMenuText)
        {
            if (Input.anyKey || Input.GetMouseButton(0)) idleTime = 0f;
            else idleTime += Time.deltaTime;

            if (idleTime >= 5f)
            {
                StageMenuTextObj.SetActive(true);
                EnableStageMenuText = false;
            }
        }

        if (StageMenuText != null & !EnableStageMenuText)
        {
            if (isFadingIn)
            {
                fadeInTimer += Time.deltaTime / 2f;
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
                float t = Time.time - fadeInStartTime;
                float alpha = (Mathf.Sin(t * 1f + Mathf.PI / 3) * 0.35f + 0.65f);
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);
            }
        }

        // 1~3에서만 Normal/Hard 버튼 노출
        if (currentIndex == 1 || currentIndex == 2 || currentIndex == 3)
        {
            if (modeChgButtonAble) modeChageButton.SetActive(true);
        }
        else
        {
            if (modeChgButtonAble) modeChageButton.SetActive(false);
        }

        // 선택(미리보기) 계산
        GetPreviewSelection(out int stageId, out Difficulty difficulty, out int legacyStageLv);

        // 레거시 유지: 다른 스크립트들이 StageLV를 계속 볼 수 있게
        SceneLinkage.StageLV = legacyStageLv;

        // 점수/별 표시(레거시 인덱스 기반)
        if (theDatabase != null && theDatabase.score != null && legacyStageLv >= 0 && legacyStageLv < theDatabase.score.Length)
        {
            txtStageScore.text = string.Format("{0:#,##0}", theDatabase.score[legacyStageLv]);
        }
        else
        {
            txtStageScore.text = "";
        }

        txtStageName.text = (currentIndex >= 0 && currentIndex < StageName.Length) ? StageName[currentIndex] : "";
        txtStageName.enableWordWrapping = false;
        txtStageName.overflowMode = TextOverflowModes.Overflow;

        int starValue = 0;
        if (theDatabase != null && theDatabase.star != null && legacyStageLv >= 0 && legacyStageLv < theDatabase.star.Length)
        {
            starValue = theDatabase.star[legacyStageLv];
        }
        ApplyStarUI(starValue);

        // Tutorial / Epilogue는 점수/별 숨김
        if (currentIndex == 0 || currentIndex == 6)
        {
            txtStageScore.text = "";
            Stars[0].SetActive(false);
            Stars[1].SetActive(false);
            Stars[2].SetActive(false);
            Stars[3].SetActive(false);
        }

        if (!SettingPanel.activeSelf && TitleMenu.SwordUpEnd)
        {
            SettingIcon.SetActive(true);
            if (!modeChgButtonEnable)
            {
                modeChgButtonEnable = true;
                modeChgButtonAble = true;
            }
        }
    }

    public void SelectStage()
    {
        StopAllCoroutines();
        StartCoroutine(SelectStageCoroutine());
    }

    public IEnumerator SelectStageCoroutine()
    {
        RectTransform canvasRect = Sword.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        BlackOverlayObj.SetActive(true);
        Vector2 startPosition = Sword.rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, canvasRect.rect.height * 1.5f);

        float animationTime = 0f;
        float duration = 1f;

        while (animationTime < duration)
        {
            animationTime += Time.deltaTime;
            float t = Mathf.Clamp01(animationTime / duration);

            Sword.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            float overlayAlpha = (t > 0.8f) ? 1f : Mathf.Clamp01(t * 1.3f);
            BlackOverlay.color = new Color(0f, 0f, 0f, overlayAlpha);

            yield return null;
        }

        // 여기서 선택 확정(SetSelection) + 레거시 StageLV도 세팅
        GetPreviewSelection(out int stageId, out Difficulty difficulty, out int legacyStageLv);

        StageSelection.SetSelection(stageId, difficulty);
        SceneLinkage.StageLV = legacyStageLv;

        // 통합 흐름: Loading → (SceneLoad가) CutScene/Stage로 라우팅
        SceneManager.LoadScene("Loading");
    }

    // =========================
    // Selection 규칙 + 레거시 인덱스 변환
    // =========================

    private void GetPreviewSelection(out int stageId, out Difficulty difficulty, out int legacyStageLv)
    {
        // UI 인덱스(currentIndex) 규칙:
        // 0 Tutorial (stageId=0, Normal)
        // 1~3 Stage1~3 (difficulty는 SceneLinkage.isNormal로)
        // 4 FinalEncore Normal (stageId=4, Normal)
        // 5 FinalEncore Hard   (stageId=4, Hard)
        // 6 Epilogue (stageId=6, Normal)

        if (currentIndex == 0)
        {
            stageId = 0;
            difficulty = Difficulty.Normal;
            legacyStageLv = 0;
            return;
        }

        if (currentIndex == 4)
        {
            stageId = 4;
            difficulty = Difficulty.Normal;
            legacyStageLv = 4;
            return;
        }

        if (currentIndex == 5)
        {
            stageId = 4;
            difficulty = Difficulty.Hard;
            legacyStageLv = 5;
            return;
        }

        if (currentIndex == 6)
        {
            stageId = 6;
            difficulty = Difficulty.Normal;
            legacyStageLv = 6;
            return;
        }

        // 1~3
        stageId = currentIndex;
        difficulty = SceneLinkage.isNormal ? Difficulty.Normal : Difficulty.Hard;

        // 레거시 규칙: Stage1~3 Hard는 +6
        legacyStageLv = (difficulty == Difficulty.Hard) ? stageId + 6 : stageId;
    }

    private void ApplyStarUI(int starValue)
    {
        switch (starValue)
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
            case 4:
            case 5:
                Stars[0].SetActive(false);
                Stars[1].SetActive(false);
                Stars[2].SetActive(false);
                Stars[3].SetActive(true);
                break;
            default:
                break;
        }
    }

    // =========================
    // Swipe
    // =========================

    private void ActivateOnlyRelevantObjects()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            stageObjects[i].gameObject.SetActive(false);
        }

        stageObjects[currentIndex].gameObject.SetActive(true);
        if (currentIndex - 1 >= 0) stageObjects[currentIndex - 1].gameObject.SetActive(true);
        if (currentIndex + 1 < stageObjects.Count) stageObjects[currentIndex + 1].gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.delta.x * swipeSpeed;

        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf) continue;
            stageObjects[i].anchoredPosition += new Vector2(deltaX, 0);
        }

        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float centerX = stageObjects[currentIndex].anchoredPosition.x;
        float effectiveThreshold = screenWidth * 0.25f;

        if (Mathf.Abs(centerX) > effectiveThreshold)
        {
            if (centerX < 0)
            {
                if (currentIndex < stageObjects.Count - 1) currentIndex++;
            }
            else
            {
                if (currentIndex > 0) currentIndex--;
            }
        }

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
                if (!stageObjects[i].gameObject.activeSelf) continue;

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
            if (diff == 0) pos = Vector2.zero;
            else if (diff == 1) pos = new Vector2(screenWidth * 0.5f, 0);
            else if (diff == -1) pos = new Vector2(-screenWidth * 0.5f, 0);
            else if (diff > 1) pos = new Vector2(screenWidth * 0.5f * diff, 0);
            else pos = new Vector2(-screenWidth * 0.5f * Mathf.Abs(diff), 0);

            result[i] = pos;
        }

        return result;
    }

    private void UpdateScaleAndColor()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf) continue;

            float dist = Mathf.Abs(stageObjects[i].anchoredPosition.x);
            float factor = Mathf.Clamp01(dist / distanceToFullDark);

            float scaleVal = Mathf.Lerp(maxScale, minScale, factor);
            stageObjects[i].localScale = new Vector3(scaleVal, scaleVal, 1f);

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

    // =========================
    // Settings / Mode
    // =========================

    public void Setting()
    {
        settingOn = !settingOn;

        if (settingOn)
        {
            SettingBackGround.SetActive(true);
            SettingPanel.SetActive(true);

            SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value =
                (PlayerPrefs.GetFloat("musicOffset", 2f) - 2f) * 100;

            SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value =
                PlayerPrefs.GetFloat("masterVolume", 1f) * 20;

            SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value =
                PlayerPrefs.GetFloat("bgmVolume", 1f) * 20;

            SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value =
                PlayerPrefs.GetFloat("enemyVolume", 1f) * 20;

            SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value =
                PlayerPrefs.GetFloat("playerVolume", 1f) * 20;

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

    public void ModeChage()
    {
        Button button = modeButtonObj.GetComponent<Button>();
        if (button == null) return;

        SceneLinkage.isNormal = !SceneLinkage.isNormal;
        modeButtonObj.GetComponent<Image>().sprite = SceneLinkage.isNormal ? normalButton : hardButton;
    }

    public void goTutorial()
    {
        StageSelection.SetSelection(0, Difficulty.Normal);
        SceneLinkage.StageLV = 0;
        SceneManager.LoadScene("Loading");
    }

    public void PPInit()
    {
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
        int sliderValue = (int)SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("musicOffset", sliderValue / 100f + 2f);
        SettingPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            $"{((sliderValue >= 0) ? "+" : "")}{sliderValue}";
    }

    public void ChangeMasterVolume()
    {
        int sliderValue = (int)SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("masterVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            $"{sliderValue * 5}%";
    }

    public void ChangeBGMVolume()
    {
        int sliderValue = (int)SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("bgmVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            $"{sliderValue * 5}%";
    }

    public void ChangeEnemyVolume()
    {
        int sliderValue = (int)SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("enemyVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            $"{sliderValue * 5}%";
    }

    public void ChangePlayerVolume()
    {
        int sliderValue = (int)SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("playerVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            $"{sliderValue * 5}%";
    }
}
