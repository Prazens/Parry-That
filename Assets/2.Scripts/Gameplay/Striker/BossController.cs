/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ParriedProjectileManager parryFX;
    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private StageFlowManager stageFlowManager;
    [SerializeField] public Animator bossAnimator;

    private readonly List<StrikerController> minions = new();

    private void Awake()
    {
        // -------------------------
        // Auto-wiring for prefab spawn
        // -------------------------
        if (stageFlowManager == null) stageFlowManager = StageFlowManager.Instance;
        if (strikerManager == null) strikerManager = FindObjectOfType<StrikerManager>();
        if (parryFX == null) parryFX = FindObjectOfType<ParriedProjectileManager>();

        if (bossAnimator == null)
        {
            bossAnimator = GetComponent<Animator>();
        }
    }

    public void RegisterStriker(StrikerController sc)
    {
        if (sc == null) return;

        if (!minions.Contains(sc))
        {
            minions.Add(sc);
        }
    }

    public void TakeDamage(int dmg, AttackType type, Direction dir)
    {
        if (bossAnimator != null)
        {
            switch (dir)
            {
                case Direction.Up: bossAnimator.SetTrigger("UpHit"); break;
                case Direction.Down: bossAnimator.SetTrigger("DownHit"); break;
                case Direction.Left: bossAnimator.SetTrigger("LeftHit"); break;
                case Direction.Right: bossAnimator.SetTrigger("RightHit"); break;
            }
        }
    }

    public void OnMinionPrepare(Direction dir, int noteType, float arriveTime)
    {
        Debug.Log("Boss Prepare Attack");

        if (bossAnimator != null)
        {
            switch (dir)
            {
                case Direction.Up: bossAnimator.SetTrigger("UpAttack"); break;
                case Direction.Down: bossAnimator.SetTrigger("DownAttack"); break;
                case Direction.Left: bossAnimator.SetTrigger("LeftAttack"); break;
                case Direction.Right: bossAnimator.SetTrigger("RightAttack"); break;
            }
        }
    }

    private void OnBossDead()
    {
        if (bossAnimator != null)
            bossAnimator.SetTrigger("BossDie");

        var chibi = transform.Find("boss_chibi");
        chibi.gameObject.SetActive(true);
    }
}
*/
