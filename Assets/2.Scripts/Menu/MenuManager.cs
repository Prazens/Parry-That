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
    public StageTitleAnim stageTitleAnim;
    
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

    [Header("Fire Material Effect")]
    public Image fireImage;           // fire 오브젝트의 Image 컴포넌트 연결
    private Material fireMatInstance; // 인스턴스 보관용
    public Color fireNormalColor = Color.white; // 일반 상태 색상
    public Color fireSpecialColor = Color.red;  // 5~7 스테이지용 색상

    [Header("Stage 5~7 Material Effect")]
    public Image targetSpriteImage; // 인스펙터에서 매터리얼이 적용된 이미지 할당
    private Material targetMatInstance; 
    private Tween matTween;
    private bool isCurrentlySpecialRange = false;
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
        diffButtonUI.InitUI(stageIndex[1]);

        if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1 || !JudgeStageUnlock(stageIndex[0], 1))
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

        // BlackOverlay = BlackOverlayObj.GetComponent<Image>();
        // RectTransform BlackOverlayRT = BlackOverlay.GetComponent<RectTransform>();
        // BlackOverlayRT.anchorMin = new Vector2(0, 0);
        // BlackOverlayRT.anchorMax = new Vector2(1, 1);
        // Color originalOverlayColor = BlackOverlay.color;
        // BlackOverlay.color = new Color (originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);

        currentState = MenuState.Title;

        if (StageDBManager.Instance.isFirstLaunch)
        {
            infoDisplayUI.InitUI(stageIndex, StageDBManager.Instance.isFirstLaunch);
            titleUI.InitUI();
            sword.InitUI();
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
        // 🔥 [추가된 부분] 매터리얼 복제 및 초기 상태 세팅
        if (targetSpriteImage != null)
        {
            targetMatInstance = new Material(targetSpriteImage.material);
            targetSpriteImage.material = targetMatInstance;

            isCurrentlySpecialRange = (stageIndex[0] >= 5 && stageIndex[0] <= 7);
            float initialTol = isCurrentlySpecialRange ? 0f : 1f;
            targetMatInstance.SetFloat("_ColorChangeTolerance", initialTol);
        }
        // 🔥 [추가] fire 매터리얼 복제 및 초기 색상 세팅
        if (fireImage != null)
        {
            fireMatInstance = new Material(fireImage.material);
            fireImage.material = fireMatInstance;

            bool isSpecial = (stageIndex[0] >= 5 && stageIndex[0] <= 7);
            Color initialColor = isSpecial ? fireSpecialColor : fireNormalColor;
            
            // All In 1 Sprite의 Hit Color 프로퍼티명은 보통 "_HitEffectColor"입니다.
            fireMatInstance.SetColor("_HitEffectColor", initialColor);
        }
    }

    public bool JudgeStageUnlock(int index, int difficulty)
    {
        if (index == 0)
        // 튜토리얼은 항상 잠금 해제된 상태로 시작
        {
            return true;
        }

        if (difficulty == 0)
        {
            return StageDBManager.Instance.stageCompletion[index - 1][0];
        }
        else if (difficulty == 1)
        {
            return StageDBManager.Instance.stageCompletion[index][0];
        }
        return false;
    }

    public void UpdateCurStage(int index, int difficulty = 0)
    {
        bool needUpdate = false;
        if (stageIndex[0] != index)  // 스테이지 인덱스가 바뀌었을 때만 업데이트
        {
            stageIndex[0] = index;
            
            if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1 || !JudgeStageUnlock(stageIndex[0], 1))
            {
                diffButtonUI.SetVisibility(false);
                difficulty = 0;  // 난이도 인덱스도 0으로 초기화
                diffButtonUI.UpdateButtonState(0);
            }
            else
            {
                diffButtonUI.SetVisibility(true);
            }

            needUpdate = true;
        }

        if (stageIndex[1] != difficulty)  // 난이도 인덱스가 바뀌었을 때만 업데이트
        {
            stageIndex[1] = difficulty;
            diskSwipeUI.UpdateDifficulty(stageIndex[1]);

            needUpdate = true;
        }

        if (needUpdate)
        {
            StageSelection.SetSelection(stageIndex[0], (Difficulty)stageIndex[1]);
            infoDisplayUI.InitUI(new int[] { StageSelection.SelectedStageId, (int)StageSelection.SelectedDifficulty });
            
            // 🔥 스테이지가 변경되었으니 타이틀 애니메이션 교체 실행!
            // // InfoDisplayUI에서 하던 이름 텍스트 변경을 이 멋진 연출이 대신하게 됩니다.
            // if (stageTitleAnim != null)
            // {
            //     stageTitleAnim.ChangeTitle(StageDBManager.Instance.StageName[stageIndex[0]]);
            // }
        }
        // 🔥 [완벽 수정본] 5~7 스테이지 구간 진입/이탈 감지 (타이틀과 불꽃을 동시에 처리!)
        bool isSpecial = (index >= 5 && index <= 7);

        // 상태가 변했을 때만 (진입할 때 1번, 이탈할 때 1번) 실행
        if (isSpecial != isCurrentlySpecialRange)
        {
            isCurrentlySpecialRange = isSpecial; // 여기서 상태를 딱 한 번만 최신화

            // 1. 타이틀 매터리얼 (관용도 1 <-> 0 전환)
            if (targetMatInstance != null)
            {
                matTween?.Kill(); 
                float targetTol = isSpecial ? 0f : 1f;
                matTween = targetMatInstance.DOFloat(targetTol, "_ColorChangeTolerance", 0.5f).SetEase(Ease.InOutQuad);
            }

            // 2. 불꽃 매터리얼 (일반 색상 <-> 특수 색상 전환)
            if (fireMatInstance != null)
            {
                // DOTween이 알아서 현재 진행 중인 Color 트윈을 덮어씌우고 부드럽게 돌아갑니다.
                Color targetColor = isSpecial ? fireSpecialColor : fireNormalColor;
                fireMatInstance.DOColor(targetColor, "_HitEffectColor", 0.5f).SetEase(Ease.InOutQuad);
            }
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
            // if (stageTitleAnim != null)
            // {
            //     stageTitleAnim.ChangeTitle(StageDBManager.Instance.StageName[stageIndex[0]]);
            // }
            // if (stageIndex[1] >= 1)
            // {
            //     diffButtonUI.InitUI(stageIndex[1]);
            // }
            return;
        }
        else if (currentState == MenuState.StageSelect)
        {
            StageSelection.SetSelection(stageIndex[0], (Difficulty)stageIndex[1]);
            CutSceneSelection.SetSelection(stageIndex[0], CutSceneCategory.Prologue);
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
        if (!JudgeStageUnlock(stageIndex[0], stageIndex[1]))
        {
            // 잠긴 스테이지
            Debug.Log("잠긴 스테이지입니다.");
            return;
        }

        diskSwipeUI.StopScroll(); // 스테이지 시작 시 디스크 스와이프 잠금
        settingUI.gameObject.SetActive(false); // 스테이지 시작 시 세팅 버튼 숨김

        float dur = 2f;
        BlackOverlayObj.SetActive(true);
        BlackOverlay = BlackOverlayObj.GetComponent<Image>();

        // 검정색 박스 진해지는 연출 (DOTween)
        BlackOverlay.DOFade(1f, dur).SetEase(Ease.OutQuad);

        // 칼 올라가는 연출은 이제 SwordMovement가 스스로 DOTween으로 처리함
        sword.StartSwordUp(height / 2f, dur); 
    }

}