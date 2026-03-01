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
    protected override bool DontDestroy => false;

    private bool EnableStageMenuText = true;
    private GameObject StageMenuTextObj;
    private TextMeshProUGUI StageMenuText;
    private Color StageMenuText_originalColor;
    private float idleTime = 0f;
    private bool isFadingIn = true;
    private float fadeInTimer = 0f;
    private float fadeInStartTime = 0f;

    private Image BlackOverlay;
    [SerializeField] private GameObject BlackOverlayObj;

    public SwordMovement sword;
    private Image swordImage;
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

    private bool isFirstLaunch = true;

    public float height;

    // for debug
    private void Start()
    {
        InitUI();  // 외부에서 호출하도록 변경
    }

    /// <summary>
    /// MenuManager의 초기화 함수
    /// </summary>
    public void InitUI()
    {
        Application.targetFrameRate = 120; // 120프레임
        height = GetComponent<RectTransform>().rect.height;

        // int[] stageInfo = SceneLinkage.stageIndex;
        // stageIndex = SceneLinkage.ConvertToNewStageIndex(SceneLinkage.StageLV);
        stageIndex = StageDBManager.Instance.CurrentStage;  // DB에서 현재 스테이지 정보 불러오기
        // 스테이지에서 나왔을 때 현재 인덱스를 그 스테이지로 설정
        // diskSwipeUI.curIndex = stageIndex;

        diskSwipeUI.InitScrollView(stageIndex[0], stageIndex[1]);
        infoDisplayUI.InitUI(stageIndex);
        if (stageIndex[1] >= 1)
        {
            diffButtonUI.InitUI(stageIndex[1]);
        }
        settingUI.InitUI();

        //RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        StageMenuTextObj = GameObject.Find("StageMenuText");
        StageMenuText = StageMenuTextObj.GetComponent<TextMeshProUGUI>();
        StageMenuTextObj.SetActive(false);
        if (StageMenuText != null)
        {
           StageMenuText_originalColor = StageMenuText.color;
        }

        //imgHistoryRect.anchorMin = new Vector2(0, 0.75f);
        //imgHistoryRect.anchorMax = new Vector2(1, 1);
        //imgHistoryRect.offsetMin = Vector2.zero;
        //imgHistoryRect.offsetMax = Vector2.zero;
        //imgHistoryRect.pivot = new Vector2(0.5f, 1);

        BlackOverlay = BlackOverlayObj.GetComponent<Image>();
        RectTransform BlackOverlayRT = BlackOverlay.GetComponent<RectTransform>();
        BlackOverlayRT.anchorMin = new Vector2(0, 0);
        BlackOverlayRT.anchorMax = new Vector2(1, 1);
        Color originalOverlayColor = BlackOverlay.color;
        BlackOverlay.color = new Color (originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);

        if (isFirstLaunch)
        {
            currentState = MenuState.Title;
            titleUI.InitUI();
            sword.InitUI();
            isFirstLaunch = false;
            titleUI.mainCamera.transform.position = new Vector3(0f, -10f, -10f);  // 임시 하드코딩
        }
        else
        {
            sword.InitUI(true);
            currentState = MenuState.StageSelect;
            SwordUpEnd();
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
            if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1)
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
            StageDBManager.Instance.CurrentStage = stageIndex;
            infoDisplayUI.DisplayInfo(StageDBManager.Instance.CurrentStage);
        }
    }

    /// <summary>
    /// 타이틀 연출 끝나고 칼 올라오는 연출까지 끝났을 때
    /// <para>또는 스테이지를 선택해서 검 올라가는 연출 끝났을 때</para>
    /// </summary>
    /// </summary>
    public void SwordUpEnd()
    {
        // 타이틀에서 스테이지 선택으로 전환
        if (currentState == MenuState.Title)
        {
            currentState = MenuState.StageSelect;
            infoDisplayUI.InitUI(stageIndex);
            diskSwipeUI.StartPreviewSound(stageIndex[0]);
            if (stageIndex[1] >= 1)
            {
                diffButtonUI.InitUI(stageIndex[1]);
            }
            return;
        }

        // 스테이지 선택에서 스테이지 로딩으로 전환
        else if (currentState == MenuState.StageSelect)
        {
            StageDBManager.Instance.CurrentStage = stageIndex;
            SceneManager.LoadScene("Loading");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 안내 문구 활성화
        if (currentState != MenuState.Title && EnableStageMenuText)
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                idleTime = 0f;
                StageMenuTextObj.SetActive(false);
                EnableStageMenuText = false;
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            if (idleTime >= 5f) // 5초 이상 입력 없으면 활성화
            {
                StageMenuTextObj.SetActive(true);
                EnableStageMenuText = true;
            }
        }
        
        // 안내 문구 펄스
        if (StageMenuText != null && EnableStageMenuText)
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

    /// <summary>
    /// 스테이지 시작 시 호출
    /// </summary>
    public void StartStage()
    {
        sword.StartSwordUp(height / 2f, 2f);  // 애니메이션 지속 시간 임시로 하드코딩
        StartCoroutine(StartStageCoroutine(2f));  // 임시로 하드코딩
        BlackOverlayObj.SetActive(true);
    }

    /// <summary>
    /// 검정색 박스 진해지는 연출 코루틴
    /// </summary>
    /// <param name="dur">지속 시간</param>
    public IEnumerator StartStageCoroutine(float dur)
    {
        BlackOverlayObj.SetActive(true);
        float elapsedTime = 0f;

        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / dur);

            float overlayAlpha = (t > 0.8f) ? 1f : Mathf.Clamp01(t * 1.3f);
            BlackOverlay.color = new Color(0f, 0f, 0f, overlayAlpha);

            yield return null; // 다음 프레임까지 대기
        }
    }
}
