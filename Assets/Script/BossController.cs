using UnityEngine;
using System;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;   // �� ��Ʈ �� ������ �ʱ�ȭ ����
    [SerializeField] private int hp;
    [SerializeField] private Transform hpFill;  // ��� HP���� fill Ʈ������(���� x ������ 0~1)

    [Header("Links")]
    [SerializeField] private ParriedProjectileManager parryFX; // ���� ��Ʈ ����Ʈ
    [SerializeField] private StrikerManager strikerManager;    // ���� �Ŵ���
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

        // ���� �ǰ� ����Ʈ (���� ����)
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
        // ��� ��Ʈ����Ŀ ����(���⸸ ���߰� �������)
        foreach (var sc in minions)
        {
            if (sc != null) sc.beCleared(); // �ʿ�� ���� ����-��� ���� ���� �б�
        }
        // �������� Ŭ���� ó��
        if (stageManager != null) { /* stageManager �� Ŭ���� �帧�� ������� �ð� ���� or ��� EndStage�� ���� */ }
    }
}
