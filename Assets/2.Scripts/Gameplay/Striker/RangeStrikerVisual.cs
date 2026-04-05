using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RangeStrikerVisual : StrikerVisual
{
    [SerializeField] private List<projectile> projectilePrefabs; // 투사체 프리팹

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
        projectile selectedProjectile = projectilePrefabs[(int)attackType];

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
        projectile projectile = Instantiate(selectedProjectile, projectilePos, Quaternion.identity);
        switch (location)
        {
            case Direction.Up:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Left:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Right:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            default:
                break;
        }

        // 투사체에 타겟 설정
        projectile.target = controller.playerManager.transform; // 플레이어를 타겟으로 설정
        projectile.owner = controller; // 소유자로 현재 스트라이커 설정
        projectile.arriveTime = arriveSec;
        projectile.type = type;

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
