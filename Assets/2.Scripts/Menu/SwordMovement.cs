using System.Collections;
using UnityEngine;
using static MenuManager;
using DG.Tweening; // 🔥 DOTween 추가

/// <summary>
/// 검 움직임 연출 관리
/// </summary>
public class SwordMovement : MonoBehaviour
{
    private RectTransform rect;
    private Tween floatingTween; // 둥둥거리는 애니메이션 저장용

    public void InitUI(bool noTitle = false)
    {
        rect = GetComponent<RectTransform>();
        if (noTitle)
        {
            ChangeSwordPosition();
        }
        
        // 🔥 타이틀 화면이든 메뉴 화면이든, 초기화 직후부터 바로 둥둥거리기 시작!
        StartFloating(); 
    }

    // 🔥 둥둥 떠다니는 움직임 (DOTween Yoyo 루프)
    public void StartFloating()
    {
        if (floatingTween != null && floatingTween.IsActive()) return;

        float startY = rect.anchoredPosition.y;
        // 위아래로 15px씩 1.5초 주기로 부드럽게(InOutSine) 영원히(Loops(-1)) 움직임
        floatingTween = rect.DOAnchorPosY(startY + 15f, 1.5f)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
    }

    // 둥둥거림 정지
    public void StopFloating()
    {
        if (floatingTween != null) floatingTween.Kill();
    }

    /// <summary>
    /// 검 올라오는 연출 (코루틴 -> DOTween으로 교체)
    /// </summary>
    public void StartSwordUp(float addY, float dur)
    {
        StopFloating(); // 솟구치기 전에 둥둥거리는 연출 먼저 정지

        float targetY = rect.anchoredPosition.y + addY;
        
        // 칼을 뽑을 때 살짝 뒤로 당겼다가 슈욱! 올라가는 찰진 느낌 (InOutBack)
        rect.DOAnchorPosY(targetY, dur)
            .SetEase(Ease.InOutBack)
            .OnComplete(() => 
            {
                MenuManager.Instance.SwordUpEnd(); // 끝나면 매니저에게 알림
            });
    }

    private void ChangeSwordPosition()
    {
        rect.anchoredPosition += Vector2.up * -MenuManager.Instance.height / 7;
    }
}