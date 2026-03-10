using System.Collections;
using UnityEngine;
using static MenuManager;

/// <summary>
/// 검 움직임 연출 관리
/// </summary>
public class SwordMovement : MonoBehaviour
{
    private RectTransform rect;
    private float elapsedTime = 0f;

    public void InitUI(bool noTitle = false)
    {
        rect = GetComponent<RectTransform>();
        if (noTitle)
        {
            ChangeSwordPosition();
        }
    }

    void Update()
    {
        if (MenuManager.Instance.currentState == MenuState.StageSelect)
        {
            elapsedTime += Time.deltaTime;
            // 칼 둥둥 떠다니는 느낌
            float newY = rect.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newY);
        }
    }

    /// <summary>
    /// 검 올라오는 연출
    /// </summary>
    /// <param name="addY">올라가는 y값</param>
    /// <param name="dur">지속 시간</param>
    public void StartSwordUp(float addY, float dur)
    {
        StartCoroutine(SwordUp(addY, dur));
    }

    /// <summary>
    /// 검 올라가는 연출 코루틴
    /// </summary>
    /// <param name="addY">올라가는 y값</param>
    /// <param name="dur">지속 시간</param>
    /// <returns></returns>
    private IEnumerator SwordUp(float addY, float dur)
    {
        float _elapsedTime = 0f;
        Vector3 startPosition = rect.anchoredPosition;
        float startY = startPosition.y;
        float targetY = startY + addY;

        while (_elapsedTime < dur)
        {
            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / dur);
            float easeOutT = 1f - Mathf.Pow(1f - t, 2);
            float currentY = Mathf.Lerp(startY, targetY, easeOutT);
            rect.anchoredPosition = new Vector3(startPosition.x, currentY, startPosition.z);

            yield return null;
        }
        rect.anchoredPosition = new Vector3(startPosition.x, targetY, startPosition.z);
        MenuManager.Instance.SwordUpEnd();
    }

    private void ChangeSwordPosition()
    {
        rect.anchoredPosition += Vector2.up * MenuManager.Instance.height;
    }
}