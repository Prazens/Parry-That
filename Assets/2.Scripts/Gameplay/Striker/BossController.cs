using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 0;
    [SerializeField] private int hp;

    [Header("Links")]
    [SerializeField] private ParriedProjectileManager parryFX;
    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private StageFlowManager stageFlowManager;
    [SerializeField] public Animator bossAnimator;

    private readonly List<StrikerController> minions = new();

    [Header("HP Bar")]
    [SerializeField] private GameObject hpBarPrefab;
    private GameObject hpBar;
    private Transform hpControl;

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

        // -------------------------
        // HP Bar spawn
        // -------------------------
        if (hpBarPrefab != null)
        {
            hpBar = Instantiate(hpBarPrefab, transform);
            hpBar.transform.localPosition = Vector3.down * 2f;

            // 기존처럼 첫 번째 자식을 HP 컨트롤로 가정
            if (hpBar.transform.childCount > 0)
            {
                hpControl = hpBar.transform.GetChild(0);
                hpControl.localScale = new Vector3(0, 1, 1);
            }
            else
            {
                Debug.LogWarning("[BossController] hpBarPrefab has no child for hpControl.");
            }
        }
        else
        {
            Debug.LogWarning("[BossController] hpBarPrefab is not assigned.");
        }
    }

    private void Start()
    {
        // Initialize()가 나중에 호출되어 hp/maxHp를 세팅하는 구조라면
        // Start에서 hp=maxHp는 사실상 의미 없거나 덮어쓰는 역할만 함.
        // 일단 유지하되, 원치 않으면 제거해도 됨.
        hp = maxHp;
        UpdateHpUI();
    }

    public void Initialize(int _hp)
    {
        maxHp += _hp;
        hp = maxHp;
        UpdateHpUI();
    }

    public void clearHp()
    {
        hp = 101;
        maxHp = 101;
        UpdateHpUI();
    }

    public void RegisterStriker(StrikerController sc)
    {
        if (sc == null) return;

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

        if (maxHp <= 0) return;

        hp -= Mathf.Max(1, dmg);
        hp = Mathf.Clamp(hp, 0, maxHp);

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
                case Direction.Up: bossAnimator.SetTrigger("UpHit"); break;
                case Direction.Down: bossAnimator.SetTrigger("DownHit"); break;
                case Direction.Left: bossAnimator.SetTrigger("LeftHit"); break;
                case Direction.Right: bossAnimator.SetTrigger("RightHit"); break;
            }
        }
    }

    private void UpdateHpUI()
    {
        if (hpControl == null) return;

        float ratio = (maxHp <= 0) ? 0f : (float)hp / maxHp;
        hpControl.localScale = new Vector3(1f - ratio, 1f, 1f);
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
