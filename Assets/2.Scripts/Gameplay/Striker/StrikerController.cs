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

    public StrikerManager manager;
    public JudgeSystem judgeSystem;

    // striker 자체에 들어가는 script
    public PlayerManager playerManager; // Player 정보 저장
    public Direction location; // 위치 방향

    //투사체 발사 시의 !관련
    [SerializeField] private GameObject exclamationPrefab; // 공통 느낌표 프리팹
    // 🔹 `List<Sprite>`로 변경 (느낌표 타입별 이미지 저장)
    [SerializeField] private List<Sprite> exclamationSprites = new List<Sprite>();
    private Transform exclamationParent; // 느낌표 표시 위치
    private List<GameObject> prepareExclamation = new List<GameObject>(); // 느낌표 오브젝트 저장
    public GameObject holdExclamation; // 홀드 느낌표
    private float spacing = 0.25f;

    private float musicOffset => (playerManager != null) ? playerManager.musicOffset : 2;

    public BossController boss;
    public bool isBossMinion = false;


    private void Start()
    {
        //SetupExclamationParent();// exclamationParent 자동 생성
    }

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

    private void HandleMeleeMovement()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        // 공격할 때 실행
        //exclamationRelocation();
    }

    public void ActMeleeHoldFinish()
    {
        holdExclamation.GetComponent<holdExclamation>().ForceStop();

        // 미스났는데도 느낌표 남아있는 거 방지
        while (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]); // 가장 오래된 느낌표 제거
            prepareExclamation.RemoveAt(0);
        }
    }

    private void HandleProjectileAttack()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        // HoldStart || HoldFinishStrong일 때 실행
        //exclamationRelocation(); // 느낌표 한 칸 제거
    }

    private void ActRangeHoldFinish()
    {
        // 근접과 동일하게 느낌표 처리
    }

    private void PrepareForAttack()
    {
        //if ((noteType != AttackType.HoldFinishStrong && noteType != AttackType.StreamStart) || isHolding || isRenta)
        //ShowExclamation(noteType); // 느낌표 표시

        // 보스 로직
        //if (isBossMinion && boss != null)
        //{
        //    boss.OnMinionPrepare(location, (int)noteType, arriveBeat);
        //}

        // 연타 로직
        //if (!isMelee && noteType == AttackType.StreamStart)
        //{
        //    var j = new Judgeable(AttackType.StreamStart, arriveTime, location, this, null, this.ActStreamStart);
        //    j.SetStreamCount(CalcStreamCountForThisSegment(currentNoteIndex));
        //    judgeSystem.EnqueueJudgeable(j);
        //    judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.StreamFinish, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActStreamFinish));
        //}
    }

    private void FireProjectile(float time, int index)
    {
        // 발사 시 느낌표 제거 (좌측부터)
        //exclamationRelocation();
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
