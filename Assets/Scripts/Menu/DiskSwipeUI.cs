using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;


/// <summary>
/// 디스크 스와이프 UI 관리
/// <para>"StageMenu.cs"에서 분리됨</para>
/// <para>터치 입력 받고 디스크 스크롤, 현재 선택된 스테이지 표시</para>
/// </summary>
public class DiskSwipeUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int[] curIndex = new int[2] { 0, 0 }; // [stageIndex, difficulty]

    [SerializeField] private List<List<RectTransform>> stageDisks;
    [SerializeField] private float threshold = 270f;
    [SerializeField] private float swipeSpeed = 0.7f;   // 감도
    [SerializeField] private float transitionTime = 0.3f; // 애니메이션 시간

    private float screenWidth;
    private bool isDragging = false;

    private float minScale = 0.8f;            // 양옆일 때 최소 스케일
    private float maxScale = 1.0f;            // 중앙일 때 최대 스케일
    private float distanceToFullDark = 600f;  // 중앙에서 이만큼 떨어지면 어둡게
    private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    private Color brightColor = new Color(1f, 1f, 1f, 1f);

    public void InitUI()
    {
        screenWidth = Screen.width;

        // 모든 오브젝트를 "현재 인덱스" 기준으로 자리 배치
        UpdateStagePositions();

        // 크기/색상 보정
        UpdateScaleAndColor();

        // "현재, 양옆"만 켜고, 나머지 끔, 없어도 되는 함수
        ActivateOnlyRelevantObjects();
    }

    private void UpdateStagePositions()
    {
        var targets = CalculateTargetPositions();
        foreach (var kv in targets)
        {
            stageDisks[kv.Key][curIndex[1]].anchoredPosition = kv.Value;
        }
    }

    private void UpdateScaleAndColor()
    {
        foreach (var diskList in stageDisks)
        {
            foreach (var disk in diskList)
            {
                if (!disk.gameObject.activeSelf)
                continue;

                float dist = Mathf.Abs(disk.anchoredPosition.x);
                float factor = Mathf.Clamp01(dist / distanceToFullDark);

                // 스케일 보간
                float scaleVal = Mathf.Lerp(maxScale, minScale, factor);
                disk.localScale = new Vector3(scaleVal, scaleVal, 1f);

                // 컬러 보간 
                Image img = disk.GetComponent<Image>();
                if (img)
                {
                    img.color = Color.Lerp(brightColor, darkColor, factor);
                }
            }
        }
    }

    private void ActivateOnlyRelevantObjects()
    {
        for (int i = 0; i < stageDisks.Count; i++)
        {
            stageDisks[i][StageDBManager.Instance.diffNumbers[i] >= curIndex[1] ? 0 : curIndex[1]].gameObject.SetActive(false);
        }

        stageDisks[curIndex[0]][curIndex[1]].gameObject.SetActive(true);
        if (curIndex[0] - 1 >= 0)
            stageDisks[curIndex[0] - 1][StageDBManager.Instance.diffNumbers[curIndex[0] - 1] >= curIndex[1] ? 0 : curIndex[1]].gameObject.SetActive(true);
        if (curIndex[0] + 1 < stageDisks.Count)
            stageDisks[curIndex[0] + 1][StageDBManager.Instance.diffNumbers[curIndex[0] + 1] >= curIndex[1] ? 0 : curIndex[1]].gameObject.SetActive(true);
    }

    private Dictionary<int, Vector2> CalculateTargetPositions()
    {
        Dictionary<int, Vector2> result = new Dictionary<int, Vector2>();

        for (int i = 0; i < stageDisks.Count; i++)
        {
            int diff = i - curIndex[0];

            Vector2 pos;
            if (diff == 0)
            {
                pos = Vector2.zero;
            }
            else if (diff == 1)
            {
                pos = new Vector2(screenWidth * 0.5f, 0); 
            }
            else if (diff == -1)
            {
                pos = new Vector2(-screenWidth * 0.5f, 0);
            }
            else if (diff > 1)
            {
                pos = new Vector2(screenWidth * 0.5f * diff, 0);
            }
            else
            {
                pos = new Vector2(-screenWidth * 0.5f * Mathf.Abs(diff), 0);
            }

            result[i] = pos;
        }

        return result;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        float deltaX = eventData.delta.x * swipeSpeed;

        for (int i = 0; i < stageDisks.Count; i++)
        {
            if (!stageDisks[i][curIndex[1]].gameObject.activeSelf)
                continue;

            stageDisks[i][curIndex[1]].anchoredPosition += new Vector2(deltaX, 0);
        }

        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float centerX = stageDisks[curIndex[0]][curIndex[1]].anchoredPosition.x;
        float effectiveThreshold = screenWidth * 0.25f; 
        if (Mathf.Abs(centerX) > effectiveThreshold) 
        {
            // 왼쪽 스와이프(centerX < 0) → currentIndex + 1
            if (centerX < 0)
            {
                if (curIndex[0] < stageDisks.Count - 1)
                {
                    curIndex[0]++;
                }
            }
            // 오른쪽 스와이프(centerX > 0) → currentIndex - 1
            else
            {
                if (curIndex[0] > 0)
                {
                    curIndex[0]--;
                }
            }
        }

        // 위치 보정(코루틴)
        StopAllCoroutines();
        StartCoroutine(SmoothMove());
    }

    private IEnumerator SmoothMove()
    {
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < stageDisks.Count; i++)
        {
            startPositions.Add(stageDisks[i][curIndex[1]].anchoredPosition);
        }
        Dictionary<int, Vector2> targetPos = CalculateTargetPositions();

        ActivateOnlyRelevantObjects();

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            for (int i = 0; i < stageDisks.Count; i++)
            {
                if (!stageDisks[i][curIndex[1]].gameObject.activeSelf)
                    continue;

                Vector2 sp = startPositions[i];
                Vector2 ep = targetPos[i];
                stageDisks[i][curIndex[1]].anchoredPosition = Vector2.Lerp(sp, ep, t);
            }

            UpdateScaleAndColor();
            yield return null;
        }

        foreach (var kv in targetPos)
        {
            if (stageDisks[kv.Key][curIndex[1]].gameObject.activeSelf)
            {
                stageDisks[kv.Key][curIndex[1]].anchoredPosition = kv.Value;
            }
        }

        UpdateScaleAndColor();
    }

    public void UpdateDifficulty(int difficulty)
    {
        curIndex[1] = difficulty;
    }

    // 회전 애니메이션
    void Update()
    {
        if (MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect && !isDragging)
        {
            // CD 회전
            stageDisks[curIndex[0]][curIndex[1]].Rotate(0, 0, 1.7f * Time.deltaTime); 
        } 

    }
}
