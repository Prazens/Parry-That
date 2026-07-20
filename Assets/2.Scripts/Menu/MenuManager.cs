using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Data;
using DG.Tweening;

/// <summary>
/// 스테이지 메뉴 UI 띄우고, UI에 정보 전달, 클릭시 스테이지 실행
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    protected override bool DontDestroy => false;

    [Header("Tutorial Guide")]
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

    [Header("Stage 5~7 Purchase")]
    [SerializeField] private GameObject purchasePanelPrefab;
    private GameObject purchasePanelInstance;
    private Button purchaseButton;
    
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
    public Image fireImage;           
    private Material fireMatInstance; 
    public Color fireNormalColor = Color.white; 
    public Color fireSpecialColor = Color.red;  

    [Header("Stage 5~7 Material Effect")]
    public Image targetSpriteImage; 
    private Material targetMatInstance; 
    private Tween matTween;
    private bool isCurrentlySpecialRange = false;

    private void OnEnable()
    {
        StagePackPurchaseManager.PurchaseStateChanged += RefreshPurchasePanel;
    }

    private void OnDisable()
    {
        StagePackPurchaseManager.PurchaseStateChanged -= RefreshPurchasePanel;
    }

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
        CreatePurchasePanel();

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

        if (targetSpriteImage != null)
        {
            targetMatInstance = new Material(targetSpriteImage.material);
            targetSpriteImage.material = targetMatInstance;

            isCurrentlySpecialRange = (stageIndex[0] >= 5 && stageIndex[0] <= 7);
            float initialTol = isCurrentlySpecialRange ? 0f : 1f;
            targetMatInstance.SetFloat("_ColorChangeTolerance", initialTol);
        }

        if (fireImage != null)
        {
            fireMatInstance = new Material(fireImage.material);
            fireImage.material = fireMatInstance;

            bool isSpecial = (stageIndex[0] >= 5 && stageIndex[0] <= 7);
            Color initialColor = isSpecial ? fireSpecialColor : fireNormalColor;
            
            fireMatInstance.SetColor("_HitEffectColor", initialColor);
        }

        RefreshPurchasePanel();
    }

    public bool JudgeStageUnlock(int index, int difficulty)
    {
        return true; 
    }

    public void UpdateCurStage(int index, int difficulty = 0)
    {
        bool needUpdate = false;
        if (stageIndex[0] != index)  
        {
            stageIndex[0] = index;
            
            if (StageDBManager.Instance.diffNumbers[stageIndex[0]] == 1 || !JudgeStageUnlock(stageIndex[0], 1))
            {
                diffButtonUI.SetVisibility(false);
                difficulty = 0;  
                diffButtonUI.UpdateButtonState(0);
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
            infoDisplayUI.InitUI(new int[] { StageSelection.SelectedStageId, (int)StageSelection.SelectedDifficulty });
        }

        RefreshPurchasePanel();

        bool isSpecial = (index >= 5 && index <= 7);

        if (isSpecial != isCurrentlySpecialRange)
        {
            isCurrentlySpecialRange = isSpecial; 

            if (targetMatInstance != null)
            {
                matTween?.Kill(); 
                float targetTol = isSpecial ? 0f : 1f;
                matTween = targetMatInstance.DOFloat(targetTol, "_ColorChangeTolerance", 0.5f).SetEase(Ease.InOutQuad);
            }

            if (fireMatInstance != null)
            {
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
            RefreshPurchasePanel();
            
            sword.StartFloating(); 
            
            infoDisplayUI.InitUI(stageIndex);
            diskSwipeUI.StartPreviewSound(stageIndex[0]);
            return;
        }
        else if (currentState == MenuState.StageSelect)
        {
            if (IsSelectedStagePurchaseLocked())
            {
                RefreshPurchasePanel();
                return;
            }

            StageSelection.SetSelection(stageIndex[0], (Difficulty)stageIndex[1]);
            CutSceneSelection.SetSelection(stageIndex[0], CutSceneCategory.Prologue);
            SceneManager.LoadScene("CutScene");
        }
    }

    void Update()
    {
        if (handGuide != null)
        {
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

            if (idleTime >= 5f && !isShowingGuide)
            {
                isShowingGuide = true;

                if (currentState == MenuState.Title)
                {
                    handGuide.PlaySwipe(new Vector2(0f, 1f));
                }
                else if (currentState == MenuState.StageSelect)
                {
                    handGuide.PlaySwipe(new Vector2(0f, 1f));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    public void StartStage()
    {
        if (IsSelectedStagePurchaseLocked())
        {
            RefreshPurchasePanel();
            return;
        }

        if (!JudgeStageUnlock(stageIndex[0], stageIndex[1]))
        {
            Debug.Log("잠긴 스테이지입니다.");
            return;
        }

        diskSwipeUI.StopScroll(); 
        settingUI.gameObject.SetActive(false); 

        float dur = 2f;
        BlackOverlayObj.SetActive(true);
        BlackOverlay = BlackOverlayObj.GetComponent<Image>();

        BlackOverlay.DOFade(1f, dur).SetEase(Ease.OutQuad);
        sword.StartSwordUp(height / 2f, dur); 
    }

    public void RefreshPurchasePanel()
    {
        if (purchasePanelInstance == null) return;

        bool shouldShow = currentState == MenuState.StageSelect && IsSelectedStagePurchaseLocked();
        purchasePanelInstance.SetActive(shouldShow);

        if (purchaseButton != null)
        {
            // 상업/비상업 모드 동적 변화에 대응하기 위해 버튼 세팅을 리프레시 때마다 갱신해줍니다.
            SetupPurchaseButtonListeners();

            StagePackPurchaseManager purchaseManager = StagePackPurchaseManager.Instance;
            
            if (StagePackPurchaseManager.IsNonCommercialMode)
            {
                purchaseButton.interactable = shouldShow;
            }
            else
            {
                purchaseButton.interactable = shouldShow &&
                                              purchaseManager.IsStoreReady &&
                                              !purchaseManager.IsPurchaseInProgress;
            }
        }
    }

    private void CreatePurchasePanel()
    {
        if (purchasePanelInstance != null || purchasePanelPrefab == null) return;

        purchasePanelInstance = Instantiate(purchasePanelPrefab, transform);
        purchasePanelInstance.transform.SetAsLastSibling();

        Button[] buttons = purchasePanelInstance.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.gameObject.name != "PurchaseButton") continue;
            purchaseButton = button;
            break;
        }

        purchasePanelInstance.SetActive(false);
    }

    /// <summary>
    /// 모드 상태에 맞는 버튼 텍스트와 이벤트 함수를 실시간으로 다시 달아주는 함수
    /// </summary>
    private void SetupPurchaseButtonListeners()
{
    if (purchaseButton == null) return;

    purchaseButton.onClick.RemoveAllListeners();

    if (StagePackPurchaseManager.IsNonCommercialMode)
    {
        // 1. 비상업용 텍스트 변경
        TMP_Text buttonText = purchaseButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null) buttonText.text = "확인"; 
        else
        {
            Text legacyText = purchaseButton.GetComponentInChildren<Text>();
            if (legacyText != null) legacyText.text = "확인";
        }

        // 2. 🔥 클릭 시 데이터 저장 후, 다른 상태 조건(currentState)과 관계없이 패널을 즉시 직접 끕니다!
        purchaseButton.onClick.AddListener(() => {
            Debug.Log("[Non-Commercial] 무료 해금 처리 완료 및 패널 강제 폐쇄.");
            PlayerPrefs.SetInt("iap_stage_pack_567_owned", 1);
            PlayerPrefs.Save();
            
            // 💡 씬 상태나 타 조건문 검사를 패스하고 오브젝트를 다이렉트로 비활성화합니다.
            if (purchasePanelInstance != null)
            {
                purchasePanelInstance.SetActive(false); 
            }
            
            // UI 상태 전체 동기화를 위해 마지막 리프레시 1회 실행
            RefreshPurchasePanel(); 
        });
    }
    else
    {
        // 상업용 텍스트 및 결제 함수 연동
        TMP_Text buttonText = purchaseButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null) buttonText.text = "구매하기"; 
        else
        {
            Text legacyText = purchaseButton.GetComponentInChildren<Text>();
            if (legacyText != null) legacyText.text = "구매하기";
        }

        purchaseButton.onClick.AddListener(StagePackPurchaseManager.Instance.PurchaseStagePack);
    }
}

    private bool IsSelectedStagePurchaseLocked()
    {
        if (StagePackPurchaseManager.IsNonCommercialMode)
        {
            return StagePackPurchaseManager.IsPremiumStage(stageIndex[0]) && 
                   PlayerPrefs.GetInt("iap_stage_pack_567_owned", 0) == 0;
        }

        return StagePackPurchaseManager.IsPremiumStage(stageIndex[0]) &&
               !StagePackPurchaseManager.IsStagePackOwned;
    }
}