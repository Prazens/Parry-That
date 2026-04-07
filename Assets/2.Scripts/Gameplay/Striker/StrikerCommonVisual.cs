using System.Collections;
using UnityEngine;

/// <summary>
/// Striker의 일반적인 애니메이션을 담당.
/// </summary>
public class StrikerCommonVisual : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particleSystemGreen; // 초록색 파티클 시스템

    // Animation Durations
    private float spawnMoveDuration => 1.0f;
    public float disappearDuration => 1.0f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Init(Vector3 defaultPosition)
    {
        // 스트라이커를 화면 밖에서 시작 위치로 이동
        StartCoroutine(LerpPosition(transform.position, defaultPosition, StageFlowManager.Instance.currentTime + spawnMoveDuration));
    }

    public void OnNotice()
    {
        animator.SetTrigger("Prepare");
    }

    public void OnAttackStart()
    {
        animator.SetTrigger("Attack");
    }

    public void OnJudge()
    {

    }

    public void OnHit()
    {
        animator.SetTrigger("Damaged");
    }

    public void OnClear()
    {
        animator.SetBool("isClear", true);
        particleSystemGreen?.Play();
        Destroy(gameObject, disappearDuration);
    }

    private IEnumerator LerpPosition(Vector3 start, Vector3 end, float endSec)
    {
        var flow = StageFlowManager.Instance;
        float duration = endSec - flow.currentTime;
        float fractionOfJourney;

        while (flow.currentTime < endSec)
        {
            fractionOfJourney = (endSec - flow.currentTime) / duration;
            transform.position = Vector3.Lerp(end, start, fractionOfJourney);
            yield return null;
        }

        transform.position = end;
    }
}
