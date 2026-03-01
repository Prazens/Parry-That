using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MenuManager;

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
    [SerializeField] private List<Sprite> unpackedStageDiskSprites;  // 스프라이트 리스트
    // [SerializeField] private Sprite tutorialDiskSprite;  // 튜토리얼 디스크 스프라이트
    // [SerializeField] private Sprite epilogueDiskSprite;  // 에필로그 디스크 스프라이트
    private List<List<List<Sprite>>> stageDiskSprites;  // [stageIndex][isCleared][difficulty] 스프라이트 목록, 튜토리얼과 에필로그 미포함
    // 없는 난이도는 자리는 있지만 null로 남겨두어야 함

    [SerializeField] private List<AudioClip> previewSounds;  // 디스크 선택시 재생할 미리듣기 사운드 목록
    private AudioSource currentPreviewSound;

    private RectTransform[] stageDisks;  // 튜토리얼과 에필로그 포함
    private float[] itemPositions;
    public bool isDragging = false;
    private int targetIndex = 0;
    private float itemWidth;
    private float centerPos; // 화면의 정중앙 좌표
    private float interval;

    //private void Start()  // 임시
    //{
    //    InitScrollView();
    //}

    public void InitScrollView(int initialIndex, int difficulty)
    {
        stageDiskSprites = new List<List<List<Sprite>>>();
        int stageCount = StageDBManager.Instance.stageNumbers - 2;
        for (int i = 0; i < stageCount; i++)
        {
            stageDiskSprites.Add(new List<List<Sprite>>());
            for (int j = 0; j < 2; j++)  // 클리어 여부에 따른 스프라이트 구분
            {
                stageDiskSprites[i].Add(new List<Sprite>());
                for (int k = 0; k < StageDBManager.MAX_DIFFS; k++)  // 난이도에 따른 스프라이트 구분
                {
                    int spriteIndex = (i * 2 * StageDBManager.MAX_DIFFS)
                        + (j * StageDBManager.MAX_DIFFS) + k;
                    if (spriteIndex < unpackedStageDiskSprites.Count)
                    {
                        stageDiskSprites[i][j].Add(unpackedStageDiskSprites[spriteIndex]);
                    }
                    else
                    {
                        Debug.LogError($"Not enough sprites in unpackedStageDiskSprites for stage {i}, clear {j}, difficulty {k}");
                    }
                }
            }
        }

        currentPreviewSound = gameObject.AddComponent<AudioSource>();  // 임시로 여기에 추가
        // 아이템 위치 및 패딩 설정, 나중에는 instantiate로 동적 생성 시켜야 함
        int childCount = contentPanel.childCount;
        stageDisks = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            stageDisks[i] = contentPanel.GetChild(i).GetComponent<RectTransform>();
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
        if (difficulty > 0)
        {
            UpdateDifficulty(difficulty);
        }
    }


    public void UpdateDifficulty(int difficulty)
    {
        // 난이도에 따라 아이템 갱신

        for (int i = 0; i < stageDiskSprites.Count; i++)
        {
            Debug.Log($"Updating stage {i + 1} disk sprite for difficulty {difficulty}");
            stageDisks[i + 1].GetComponent<Image>().sprite = stageDiskSprites
                [i]
                [StageDBManager.Instance.starRatings[StageDBManager.Instance.CurrentStage[0], StageDBManager.Instance.CurrentStage[1]] == 0 ? 0 : 1]
                [Mathf.Min(difficulty, StageDBManager.Instance.diffNumbers[i + 1] - 1)];
        }
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
        StartCoroutine(TempMusicPlay(targetIndex));
        MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        UpdateScaleAndColor();
    }

    public void GoLeft()
    {
        if (targetIndex > 0)
        {
            targetIndex--;
            GoToStage(targetIndex);
        }
    }

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
        if (!isDragging && MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect)
        {
            stageDisks[targetIndex].Rotate(0, 0, 1.7f * Time.deltaTime);  // CD 회전, 임시로 하드코딩 된 값
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
            StartCoroutine(TempMusicPlay(targetIndex));
            MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        }
        else if (currentScrollX > itemPositions[targetIndex] + interval / 2f && targetIndex != 0)
        {
            targetIndex--;
            StartCoroutine(TempMusicPlay(targetIndex));
            MenuManager.Instance.UpdateCurStage(targetIndex, MenuManager.Instance.stageIndex[1]);
        }
    }

    /// <summary>
    /// 미리듣기 사운드 재생 코루틴
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    IEnumerator TempMusicPlay(int targetIndex)  // 임시로 여기에 넣어놓음, 아마 매니저를 새로 파거나 MenuManager에 넣어야 할 듯
    {
        Debug.Log("TempMusicPlay called for index: " + targetIndex);

        if (MenuManager.Instance.currentState != MenuManager.MenuState.StageSelect)
        {
            yield break;
        }

        currentPreviewSound.loop = true;
        currentPreviewSound.Stop();
        if (targetIndex == StageDBManager.Instance.stageNumbers - 1)  // 에필로그는 미리듣기 없음
        {
            yield return null;
        }
        currentPreviewSound.clip = previewSounds[targetIndex];
        currentPreviewSound.volume = PlayerPrefs.GetFloat("bgmVolume", 1f) * PlayerPrefs.GetFloat("masterVolume", 1f);
        currentPreviewSound.Play();
        yield return null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
        {
            return;
        }

        if (!isDragging)
        {
            isDragging = true;
        }
        CheckTargetIndex();
        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
        {
            return;
        }

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
