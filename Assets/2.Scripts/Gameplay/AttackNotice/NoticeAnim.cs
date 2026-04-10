using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public enum SignalType { Tap, Swipe, GhostSwipe }

public class NoticeAnim : MonoBehaviour
{
    [Header("신호 설정")]
    public SignalType signalType;

    [Header("시각 요소")]
    public Transform visualRoot; // 🔥 애니메이션을 담당할 자식 오브젝트 (이미지와 파티클을 담음)
    public Image image;
    public ParticleSystem spawnParticle;
    public ParticleSystem hitParticle;

    [Header("슬라이드 설정")]
    public float slideDistance = 2f; 

    private Vector3 originalScale;
    private Color originalColor;
    private Quaternion originalRotation; 
    private Sequence currentAnim;

    public void Init()
    {
        // 뼈대(transform)가 아니라 연출을 담당할 자식(visualRoot)의 초기값을 기억합니다.
        originalScale = visualRoot.localScale;
        originalRotation = visualRoot.localRotation; 
        if (image != null)
            originalColor = image.color;
    }

    // 1. 생성 연출 (호출 시 무조건 초기화)
    public void SpawnSignal(float bpm)
    {
        float beatDuration = 60f / bpm; 
        
        if (currentAnim != null) currentAnim.Kill();
        
        gameObject.SetActive(true);
        
        // 자식 오브젝트의 물리 상태 초기화
        visualRoot.localScale = Vector3.zero;
        visualRoot.localRotation = originalRotation; 
        visualRoot.localPosition = Vector3.zero; // 부모의 정중앙으로 초기화
        image.color = originalColor;

        if (spawnParticle != null) spawnParticle.Play();

        currentAnim = DOTween.Sequence();

        if (signalType == SignalType.Tap)
        {
            // [탭] 제자리에서 커짐
            currentAnim.Append(visualRoot.DOScale(originalScale * 1.1f, beatDuration * 0.3f).SetEase(Ease.OutBack))
                       .Append(visualRoot.DOScale(originalScale, beatDuration * 0.1f));
        }
        else 
        {
            // [스와이프] 외부 매니저가 "부모(transform)"의 위치와 회전을 이미 잡아두었다고 가정합니다.
            // 자식(visualRoot)은 로컬 Y축의 아래쪽(-slideDistance)에서 출발하여, 부모의 중심점(Vector3.zero)으로 슬라이드합니다.
            visualRoot.localPosition = new Vector3(0f, -slideDistance, 0f);

            // 🔥 월드 좌표 DOMove -> 로컬 좌표 DOLocalMove 로 변경! (부모가 이동해도 목적지가 꼬이지 않음)
            currentAnim.Append(visualRoot.DOLocalMove(Vector3.zero, beatDuration * 0.4f).SetEase(Ease.OutCubic))
                       .Join(visualRoot.DOScale(originalScale, beatDuration * 0.4f).SetEase(Ease.OutBack));
        }
    }

    // 2. 패링 직전 연출 (Telegraph)
    public void PreHit(float bpm)
    {
        float beatDuration = 60f / bpm;
        if (currentAnim != null) currentAnim.Kill();

        currentAnim = DOTween.Sequence();
        
        currentAnim.Append(visualRoot.DOScale(originalScale * 1.2f, beatDuration * 0.2f).SetEase(Ease.OutQuad))
                   .Join(image.DOColor(Color.white, beatDuration * 0.2f));
    }

    // 3. 패링 성공 연출 (Hit)
    public void HitSignal(float bpm, Action<NoticeAnim> onDestroy)
    {
        float beatDuration = 60f / bpm;
        if (currentAnim != null) currentAnim.Kill(); 
        
        if (hitParticle != null) hitParticle.Play();

        currentAnim = DOTween.Sequence();
        currentAnim.Append(visualRoot.DOScale(originalScale * 1.5f, beatDuration * 0.15f).SetEase(Ease.OutExpo))
                   .Join(image.DOFade(0f, beatDuration * 0.15f))
                   .OnComplete(() => onDestroy?.Invoke(this));
    }

    // 4. Miss 연출 (Fail)
    public void MissSignal(float bpm, Action<NoticeAnim> onDestroy)
    {
        float beatDuration = 60f / bpm;
        if (currentAnim != null) currentAnim.Kill();

        currentAnim = DOTween.Sequence();
        // 낙하는 화면 아래로 떨어져야 하므로 월드 좌표 Y축 이동을 유지해도 무방하지만, 
        // 매니저의 재정렬과 겹치는 것을 대비해 안전하게 visualRoot를 조작합니다.
        currentAnim.Append(visualRoot.DOMoveY(visualRoot.position.y - 2f, beatDuration * 0.15f).SetEase(Ease.InQuad))
                   .Join(visualRoot.DORotate(new Vector3(0, 0, 15f), beatDuration * 0.15f, RotateMode.LocalAxisAdd))
                   .Join(image.DOFade(0f, beatDuration * 0.15f))
                   .OnComplete(() => onDestroy?.Invoke(this));
    }
}