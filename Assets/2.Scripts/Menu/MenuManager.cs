using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Data;

/// <summary>
/// 스테이지 메뉴 UI 띄우고, UI에 정보 전달, 클릭시 스테이지 실행
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    Image Sword;

    private float elapsedTime = 0f;

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

    // public GameObject modeChageButton;

    [Header("UI Links")]
    public TitleUI titleUI;
    public DiskSwipeUI diskSwipeUI;
    public InfoDisplayUI infoDisplayUI;
    public SettingUI settingUI;
    public DiffButtonUI diffButtonUI;

    public enum MenuState
    {
        Title,
        StageSelect,
        Settings,
        None
    }
    public MenuState currentState = MenuState.None;

    public int[] stageIndex = new int[] { 0, 0 };

    public void InitUI()
    {
        // int[] stageInfo = SceneLinkage.stageIndex;
        stageIndex = SceneLinkage.ConvertToNewStageIndex(SceneLinkage.StageLV);
        // 스테이지에서 나왔을 때 현재 인덱스를 그 스테이지로 설정
        // diskSwipeUI.curIndex = stageIndex;

        currentState = MenuState.StageSelect;
        diskSwipeUI.InitScrollView();
        infoDisplayUI.InitUI(stageIndex);
        if (stageIndex[1] >= 1)
        {
            diffButtonUI.InitUI(stageIndex[1]);
        }
        settingUI.InitUI();

        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        StageMenuTextObj = GameObject.Find("StageMenuText");
        StageMenuText = StageMenuTextObj.GetComponent<TextMeshProUGUI>();
        StageMenuTextObj.SetActive(false);
        if (StageMenuText != null)
        {
            StageMenuText_originalColor = StageMenuText.color;
        }


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

        if (!TitleUI.TitlePassed)
        {
            currentState = MenuState.Title;
            titleUI.InitUI();
        }
    }

    /// <summary>
    /// 현재 스테이지 정보 업데이트
    /// <para>DiskSwipeUI, DiffButtonUI에서 정보 갱신받음</para>
    /// <para>DiskSwipeUI, DiffButtonUI, InfoDisplayUI를 갱신함</para>   
    /// </summary>
    /// <param name="index">스테이지 인덱스 번호만</param>
    /// <param name="difficulty">난이도 인덱스</param>
    public void UpdateCurStage(int index, int difficulty = 0)
    {
        bool needUpdate = false;
        if (stageIndex[0] != index)
        {
            stageIndex[0] = index;
            if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 0)
            {
                // 난이도 없는 스테이지로 전환 시
                diffButtonUI.SetVisibility(false);
            }
            else
            {
                diffButtonUI.SetVisibility(true);
            }

            needUpdate = true;
        }

        if (stageIndex[1] != difficulty)
        {
            stageIndex[1] = difficulty;
            diskSwipeUI.UpdateDifficulty(stageIndex[1]);

            needUpdate = true;
        }

        if (needUpdate)
        {
            infoDisplayUI.DisplayInfo(stageIndex);
        }
    }

    public void WhenTitleEnd()
    {
        currentState = MenuState.StageSelect;
        diskSwipeUI.InitScrollView();
        infoDisplayUI.InitUI(stageIndex);
        if (stageIndex[1] >= 1)
        {
            diffButtonUI.InitUI(stageIndex[1]);
        }
    }

    public void SwordUpEnd()
    {
        currentState = MenuState.StageSelect;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != MenuState.Title)
        {
            // 칼 둥둥 떠다니는 느낌
            elapsedTime += Time.deltaTime;
            float newY = Sword.rectTransform.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            Sword.rectTransform.anchoredPosition = new Vector2(Sword.rectTransform.anchoredPosition.x, newY);
        }


        if (currentState != MenuState.Title & EnableStageMenuText)
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
                float alpha = Mathf.Sin(elapsedTime * 1f + Mathf.PI / 3) * 0.35f + 0.65f;
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);
            }
        }

        // if (diskSwipeUI.currentIndex == 1
        //     || diskSwipeUI.currentIndex == 2
        //     || diskSwipeUI.currentIndex == 3)
        // {
        //     SceneLinkage.StageLV = SceneLinkage.isEasy ? diskSwipeUI.currentIndex + 6 : diskSwipeUI.currentIndex;
        //     // if (modeChgButtonAble) modeChageButton.SetActive(true);
        // }
        // else
        // {
        //     SceneLinkage.StageLV = diskSwipeUI.currentIndex;
        //     // if (modeChgButtonAble) modeChageButton.SetActive(false);
        // }
        
    }

    public void StartStage()
    {
        StartCoroutine(StartStageCoroutine());
    }

    public IEnumerator StartStageCoroutine()
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
        SceneManager.LoadScene("Loading");
    }



}
