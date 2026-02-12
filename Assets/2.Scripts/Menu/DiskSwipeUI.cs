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
    private List<List<Sprite>> stageDiskSprites;  // [stageIndex][difficulty] 스프라이트 목록

    [SerializeField] private List<AudioClip> previewSounds;  // 디스크 선택시 재생할 미리듣기 사운드 목록
    private AudioSource currentPreviewSound;

    private RectTransform[] stageDisks;
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

    public void InitScrollView(int initialIndex = 0, int difficulty = 0)
    {
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
    }

    public void UpdateDifficulty(int difficulty)
    {
        // 난이도에 따라 아이템 갱신
    }

    private void GoToStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageDisks.Length)
        {
            Debug.LogError($"Invalid stageIndex: {stageIndex}");
            return;
        }

        targetIndex = stageIndex;
        contentPanel.anchoredPosition = new Vector2(itemPositions[targetIndex], contentPanel.anchoredPosition.y);
        UpdateScaleAndColor();
    }

    private void Update()
    {
        if (!isDragging && MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect)
        {
            stageDisks[targetIndex].Rotate(0, 0, 1.7f * Time.deltaTime);  // CD 회전, 임시로 하드코딩 된 값
        }
    }

    private void UpdateScaleAndColor()
    {
        Debug.Log("UpdateScaleAndColor called");
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

    IEnumerator TempMusicPlay(int targetIndex)  // 임시로 여기에 넣어놓음, 아마 매니저를 새로 파거나 MenuManager에 넣어야 할 듯
    {
        Debug.Log("TempMusicPlay called for index: " + targetIndex);
        currentPreviewSound.loop = true;
        currentPreviewSound.Stop();
        if (targetIndex >= previewSounds.Count)
        {
            yield return null;
        }
        currentPreviewSound.clip = previewSounds[targetIndex];
        currentPreviewSound.volume = 0.5f;
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
