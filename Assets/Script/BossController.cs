using UnityEngine;
using System;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;   // 총 노트 수 합으로 초기화 가능
    [SerializeField] private int hp;
    [SerializeField] private Transform hpFill;  // 상단 HP바의 fill 트랜스폼(로컬 x 스케일 0~1)

    [Header("Links")]
    [SerializeField] private ParriedProjectileManager parryFX; // 보스 히트 이펙트
    [SerializeField] private StrikerManager strikerManager;    // 기존 매니저
    [SerializeField] private StageManager stageManager;

    private readonly List<StrikerController> minions = new();

    public GameObject hpBarPrefab;
    public GameObject hpBar;
    public Transform hpControl;

    public void Initialize(int _hp)
    {
        hp = maxHp = _hp;
        hpBar = Instantiate(hpBarPrefab, transform);
        hpBar.transform.localPosition = Vector3.down * 2f;
        hpControl = hpBar.transform.GetChild(0);
        hpControl.transform.localScale = new Vector3(0, 1, 1);
    }

    public void RegisterStriker(StrikerController sc)
    {
        if (!minions.Contains(sc))
        {
            minions.Add(sc);
            sc.boss = this;
            sc.isBossMinion = true;
        }
    }

    public void TakeDamage(int dmg, AttackType type)
    {
        if (hp <= 0) return;

        hp -= Mathf.Max(1, dmg);
        UpdateHpUI();

        // 보스 피격 이펙트 (방향 무시)
        if (parryFX != null) parryFX.ParryTusacheBoss((int)type, transform);

        if (hp <= 0)
        {
            OnBossDead();
        }
    }

    private void UpdateHpUI()
    {
        hpControl.transform.localScale = new Vector3(1 - ((float)hp / maxHp), 1, 1);
    }

    private void OnBossDead()
    {
        // 모든 스트라이커 정리(연출만 멈추고 사라지게)
        foreach (var sc in minions)
        {
            if (sc != null) sc.beCleared(); // 필요시 전용 보스-사망 연출 별도 분기
        }
        // 스테이지 클리어 처리
        if (stageManager != null) { /* stageManager 쪽 클리어 흐름은 기존대로 시간 종료 or 즉시 EndStage도 가능 */ }
    }
}
