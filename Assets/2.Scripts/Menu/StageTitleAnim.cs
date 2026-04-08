using UnityEngine;
using TMPro;
using DG.Tweening;

public class StageTitleAnim : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform movePanel;       
    public CanvasGroup canvasGroup;       
    public TextMeshProUGUI titleText;     
    public StageTitleVert trapezoidEffect;

    [Header("위치 세팅")]
    public float startX = -1500f;
    public float centerX = 0f;
    public float endX = 1500f;

    [Header("시간 세팅")]
    public float slideDuration = 0.6f;

    private enum AnimState { Idle, Intro, Outro }
    private AnimState currentState = AnimState.Idle;
    private string targetStageName; 

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = movePanel.GetComponent<CanvasGroup>();
        movePanel.anchoredPosition = new Vector2(startX, movePanel.anchoredPosition.y);
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ChangeTitle(string newStageName)
    {
        targetStageName = newStageName; 

        if (currentState == AnimState.Outro) return;
        else if (currentState == AnimState.Intro || (gameObject.activeInHierarchy && canvasGroup.alpha > 0.1f))
        {
            PlayTransitionOutro();
        }
        else
        {
            PlayIntro();
        }
    }

    private void PlayTransitionOutro()
    {
        currentState = AnimState.Outro;
        KillAllTweens();

        float targetX = endX;
        float duration = slideDuration * 0.8f; 
        float targetScale = 0.2f;
        Ease moveEase = Ease.InCubic;

        // 🔥 핵심 해결: 미친 듯이 연타했을 때의 방어 로직!
        // 텍스트가 아직 중앙에 도착하지도 못했는데 (왼쪽에서 오던 중인데) 또 스와이프를 했다면?
        if (movePanel.anchoredPosition.x < centerX - 100f)
        {
            // 가운데서 흉하게 작아지지 말고, 나왔던 곳(왼쪽)으로 다시 빠르게 백스텝으로 도망갑니다!
            targetX = startX;
            duration = 0.25f; // 눈에 안 거슬리게 초고속으로 치워버림
            targetScale = movePanel.localScale.x; // 크기는 그대로 유지 (작아지지 않음!)
            moveEase = Ease.OutQuad;
        }

        Sequence transitionSeq = DOTween.Sequence();
        transitionSeq.Append(movePanel.DOAnchorPosX(targetX, duration).SetEase(moveEase))
                     .Join(movePanel.DOScale(targetScale, duration).SetEase(moveEase))
                     .Join(canvasGroup.DOFade(0f, duration));

        transitionSeq.OnComplete(() => {
            PlayIntro(); 
        });
    }

    public void PlayIntro()
    {
        currentState = AnimState.Intro;
        gameObject.SetActive(true);
        
        KillAllTweens();
        
        canvasGroup.alpha = 0f;
        movePanel.anchoredPosition = new Vector2(startX, movePanel.anchoredPosition.y);
        movePanel.localScale = Vector3.one; 

        titleText.text = targetStageName; 
        if (trapezoidEffect != null) trapezoidEffect.ApplyWarp();

        Sequence introSeq = DOTween.Sequence();
        introSeq.Append(movePanel.DOAnchorPosX(centerX, slideDuration).SetEase(Ease.OutExpo))
                .Join(canvasGroup.DOFade(1f, slideDuration * 0.8f));

        introSeq.OnComplete(() => {
            currentState = AnimState.Idle;
        });
    }

    // 게임 끄거나 완전히 닫을 때 쓸 수 있는 수동 퇴장 함수
    public void PlayOutro()
    {
        currentState = AnimState.Outro;
        KillAllTweens();

        Sequence outroSeq = DOTween.Sequence();
        outroSeq.Append(movePanel.DOAnchorPosX(endX, slideDuration).SetEase(Ease.InCubic))
                .Join(movePanel.DOScale(0.2f, slideDuration).SetEase(Ease.InCubic))
                .Join(canvasGroup.DOFade(0f, slideDuration));

        outroSeq.OnComplete(() => {
            currentState = AnimState.Idle;
            gameObject.SetActive(false);
        });
    }

    private void KillAllTweens()
    {
        movePanel.DOKill();
        canvasGroup.DOKill();
    }
}