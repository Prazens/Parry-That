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

    public abstract float preAttackDelay { get; } // 공격 명령으로부터 판정까지 걸리는 시간
    public abstract float disappearDuration { get; }
    protected Direction direction;
    protected bool isHolding = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public virtual void SetDirection(int direction)
    {
        this.direction = (Direction)direction;
        animator.SetInteger("direction", direction);
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

    public abstract void OnHit(AttackType attackType);

    public virtual void OnClear()
    {
        animator.SetBool("isClear", true);
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
}
