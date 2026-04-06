using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RangeStrikerVisual : StrikerVisual
{
    [SerializeField] private List<Projectile> projectilePrefabs; // 투사체 프리팹

    public override float preAttackDelay => 0.5f;
    public override float disappearDuration => 1.0f;

    public override GameObject OnAttackStart(StrikerAttackContext context)
    {
        if (context.attackType == AttackType.HoldFinishStrong && !isHolding) return null;

        if (context.attackType == AttackType.Normal || context.attackType == AttackType.Strong)
        {
            GameObject projectile = FireProjectile(StageFlowManager.Instance.BeatToSec(context.arriveBeat), context.attackType);
            animator.SetTrigger("Attack");
            return projectile;
        }

        return null;
    }

    public override void OnHit(AttackType attackType)
    {
        base.OnHit(attackType);
        animator.SetTrigger("isDamaged");
    }

    // 투사체 발사
    private GameObject FireProjectile(float arriveSec, AttackType attackType)
    {
        int type = (int)attackType;
        if (type < 0 || type >= projectilePrefabs.Count)
        {
            Debug.LogWarning($"{name}.FireProjectile: projectile prefab not exists for attackType {type}");
            return null;
        }
        Projectile selectedProjectile = projectilePrefabs[(int)attackType];

        Vector3 projectilePos = transform.position;
        switch (location)
        {
            case Direction.Left:
                projectilePos += new Vector3(-2f, 0, 0);
                break;
            case Direction.Right:
                projectilePos += new Vector3(2f, 0, 0);
                break;
        }
        // 투사체 생성
        Projectile projectile = Instantiate(selectedProjectile, projectilePos, Quaternion.identity);
        projectile.Setup(location, projectilePos, targetPosition, arriveSec);

        return projectile.gameObject;
    }

    protected override void ActHoldStart(Judgeable judgeable)
    {
        holdSpriteAnimator?.SetTrigger("holdStart");
 
        //dynamicUIManager.CutInDisplay(StageFlowManager.Instance.BeatToSec(judgeable.nextArriveBeat));
 
        isHolding = true;
    }

    protected override void ActHoldFinish(Judgeable judgeable)
    {
        holdSpriteAnimator?.SetTrigger("holdFinish");
 
        isHolding = false;
        
        dynamicUIManager.CutInDisplay(0, true);
    }
}
