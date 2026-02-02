using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Mathematics;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections;

public class MagnetScrollView : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    public HorizontalLayoutGroup layoutGroup;
    
    [Space]
    // [SerializeField] private float threshold = 270f;
    // [SerializeField] private float transitionTime = 0.3f; // 애니메이션 시간
    [SerializeField] private float scaleSpeed = 10f;     // 크기가 변하는 속도
    [SerializeField] private float minScale = 0.8f;            // 양옆일 때 최소 스케일
    [SerializeField] private float maxScale = 1.0f;            // 중앙일 때 최대 스케일
    [SerializeField] private float distanceToFullDark = 600f;  // 중앙에서 이만큼 떨어지면 어둡게
    [SerializeField] private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color brightColor = new Color(1f, 1f, 1f, 1f);


    private RectTransform[] stageDisks;
    private float[] itemPositions;
    private bool isDragging = false;
    private int targetIndex = 0;
    private float itemWidth;
    private float centerPos; // 화면의 정중앙 좌표
    private float interval;

    public void Start()
    {
        InitScrollView();
    }

    public void InitScrollView()
    {
        // 1. 아이템 목록 가져오기
        int childCount = contentPanel.childCount;
        stageDisks = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            stageDisks[i] = contentPanel.GetChild(i).GetComponent<RectTransform>();
        }

        if (childCount == 0) return;

        // 2. 패딩 자동 계산
        // 화면 너비의 절반에서 아이템 너비의 절반을 뺀 만큼 여백을 줘야 1번이 가운데 옴
        itemWidth = stageDisks[0].rect.width;
        float viewPortWidth = scrollRect.viewport.rect.width;
        int padding = (int)((viewPortWidth - itemWidth) / 2);

        layoutGroup.padding.left = padding;
        layoutGroup.padding.right = padding;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel); // 레이아웃 즉시 적용

        // 3. 각 아이템이 중앙에 올 때의 Content 좌표 미리 계산
        itemPositions = new float[childCount];
        interval = layoutGroup.spacing + itemWidth;
        
        // Content의 초기 x좌표는 0이라고 가정할 때 (왼쪽 정렬 기준)
        // 0번 아이템을 중앙에 두려면 content는 0에 있어야 함 (Left Padding 덕분)
        // 1번 아이템을 중앙에 두려면 content는 -(itemWidth + spacing) 만큼 이동해야 함
        for (int i = 0; i < childCount; i++)
        {
            itemPositions[i] = -(i * interval);
        }
        
        // 화면 정중앙 x좌표 (ViewPort 기준)
        centerPos = viewPortWidth / 2f;
    }

    public void UpdateDifficulty(int difficulty)
    {
        // 난이도에 따라 아이템 갱신 필요 시 구현
    }

    void Update()
    {
        if (!isDragging && MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect)
        {
            // CD 회전
            stageDisks[targetIndex].Rotate(0, 0, 1.7f * Time.deltaTime);
        }
    }

    private void UpdateScaleAndColor()
    {
        int maxEffectRange = 2;
        for (int i = Mathf.Max(0, targetIndex - maxEffectRange); i < Mathf.Min(stageDisks.Length, targetIndex + maxEffectRange + 1); i++)
        {
            float dist = Mathf.Abs(stageDisks[i].anchoredPosition.x + contentPanel.anchoredPosition.x - centerPos);
            float factor = Mathf.Clamp01(dist / distanceToFullDark);

            // 스케일 보간
            float scaleVal = Mathf.Lerp(maxScale, minScale, factor);
            stageDisks[i].localScale = new Vector3(scaleVal, scaleVal, 1f);

            // 컬러 보간 
            Image img = stageDisks[i].GetComponent<Image>();
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
            Debug.Log("Increasing target index: " + targetIndex);
        }
        else if (currentScrollX > itemPositions[targetIndex] + interval / 2f && targetIndex != 0)
        {
            targetIndex--;
            Debug.Log("Decreasing target index: " + targetIndex);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("On Drag");
        isDragging = true;
        CheckTargetIndex();
        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
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
                if (Mathf.Abs(scrollRect.velocity.x) < 0.05f)
                {
                    yield break;
                }
                else if (Mathf.Abs(scrollRect.velocity.x) < 400f) // 속도가 줄면 자석 발동
                {
                    Debug.Log("Snapping to index: " + targetIndex);
                    scrollRect.velocity = Vector2.zero; // 물리 관성 끄기
                    Vector2 newPos = contentPanel.anchoredPosition;
                    while (Mathf.Abs(newPos.x - itemPositions[targetIndex]) > 1f)
                    {
                        if (isDragging)
                        {
                            yield break;
                        }
                        newPos.x = Mathf.Lerp(newPos.x, itemPositions[targetIndex], Time.deltaTime * 5f);
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