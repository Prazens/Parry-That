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
using DG.Tweening;
using UnityEditor.SceneManagement; // 🔥 DOTween 사용 필수 선언

/// <summary>
/// 스테이지 메뉴 UI 띄우고, UI에 정보 전달, 클릭시 스테이지 실행
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    protected override bool DontDestroy => false;

    [Header("Tutorial Guide")]
    // 🔥 텍스트 관련 변수 싹 다 지우고 손가락 가이드 연결
    public HandTutorialIndicator handGuide; 
    private float idleTime = 0f;
    private bool isShowingGuide = false;

    private Image BlackOverlay;
    [SerializeField] private GameObject BlackOverlayObj;

    public SwordMovement sword;
    private Image swordImage;

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

    public float height;

    private void Start()
    {
        InitUI(); 
    }

    public void InitUI()
    {
        if (StageDBManager.Instance.isFirstLaunch)
        {
            Application.targetFrameRate = 120;
        }
        
        if (transform.parent != null && transform.parent.GetComponent<RectTransform>() != null)
        {
            height = transform.parent.GetComponent<RectTransform>().rect.height;
        }
        else
        {
            RectTransform myRect = GetComponent<RectTransform>();
            height = myRect != null ? myRect.rect.height : Screen.height;
            Debug.LogWarning("MenuManager의 부모 RectTransform을 찾을 수 없어 기본 높이를 사용합니다. MenuManager가 Canvas 안에 있는지 확인해주세요!");
        }

        stageIndex = new int[] { StageSelection.SelectedStageId, (int)StageSelection.SelectedDifficulty }; 

        diskSwipeUI.InitScrollView(stageIndex[0], stageIndex[1]);
        infoDisplayUI.InitUI(stageIndex);

        diffButtonUI.InitUI(stageIndex[1]);
        if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1)
        {
            diffButtonUI.SetVisibility(false);
        }
        else
        {
            diffButtonUI.SetVisibility(true);
        }

        settingUI.InitUI();

        if (handGuide != null)
        {
            handGuide.StopTutorial();
        }

        BlackOverlay = BlackOverlayObj.GetComponent<Image>();
        RectTransform BlackOverlayRT = BlackOverlay.GetComponent<RectTransform>();
        BlackOverlayRT.anchorMin = new Vector2(0, 0);
        BlackOverlayRT.anchorMax = new Vector2(1, 1);
        Color originalOverlayColor = BlackOverlay.color;
        BlackOverlay.color = new Color (originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);
        currentState = MenuState.Title;

        if (StageDBManager.Instance.isFirstLaunch)
        {
            titleUI.InitUI();
            sword.InitUI();
            StageDBManager.Instance.isFirstLaunch = false;
            titleUI.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;  
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, height);  
        }
        else
        {
            titleUI.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -height);  
            transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;  
            sword.InitUI(true);
            SwordUpEnd();
        }
    }

    public void UpdateCurStage(int index, int difficulty = 0)
    {
        bool needUpdate = false;
        if (stageIndex[0] != index)
        {
            stageIndex[0] = index;
            if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1)
            {
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
            StageSelection.SetSelection(stageIndex[0], (Difficulty)stageIndex[1]);
            infoDisplayUI.DisplayInfo(new int[] { StageSelection.SelectedStageId, (int)StageSelection.SelectedDifficulty });
        }
    }

    public void SwordUpEnd()
    {
        if (currentState == MenuState.Title)
        {
            currentState = MenuState.StageSelect;
            
            // 🔥 타이틀에서 칼 뽑고 넘어오면, 이제 칼이 둥둥 떠다니도록 지시!
            sword.StartFloating(); 
            
            infoDisplayUI.InitUI(stageIndex);
            diskSwipeUI.StartPreviewSound(stageIndex[0]);
            // if (stageIndex[1] >= 1)
            // {
            //     diffButtonUI.InitUI(stageIndex[1]);
            // }
            return;
        }
        else if (currentState == MenuState.StageSelect)
        {
            StageSelection.SetSelection(stageIndex[0], (Difficulty)stageIndex[1]);
            SceneManager.LoadScene("CutScene");
        }
    }

    void Update()
    {
        if (handGuide != null)
        {
            // 입력 감지 시 타이머 초기화 및 가이드 숨김
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                idleTime = 0f;
                if (isShowingGuide)
                {
                    handGuide.StopTutorial();
                    isShowingGuide = false;
                }
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            // 5초 이상 아무 입력이 없을 때 가이드 등장!
            if (idleTime >= 5f && !isShowingGuide)
            {
                isShowingGuide = true;

                if (currentState == MenuState.Title)
                {
                    // 🔥 타이틀 창: 위로 스와이프 (칼 뽑기 안내)
                    handGuide.PlaySwipe(new Vector2(0f, 1f));
                }
                else if (currentState == MenuState.StageSelect)
                {
                    // 🔥 스테이지 창: 좌측으로 스와이프 (디스크 넘기기 안내)
                    handGuide.PlaySwipe(new Vector2(0f, 1f));
                }
            }
        }
    }

    /// <summary>
    /// 스테이지 시작 시 호출 (검정 배경 연출)
    /// </summary>
    public void StartStage()
    {
        float dur = 2f;
        BlackOverlayObj.SetActive(true);

        // 검정색 박스 진해지는 연출 (DOTween)
        BlackOverlay.DOFade(1f, dur).SetEase(Ease.OutQuad);

        // 칼 올라가는 연출은 이제 SwordMovement가 스스로 DOTween으로 처리함
        sword.StartSwordUp(height / 2f, dur); 
    }

}