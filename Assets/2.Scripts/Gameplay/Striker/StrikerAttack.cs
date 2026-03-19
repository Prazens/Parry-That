using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격을 수행, 즉 Judgeable을 생성하는 역할.
/// </summary>
public class StrikerAttack : MonoBehaviour
{
    protected StrikerController controller;
    protected JudgeSystem judgeSystem;
    protected Direction location;

    public virtual void Init(StrikerController _controller)
    {
        controller = _controller;
        judgeSystem = _controller.judgeSystem;
        location = _controller.location;
    }

    public virtual void OnNotice(float arriveBeat, float nextArriveBeat, AttackType attackType)
    {
        if (attackType == AttackType.HoldStart)
        {
            judgeSystem.EnqueueJudgeable(new Judgeable(attackType, arriveBeat, nextArriveBeat, location, controller, null, controller.ActHoldStart));
        }
        else if (attackType == AttackType.HoldFinishStrong)
        {
            judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.HoldStop, arriveBeat, nextArriveBeat, location, controller, null, controller.ActHoldFinish));
        }
    }

    public virtual void OnAttackStart(StrikerAttackContext context, GameObject projectile)
    {
        if (context.attackType == AttackType.HoldStart || context.attackType == AttackType.HoldFinishStrong) return;
        judgeSystem.EnqueueJudgeable(new Judgeable(context.attackType, context.arriveBeat, context.nextArriveBeat, location, controller, projectile, null));
    }
}
