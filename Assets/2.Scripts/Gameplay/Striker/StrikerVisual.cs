using System.Collections;
using UnityEngine;

/// <summary>
/// 애니메이션, 움직임, 투사체 등, 눈에 보이는 연출을 담당.
/// Striker의 종류별로 하위 클래스를 가짐.
/// </summary>
public abstract class StrikerVisual : MonoBehaviour
{
    protected StrikerController controller;
    public DynamicUIManager dynamicUIManager;

    [SerializeField] protected Animator animator;
    [SerializeField] protected Animator holdSpriteAnimator = null;
    [SerializeField] protected ParticleSystem particleSystemGreen; // 초록색 파티클 시스템

    // Animation Durations
    public abstract float preAttackDelay { get; } // 공격 명령으로부터 판정까지 걸리는 시간
    private float spawnMoveDuration => 1.0f; // 이동 시간
    public abstract float disappearDuration { get; }

    // Location & Positions
    protected Direction location;
    protected Vector3 defaultPosition;
    protected Vector3 spawnPosition; // 기본 위치에서 화면 밖으로 보정된 위치
    protected Vector3 targetPosition; // 플레이어의 위치에서 약간 보정된 위치
    private float spawnOffset => 3.0f;
    private float targetOffset => 2.0f;

    protected bool isHolding = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public virtual void Init(StrikerController _controller, Direction location, Vector3 defaultPosition, Vector3 targetPosition)
    {
        controller = _controller;
        SetLocation(location);
        SetPosition(defaultPosition, targetPosition);

        // 스트라이커를 화면 밖에서 시작 위치로 이동
        StartCoroutine(LerpPosition(spawnPosition, defaultPosition, spawnMoveDuration));
    }

    private void OnEnable()
    {
        if (controller == null)
            return;

        // 스트라이커를 화면 밖에서 시작 위치로 이동
        StartCoroutine(LerpPosition(spawnPosition, defaultPosition, spawnMoveDuration));
    }

    protected virtual void SetLocation(Direction location)
    {
        this.location = location;
    }

    protected void SetPosition(Vector3 defaultPosition, Vector3 targetPosition)
    {
        this.defaultPosition = defaultPosition;
        this.spawnPosition = AdjustPosition(defaultPosition, spawnOffset);
        this.targetPosition = AdjustPosition(targetPosition, targetOffset);

        transform.position = spawnPosition;
    }

    private Vector3 AdjustPosition(Vector3 position, float offset)
    {
        switch (location)
        {
            case Direction.Up:
                position += Vector3.up * offset;
                break;
            case Direction.Down:
                position += Vector3.down * offset;
                break;
            case Direction.Left:
                position += Vector3.left * offset;
                break;
            case Direction.Right:
                position += Vector3.right * offset;
                break;
        }
        return position;
    }

    public virtual void SetAttackType(int attackType)
    {
        animator.SetInteger("attackType", attackType);
    }

    public abstract GameObject OnAttackStart(StrikerAttackContext context);

    public virtual void OnJudge(Judgeable judgeable, bool isHit)
    {
        if (judgeable.attackType == AttackType.HoldStart)
        {
            ActHoldStart(judgeable);
        }
        else if (judgeable.attackType == AttackType.HoldFinishStrong)
        {
            ActHoldFinish(judgeable);
        }
    }

    public virtual void OnHit(AttackType attackType)
    {
        dynamicUIManager?.ShowParticle(location, false);
    }

    public virtual void OnClear()
    {
        animator.SetTrigger("Cleared");
        particleSystemGreen?.Play();
        StartCoroutine(DisappearAfterAnim());
    }

    protected abstract void ActHoldStart(Judgeable judgeable);

    protected abstract void ActHoldFinish(Judgeable judgeable);

    private IEnumerator DisappearAfterAnim()
    {
        yield return new WaitForSeconds(disappearDuration);

        gameObject.SetActive(false);
    }

    protected IEnumerator LerpPosition(Vector3 start, Vector3 end, float duration)
    {
        var flow = StageFlowManager.Instance;
        float startSec = flow.currentTime;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed = flow.currentTime - startSec;
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        transform.position = end;
    }
}
