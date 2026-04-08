
using UnityEngine;
using DG.Tweening;
public enum SignalType { Tap, Swipe, GhostSwipe }

public class NoticeAnim : MonoBehaviour
{
    [Header("신호 설정")]
    public SignalType signalType;

    [Header("시각 요소")]
    public SpriteRenderer spriteRenderer;
    public ParticleSystem spawnParticle;
    public ParticleSystem hitParticle;

    [Header("슬라이드 설정")]
    public float slideDistance = 2f; 

    private Vector3 originalScale;
    private Color originalColor;
    private Quaternion originalRotation; // 🔥 회전 초기화를 위해 추가
    private Sequence currentAnim; 

    private void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation; // 시작 시점의 회전 저장
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    // 1. 생성 연출 (호출 시 무조건 초기화)
    public void SpawnSignal(float bpm)
    {
        float beatDuration = 60f / bpm; 
        
        // 🔥 [중요] 이전 연출 강제 종료 및 물리 상태 초기화
        if (currentAnim != null) currentAnim.Kill();
        
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        transform.rotation = originalRotation; // 회전값 원상복구
        spriteRenderer.color = originalColor; // 투명도/색상 원상복구
        
        // All In 1 Sprite의 특정 속성(예: HitEffect)을 쓴다면 여기서 0으로 초기화
        // spriteRenderer.material.SetFloat("_HitEffectBlend", 0f);

        if (spawnParticle != null) spawnParticle.Play();

        currentAnim = DOTween.Sequence();

        if (signalType == SignalType.Tap)
        {
            currentAnim.Append(transform.DOScale(originalScale * 1.1f, beatDuration * 0.3f).SetEase(Ease.OutBack))
                       .Append(transform.DOScale(originalScale, beatDuration * 0.1f));
        }
        else 
        {
            // 스와이프류는 현재 위치를 기준으로 슬라이드 시작점 계산
            // (이미 매니저가 배치와 회전을 끝낸 상태라고 가정)
            Vector3 targetPosition = transform.position; 
            transform.position = targetPosition - (transform.up * slideDistance);

            currentAnim.Append(transform.DOMove(targetPosition, beatDuration * 0.4f).SetEase(Ease.OutCubic))
                       .Join(transform.DOScale(originalScale, beatDuration * 0.4f).SetEase(Ease.OutBack));
        }
    }

    // 2. 패링 직전 연출 (Telegraph)
    public void PreHit(float bpm)
    {
        float beatDuration = 60f / bpm;
        if (currentAnim != null) currentAnim.Kill();

        currentAnim = DOTween.Sequence();
        
        // 살짝 커지며 강조 (색상을 흰색으로 강조)
        currentAnim.Append(transform.DOScale(originalScale * 1.2f, beatDuration * 0.2f).SetEase(Ease.OutQuad))
                   .Join(spriteRenderer.DOColor(Color.white, beatDuration * 0.2f));
    }

    // 3. 패링 성공 연출 (Hit)
    public void HitSignal(float bpm)
    {
        float beatDuration = 60f / bpm;
        if (currentAnim != null) currentAnim.Kill(); 
        
        if (hitParticle != null) hitParticle.Play();

        currentAnim = DOTween.Sequence();
        currentAnim.Append(transform.DOScale(originalScale * 1.5f, beatDuration * 0.15f).SetEase(Ease.OutExpo))
                   .Join(spriteRenderer.DOFade(0f, beatDuration * 0.15f))
                   .OnComplete(() => gameObject.SetActive(false));
    }

    // 4. Miss 연출 (Fail)
    public void MissSignal()
    {
        if (currentAnim != null) currentAnim.Kill();

        currentAnim = DOTween.Sequence();
        // 힘없이 회전하며 아래로 낙하
        currentAnim.Append(transform.DOMoveY(transform.position.y - 2f, 0.5f).SetEase(Ease.InQuad))
                   .Join(transform.DORotate(new Vector3(0, 0, 15f), 0.5f, RotateMode.LocalAxisAdd))
                   .Join(spriteRenderer.DOFade(0f, 0.5f))
                   .OnComplete(() => gameObject.SetActive(false));
    }
}