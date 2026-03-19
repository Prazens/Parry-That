using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HandTutorialIndicator : MonoBehaviour
{
    [Header("UI 연결")]
    public RectTransform handIcon;
    public CanvasGroup handCanvasGroup;
    
    // 🔥 새로 추가: 파티클이 정확히 따라갈 손끝 위치
    public Transform fingerTipPoint;    
    
    [Header("동작별 파티클 연결")]
    public ParticleSystem touchParticle;
    public ParticleSystem swipeParticle;
    public ParticleSystem holdParticle;

    [Header("설정값")]
    public float holdDuration = 1.5f; // 홀드를 유지하는 시간 (초)

    private Sequence animSeq;
    private Vector2 defaultPos;

    private void Awake()
    {
        // 프리팹이 처음 켜질 때의 손가락 위치를 기본 위치로 저장
        defaultPos = handIcon.anchoredPosition;
    }

    private void OnDisable()
    {
        StopTutorial(); // 오브젝트가 꺼지면 애니메이션 찌꺼기 완벽 제거
    }

    // 내부 애니메이션 초기화용 (새 동작이 들어올 때 겹침 방지)
    private void ResetState()
    {
        if (animSeq != null) animSeq.Kill();
        
        touchParticle.Stop();
        touchParticle.Clear(); // 🔥 찌꺼기 청소

        swipeParticle.Stop();
        swipeParticle.Clear(); // 🔥 찌꺼기 청소

        holdParticle.Stop();
        holdParticle.Clear();  // 🔥 찌꺼기 청소
        
        handIcon.anchoredPosition = defaultPos;
        handIcon.localScale = Vector3.one;
        handCanvasGroup.alpha = 1f;
    }

    // 🛑 다른 스크립트에서 이 가이드를 완전히 끌 때 호출
    public void StopTutorial()
    {
        ResetState();
        gameObject.SetActive(false);
    }

    // 👆 1. 터치 (Tap) 루프 실행
    public void PlayTouch()
    {
        gameObject.SetActive(true);
        ResetState();
        
        animSeq = DOTween.Sequence();
        animSeq.Append(handIcon.DOScale(0.8f, 0.2f).SetEase(Ease.OutQuad)) // 1. 누른다 (작아짐)
               .AppendCallback(() => touchParticle.Play())                 // 2. 파티클 팡!
               .Append(handIcon.DOScale(1f, 0.2f).SetEase(Ease.OutBack))   // 3. 뗀다 (원상복구)
               .AppendInterval(0.6f)                                       // 4. 대기
               .SetLoops(-1);                                              // 5. 무한 반복
    }

    // 🚀 2. 스와이프 (Swipe) 루프 실행
    public void PlaySwipe(Vector2 swipeOffset)
    {
        gameObject.SetActive(true);
        ResetState();
        
        // 🔥 [속도 조절] 여기서 숫자를 수정하세요!
        float swipeDuration = 0.8f;  // 스와이프 쫙! 긋는 시간 (기존 0.6f -> 1.2f로 2배 느려짐. 클수록 느립니다)
        float fadeDuration = 0.3f;   // 끝에서 스르륵 사라지는 페이드 아웃 시간

        animSeq = DOTween.Sequence();
        
        // 1. 누르는 애니메이션 (0.15초)
        animSeq.Append(handIcon.DOScale(0.9f, 0.15f).SetEase(Ease.OutQuad)) 
               .AppendCallback(() => {
                   swipeParticle.transform.localScale = Vector3.one; 
                   swipeParticle.Play();                             
               })
               
               // 2. 스와이프 이동 (0.8초)
               .Append(handIcon.DOAnchorPos(defaultPos + swipeOffset, swipeDuration).SetEase(Ease.OutSine)
                    .OnUpdate(() => {
                        if (fingerTipPoint != null)
                        {
                            swipeParticle.transform.position = fingerTipPoint.position;
                        }
                    }))
               
               // 🔥 핵심 추가: 손가락이 사라지기 시작하는 정확한 타이밍에 파티클 뿜기를 '미리' 멈춥니다!
               // 시간 계산: 앞선 누르기(0.15) + 스와이프 시간(swipeDuration) - 페이드 시간(fadeDuration)
               .InsertCallback(0.15f + swipeDuration - fadeDuration, () => {
                   swipeParticle.Stop(); 
               })

               // 3. 이동 끝자락에서 손가락 투명해지기
               .Join(handCanvasGroup.DOFade(0f, fadeDuration).SetDelay(swipeDuration - fadeDuration))
               
               // 4. 파티클 스케일도 0으로 줄여버리기
               .Join(swipeParticle.transform.DOScale(Vector3.zero, fadeDuration).SetDelay(swipeDuration - fadeDuration - 0.1f))
               
               // 5. 복귀 및 찌꺼기 완벽 청소
               .AppendCallback(() => {
                   swipeParticle.Stop();
                   swipeParticle.Clear(); // 찌꺼기 싹둑!
                   handIcon.anchoredPosition = defaultPos;
                   handIcon.localScale = Vector3.one;
               })
               .Append(handCanvasGroup.DOFade(1f, 0.2f))
               .AppendInterval(0.5f)
               .SetLoops(-1);
    }

    // ⏱️ 3. 홀드 (Hold) 루프 실행
    public void PlayHold()
    {
        gameObject.SetActive(true);
        ResetState();
        
        animSeq = DOTween.Sequence();
        animSeq.Append(handIcon.DOScale(0.8f, 0.2f).SetEase(Ease.OutQuad)) // 1. 누른다
               .AppendCallback(() => holdParticle.Play())                  // 2. 지속 파티클 켜기
               .AppendInterval(holdDuration)                               // 3. 누른 채로 지정된 시간 대기
               .AppendCallback(() => holdParticle.Stop())                  // 4. 지속 파티클 끄기
               .Append(handIcon.DOScale(1f, 0.2f).SetEase(Ease.OutBack))   // 5. 뗀다
               .AppendInterval(0.5f)
               .SetLoops(-1);
    }
}