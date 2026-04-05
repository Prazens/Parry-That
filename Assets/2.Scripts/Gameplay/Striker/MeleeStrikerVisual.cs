using System.Collections;
using UnityEngine;

public sealed class MeleeStrikerVisual : StrikerVisual
{
    [SerializeField] private AnimatorOverrideController[] directionOverrides;
    [SerializeField] private Animator bladeAnimator;

    private readonly float attackMoveTime = 0.1f;
    private readonly float attackAnimTime = 0.02f;
    public override float preAttackDelay => attackMoveTime + attackAnimTime;
    public override float disappearDuration => 2.5f;

    // Melee 전용 변수
    private bool isLastInBurst = false;
    private bool isFinalAttack = false;
    private bool isMoved = false;
    private bool isMoving = false;

    protected override void SetLocation(Direction location)
    {
        base.SetLocation(location);

        animator.runtimeAnimatorController = directionOverrides[(int)location - 1];
        bladeAnimator.SetInteger("bladeDirection", (int)location);
    }

    public override void SetAttackType(int attackType)
    {
        base.SetAttackType(attackType);

        bladeAnimator.SetInteger("attackType", attackType);
    }

    public override GameObject OnAttackStart(StrikerAttackContext context)
    {
        this.isLastInBurst = context.isLastInBurst;
        this.isFinalAttack = context.isFinalAttack;

        if (context.attackType == AttackType.HoldFinishStrong && !isHolding) return null;

        if (context.attackType != AttackType.HoldFinishStrong)
        {
            StartCoroutine(ActMeleeAttack(context.attackType));
        }

        return null;
    }

    public override void OnHit(AttackType attackType)
    {
        base.OnHit(attackType);

        if (attackType == AttackType.Normal || attackType == AttackType.Strong ||
            attackType == AttackType.HoldFinishStrong)
        {
            if (isLastInBurst)
            {
                if (isFinalAttack)
                {
                    StartCoroutine(ActMeleeClear());
                }
                else
                {
                    StartCoroutine(ActMeleeAttackBack());
                }
            }
        }
    }

    private IEnumerator ActMeleeAttack(AttackType attackType)
    {
        //공격 이전에 출발
        yield return ActMeleeAttackGo();

        //공격 애니메이션 작용
        SetAttackType((int)attackType);

        if (attackType != AttackType.HoldStart)
        {
            int randomNum = Random.Range(0, 2);
            animator.SetInteger("randomSelector", randomNum);
            bladeAnimator.SetInteger("randomSelector", randomNum);
        }

        animator.SetTrigger("Attack");
        bladeAnimator.SetTrigger("bladePlay");

        if (attackType == AttackType.HoldStart)
        {
            transform.GetChild(0).transform.localPosition = DirTool.TranstoVec(DirTool.ReverseDir(location)) * 2f;
            yield break;
        }

        if (isLastInBurst)
        {
            // 애니메이션 지속 시간을 고려하여 대기 후 실행
            yield return new WaitForSeconds(0.1f); // 공격 후 0.1초 딜레이

            if (isFinalAttack)
            {
                StartCoroutine(ActMeleeClear());
            }
            else
            {
                StartCoroutine(ActMeleeAttackBack());
            }
        }
    }

    protected override void ActHoldStart(Judgeable judgeable)
    {
        animator.SetBool("isHolding", true);
        //dynamicUIManager.CutInDisplay(StageFlowManager.Instance.BeatToSec(judgeable.nextArriveBeat));

        isHolding = true;
    }
 
    protected override void ActHoldFinish(Judgeable judgeable)
    {
        animator.SetBool("isHolding", false);
        bladeAnimator.SetTrigger("bladeHoldFinish");
        transform.GetChild(0).transform.localPosition = Vector3.zero;

        isHolding = false;

        dynamicUIManager.CutInDisplay(0, true);
    }

    private IEnumerator ActMeleeAttackGo()
    {
        if (isMoved || isMoving) yield break;

        isMoving = true;

        yield return LerpPosition(defaultPosition, targetPosition, attackMoveTime);

        isMoved = true;
        isMoving = false;
    }

    private IEnumerator ActMeleeAttackBack()
    {
        if (!isMoved || isMoving) yield break;

        isMoved = false;
        isMoving = true;
        animator.SetBool("movingBack", true);

        yield return LerpPosition(targetPosition, defaultPosition, attackMoveTime / 2.0f);

        animator.SetBool("movingBack", false);
        isMoving = false;
    }

    private IEnumerator ActMeleeClear()
    {
        if (!isMoved || isMoving) yield break;

        isMoved = false;
        isMoving = true;
        animator.SetTrigger("Bye");

        yield return LerpPosition(targetPosition, defaultPosition, 0.3f);

        isMoving = false;
    }
}
