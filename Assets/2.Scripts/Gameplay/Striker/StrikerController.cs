using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격 시 필요한 정보를 담는 구조체.
/// </summary>
public struct StrikerAttackContext
{
    public float currentSec;
    public float arriveBeat;
    public float nextArriveBeat;
    public AttackType attackType;
    public bool isLastInBurst;
    public bool isFinalAttack;

    public StrikerAttackContext(float currentSec, float arriveBeat, float nextArriveBeat, AttackType attackType, bool isLastInBurst, bool isFinalAttack)
    {
        this.currentSec = currentSec;
        this.arriveBeat = arriveBeat;
        this.nextArriveBeat = nextArriveBeat;
        this.attackType = attackType;
        this.isLastInBurst = isLastInBurst;
        this.isFinalAttack = isFinalAttack;
    }
}

/// <summary>
/// 각 Striker의 세부 컴포넌트들을 연결하는 핵심 로직.
/// </summary>
public class StrikerController : MonoBehaviour
{
    [Header("Striker Components")]
    [SerializeField] private StrikerAttack attack;
    [SerializeField] private StrikerVisual visual;
    [SerializeField] private StrikerSound sound;

    public StrikerAttack Attack => attack;
    public StrikerVisual Visual => visual;
    public StrikerSound Sound => sound;

    // References
    public StrikerManager manager;
    public JudgeSystem judgeSystem;
    public PlayerManager playerManager; // Player 정보 저장

    public Direction location; // 위치 방향

    private float musicOffset => (playerManager != null) ? playerManager.musicOffset : 2;

    public BossController boss;
    public bool isBossMinion = false;

    public void Initialize(PlayerManager targetPlayer, Direction location)
    {
        playerManager = targetPlayer;
        this.location = location;

        attack.Init(this);
        visual.Init(this, location, transform.position, playerManager.transform.position);
    }

    public float BeatToSec(float beat) => manager.BeatToSec(beat, musicOffset);

    public void OnNotice(float arriveBeat, float nextArriveBeat, AttackType attackType)
    {
        attack.OnNotice(arriveBeat, nextArriveBeat, attackType);
        sound.PlayPrepareSound(attackType);
    }

    public void OnAttackStart(StrikerAttackContext context)
    {
        GameObject projectile = visual.OnAttackStart(context);
        attack.OnAttackStart(context, projectile);
    }

    public void OnJudge(Judgeable judgeable, bool isHit)
    {
        visual.OnJudge(judgeable, isHit);
        sound.PlayHoldSound(judgeable.attackType);

        if (isHit)
        {
            OnHit(judgeable.attackType);
        }
    }

    private void OnHit(AttackType attackType)
    {
        visual.OnHit(attackType);
        sound.PlayParrySound(attackType);
    }

    public void OnClear()
    {
        visual.OnClear();
    }

    private void PrepareForAttack()
    {
        // 보스 로직
        //if (isBossMinion && boss != null)
        //{
        //    boss.OnMinionPrepare(location, (int)noteType, arriveBeat);
        //}
    }

    public void ClearProjectiles()
    {
        while (judgeSystem.CountJudgeable(location) > 0)
        {
            GameObject projectile = judgeSystem.DequeueJudgeable(location).judgeableObject;
            if (projectile != null)
            {
                Destroy(projectile); // Projectile 삭제
            }
        }
    }
}
