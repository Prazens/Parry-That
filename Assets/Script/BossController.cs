using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;  
    [SerializeField] private int hp;

    [Header("Links")]
    [SerializeField] private ParriedProjectileManager parryFX;
    [SerializeField] private StrikerManager strikerManager;   
    [SerializeField] private StageManager stageManager;
    public Animator bossAnimator;

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

    private void Start()
    {
        hp = maxHp;
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

    public void TakeDamage(int dmg, AttackType type, Direction dir)
    {
        Debug.Log("boss Damaged");
        hp -= Mathf.Max(1, dmg);
        UpdateHpUI();
        if (hp == 0)
        {
            OnBossDead();
            return;
        }

        if (bossAnimator != null)
        {
            switch (dir)
            {
                case Direction.Up:
                    bossAnimator.SetTrigger("UpHit");
                    break;
                case Direction.Down:
                    bossAnimator.SetTrigger("DownHit");
                    break;
                case Direction.Left:
                    bossAnimator.SetTrigger("LeftHit");
                    break;
                case Direction.Right:
                    bossAnimator.SetTrigger("RightHit");
                    break;
            }
        }
    }

    private void UpdateHpUI()
    {
        hpControl.transform.localScale = new Vector3(1 - ((float)hp / maxHp), 1, 1);
    }

    public void OnMinionPrepare(Direction dir, int noteType, float arriveTime)
    {
        Debug.Log("Boss Prepare Attack");
        if (bossAnimator != null)
        {
            switch (dir)
            {
                case Direction.Up:
                    bossAnimator.SetTrigger("UpAttack");
                    break;
                case Direction.Down:
                    bossAnimator.SetTrigger("DownAttack");
                    break;
                case Direction.Left:
                    bossAnimator.SetTrigger("LeftAttack");
                    break;
                case Direction.Right:
                    bossAnimator.SetTrigger("RightAttack");
                    break;
            }
        }
    }

    private void OnBossDead()
    {
        if(bossAnimator != null)
        {
            bossAnimator.SetTrigger("BossDie");
        }
    }
}
