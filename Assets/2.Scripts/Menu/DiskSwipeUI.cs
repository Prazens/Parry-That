using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MenuManager;


// 직렬화를 위한 Wrapper 클래스
[System.Serializable]
public class StageDiskSprites
{
    public Sprite notClearedNormal;
    public Sprite notClearedHard;
    public Sprite clearedNormal;
    public Sprite clearedHard;

    public Sprite this[int difficulty, bool isCleared]
    {
        get
        {
            return difficulty switch
            {
                0 => isCleared ? clearedNormal : notClearedNormal,
                1 => isCleared ? clearedHard : notClearedHard,
                _ => null
            };
        }
    }
}

/// <summary>
/// 디스크 스와이프 UI 관리
/// <para>"StageMenu.cs"에서 분리됨</para>
/// <para>터치 입력 받고 디스크 스크롤, 현재 선택된 스테이지 표시</para>
/// <para>스테이지 선택 관련 연출 포함?</para>
/// </summary>
public class DiskSwipeUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    public HorizontalLayoutGroup layoutGroup;

    [Space]
    [SerializeField] private float minScale = 0.8f;            // 양옆일 때 최소 스케일
    [SerializeField] private float maxScale = 1.0f;            // 중앙일 때 최대 스케일
    [SerializeField] private float distanceToFullDark = 600f;  // 중앙에서 이만큼 떨어지면 어둡게
    [SerializeField] private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color brightColor = new Color(1f, 1f, 1f, 1f);

    [Space]
    [SerializeField] private GameObject stageDiskPrefab;  // 스테이지 디스크 프리팹
    [SerializeField] private List<StageDiskSprites> stageDiskSprites;  // 스프라이트 리스트, [stageIndex][difficulty, isCleared]
    // private List<List<List<Sprite>>> stageDiskSprites;  // [stageIndex][isCleared][difficulty] 스프라이트 목록, 튜토리얼과 에필로그 미포함
    // 없는 난이도는 자리는 있지만 null로 남겨두어야 함

    [SerializeField] private List<AudioClip> previewSounds;  // 디스크 선택시 재생할 미리듣기 사운드 목록

    [Space]
    [SerializeField] private float rotationSpeed = 6f;  // 디스크 회전 속도

    [Space]
    [SerializeField] private float swipeThreshold = 10f;
    private float weightedSwipeThreshold;
    private bool isDragging = false;

    private RectTransform[] stageDisks;  // 튜토리얼과 에필로그 포함
    private float[] itemPositions;
    
    private bool canRotate = true;  // 디스크 회전 허용 여부

    // targetIndex에 접근할 때마다 해당 스테이지가 잠금 해제되어 있는지 확인하도록 하기 위해 프로퍼티로 감싸서 사용
    private int _targetIndex = 0;  // 프로퍼티용 내부 변수, 같은 클래스에서라도 직접 접근하지 말 것
    private int targetIndex
    {
        get => _targetIndex;
        set
        {
            if (value != _targetIndex)
            {
                // Debug.Log($"Attempting to set targetIndex to {value}, checking unlock status...");
                canRotate = MenuManager.Instance.JudgeStageUnlock(value, MenuManager.Instance.stageIndex[1]);
                // stageDisks[_targetIndex].localRotation = Quaternion.Euler(0, 0, 0);
                
                stageDisks[_targetIndex].DOLocalRotate(new Vector3(0, 0, 0), 2f / rotationSpeed).SetEase(Ease.OutQuad);
                _targetIndex = value;
            }
        }
    }

    private float itemWidth;
    private float centerPos; // 화면의 정중앙 좌표
    private float interval;

    private int tempDiff;  // gc 방지(효과가 있는지는 몰루)

    private bool canClickDisk = false;

    private void Start()
    {
        weightedSwipeThreshold = swipeThreshold * Screen.height / 1920f;
    }

    private void OnEnable()
    {
        canClickDisk = false;
        StartCoroutine(EnableDiskClick());
    }

    private IEnumerator EnableDiskClick()
    {
        yield return new WaitForSeconds(0.5f);
        canClickDisk = true;
    }

    public void InitScrollView(int initialIndex, int difficulty)
    {
        // 아이템 생성, 위치 및 패딩 설정

        int childCount = StageDBManager.Instance.stageNumbers;
        stageDisks = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            stageDisks[i] = Instantiate(stageDiskPrefab, contentPanel)
                .GetComponent<RectTransform>();

            Button button = stageDisks[i].GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnDiskClick);
            }
        }

        if (childCount == 0) return;

        itemWidth = stageDisks[0].rect.width;
        float viewPortWidth = scrollRect.viewport.rect.width;
        int padding = (int)((viewPortWidth - itemWidth) / 2);

        layoutGroup.padding.left = padding;
        layoutGroup.padding.right = padding;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);  // 레이아웃 즉시 적용

        interval = layoutGroup.spacing + itemWidth;  // 아이템의 중심 간의 거리
        
        itemPositions = new float[childCount];  // 각 아이템이 중앙에 올 때의 Content 좌표
        for (int i = 0; i < childCount; i++)
        {
            itemPositions[i] = -(i * interval);
        }
        
        centerPos = viewPortWidth / 2f;  // 화면 정중앙 x좌표 (ViewPort 기준)
        GoToStage(initialIndex);
        UpdateDifficulty(difficulty);
    }

    /// <summary>
    /// 난이도 변경에 따라 디스크 스프라이트와 잠금 상태 업데이트
    /// </summary>
    /// <param name="difficulty"></param>
    public void UpdateDifficulty(int difficulty)
    {
        // 난이도에 따라 아이템 갱신

        for (int i = 0; i < stageDiskSprites.Count; i++)
        {
            tempDiff = Mathf.Min(difficulty, StageDBManager.Instance.diffNumbers[targetIndex] - 1);  // 해당 스테이지에 난이도가 없는 경우 최대 난이도로 대체
            Debug.Log($"Updating stage {i} disk sprite for difficulty {tempDiff}");
            stageDisks[i].GetComponent<Image>().sprite = stageDiskSprites[i]
                [tempDiff,
                 StageDBManager.Instance.stageCompletion[i][tempDiff]];

            // 스테이지 잠김 여부에 따라 아이템 활성화
            stageDisks[i].GetChild(0).gameObject.SetActive(!MenuManager.Instance.JudgeStageUnlock(i, tempDiff));
            
        }
        tempDiff = Mathf.Min(difficulty, StageDBManager.Instance.diffNumbers[targetIndex] - 1);  // 현재 선택된 스테이지의 유효한 최대 난이도
        canRotate = MenuManager.Instance.JudgeStageUnlock(targetIndex, tempDiff);
    }

    /// <summary>
    /// 스테이지 인덱스에 해당하는 디스크로 이동
    /// </summary>
    /// <param name="stageIndex"></param>
    public void GoToStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageDisks.Length)
        {
            Debug.LogError($"Invalid stageIndex: {stageIndex}");
            return;
        }

        targetIndex = stageIndex;
        contentPanel.anchoredPosition = new Vector2(itemPositions[targetIndex], contentPanel.anchoredPosition.y);
        StartPreviewSound(targetIndex);
        MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        UpdateScaleAndColor();
    }

    public void StartPreviewSound(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= previewSounds.Count)
        {
            MenuAudioManager.Instance.Stop(AudioTag.BGM);  // 사운드 멈춤
            return;
        }
        MenuAudioManager.Instance.Play(previewSounds[stageIndex], AudioTag.BGM, true);
    }

    /// <summary>
    /// 스와이프 대신 버튼으로 이동
    /// </summary>
    public void GoLeft()
    {
        if (targetIndex > 0)
        {
            targetIndex--;
            GoToStage(targetIndex);
        }
    }

    /// <summary>
    /// 스와이프 대신 버튼으로 이동
    /// </summary>
    public void GoRight()
    {
        if (targetIndex < stageDisks.Length - 1)
        {
            targetIndex++;
            GoToStage(targetIndex);
        }
    }

    private void Update()
    {
        if (!isDragging && MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect && canRotate)
        {
            stageDisks[targetIndex].Rotate(0, 0, rotationSpeed * Time.deltaTime);  // CD 회전, 임시로 하드코딩 된 값
        }
    }

    /// <summary>
    /// 중앙에서 멀어질수록 작아지고 어두워지는 효과 업데이트
    /// </summary>
    private void UpdateScaleAndColor()
    {
        // Debug.Log("UpdateScaleAndColor called");
        int maxEffectRange = 2;  // 임시로 하드코딩 된 값
        for (int i = Mathf.Max(0, targetIndex - maxEffectRange); i < Mathf.Min(stageDisks.Length, targetIndex + maxEffectRange + 1); i++)
        {
            float dist = Mathf.Abs(stageDisks[i].anchoredPosition.x + contentPanel.anchoredPosition.x - centerPos);
            float factor = Mathf.Clamp01(dist / distanceToFullDark);

            float scaleVal = Mathf.Lerp(maxScale, minScale, factor);  // 스케일 보간
            stageDisks[i].localScale = new Vector3(scaleVal, scaleVal, 1f);

            Image img = stageDisks[i].GetComponent<Image>();  // 컬러 보간
            if (img)
            {
                img.color = Color.Lerp(brightColor, darkColor, factor);
            }
        }
    }

    /// <summary>
    /// 선택중인 디스크 확인 및 targetIndex 업데이트
    /// </summary>
    private void CheckTargetIndex()
    {
        float currentScrollX = contentPanel.anchoredPosition.x;
        if (currentScrollX < itemPositions[targetIndex] - interval / 2f && targetIndex != itemPositions.Length - 1)
        {
            targetIndex++;
            StartPreviewSound(targetIndex);
            MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        }
        else if (currentScrollX > itemPositions[targetIndex] + interval / 2f && targetIndex != 0)
        {
            targetIndex--;
            StartPreviewSound(targetIndex);
            MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
            return;

        float swipeDistance =
            Vector2.Distance(eventData.pressPosition, eventData.position);

        if (swipeDistance > weightedSwipeThreshold)
        {
            isDragging = true;
        }

        CheckTargetIndex();
        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
            return;

        StartCoroutine(ResetDragging());
    }

    private IEnumerator ResetDragging()
    {
        yield return null;
        isDragging = false;
        StartCoroutine(SnapToDisk());
    }

    public void OnDiskClick()
    {
        if (!canClickDisk)
            return;
            
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
            return;

        if (isDragging)
            return;

        if (!canRotate)
            return;

        MenuManager.Instance.StartStage();
    }

    public void StopScroll()
    {
        scrollRect.horizontal = false;
        isDragging = false;
        StartCoroutine(SnapToDisk());
    }

    /// <summary>
    /// 디스크가 중앙에 스냅되도록 하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator SnapToDisk()
    {
        while (true)
        {
            if (!isDragging)
            {
                CheckTargetIndex();

                if (Mathf.Abs(scrollRect.velocity.x) < 0.05f)  // 임시로 하드코딩 된 값
                {
                    yield break;
                }

                else if (Mathf.Abs(scrollRect.velocity.x) < 400f)  // 속도가 줄면 자석 발동, 임시로 하드코딩 된 값
                {
                    scrollRect.velocity = Vector2.zero;  // 물리 관성 끄기
                    Vector2 newPos = contentPanel.anchoredPosition;

                    while (Mathf.Abs(newPos.x - itemPositions[targetIndex]) > 1f)  // 임시로 하드코딩 된 값
                    {
                        if (isDragging)
                        {
                            yield break;
                        }
                        newPos.x = Mathf.Lerp(newPos.x, itemPositions[targetIndex], Time.deltaTime * 5f);  // 임시로 하드코딩 된 값
                        contentPanel.anchoredPosition = newPos;

                        UpdateScaleAndColor();
                        yield return null;
                    }

                    contentPanel.anchoredPosition = new Vector2(itemPositions[targetIndex], contentPanel.anchoredPosition.y);
                    yield break;
                }
                
                UpdateScaleAndColor();
                yield return null;
            }
            else
            {
                yield break;
            }
        }
    }
}
