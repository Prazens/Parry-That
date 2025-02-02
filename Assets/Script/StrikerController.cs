using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerController : MonoBehaviour
{
    // striker 자체에 들어가는 script
    [SerializeField] private List<GameObject> projectilePrefabs; // 투사체 프리팹
    public PlayerManager playerManager; // Player 정보 저장
    public UIManager uiManager;
    [SerializeField] public ChartData chartData; // 채보 데이터
    [SerializeField] public int hp; // 스트라이커 HP
    private int initialHp; // 스트라이커 initialHp
    [SerializeField] public int bpm; // BPM
    public Direction location; // 위치 방향
    private int currentNoteIndex = 0; // 현재 채보 인덱스
    [SerializeField] private Animator animator;
    // 임시로 발사체 저장해놓을 공간
    private float lastProjectileTime = 0f; // 마지막 투사체 발사 시간

    [SerializeField] public Queue<GameObject> projectileQueue = new Queue<GameObject>{};
    private Queue<Tuple<float, int>> prepareQueue = new Queue<Tuple<float, int>>(); // (arriveTime, type) 저장


    public GameObject hpBarPrefab;
    private GameObject hpBar;
    private Transform hpControl;

    private void Update() // 현재 striker 자체에서 투사체 일정 간격으로 발사
    {
        // 투사체 발사 타이밍 계산
        if (currentNoteIndex >= chartData.notes.Count) return;

        // 현재 시간 가져오기
        float currentTime = StageManager.Instance.currentTime;
        // 1️⃣ `prepareTime` 확인 → 준비 상태 활성화 & `arriveTime`과 `type` 저장
        if (currentNoteIndex < chartData.notes.Count && currentTime >= chartData.notes[currentNoteIndex].time * (60f / bpm) + playerManager.musicOffset)
        {
            PrepareForFire();
        }

        // 채보 시간에 맞춰 발사
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset - 0.5f)
        {
            FireProjectile(prepareQueue.Peek().Item1, prepareQueue.Peek().Item2);
            prepareQueue.Dequeue(); // 발사된 노트 제거
            lastProjectileTime = currentTime;
        }
        // 마지막 투사체 발사 이후 2초가 지나면 공격 상태 해제
        if (currentTime - lastProjectileTime > 2.0f)
        {
            animator.SetBool("isAttacking", false);
        }
    }
    private void PrepareForFire()
    {
        float arriveTime = chartData.notes[currentNoteIndex].arriveTime;
        int noteType = chartData.notes[currentNoteIndex].type; // 노트 타입 저장

        prepareQueue.Enqueue(new Tuple<float, int>(arriveTime, noteType)); // 도착 시간과 타입 저장
        Debug.Log("prepare!");

        // 애니메이션 실행 (느낌표 표시)

        currentNoteIndex++; // 다음 노트로 이동
    }

    // 투사체 발사
    private void FireProjectile(float time, int index)
    {
        if (index < 0 || index >= projectilePrefabs.Count) return;
        GameObject selectedProjectile = projectilePrefabs[index];
        if (!animator.GetBool("isAttacking"))
        {
            animator.SetBool("isAttacking", true);
        }

        // 투사체 생성
        GameObject projectile = Instantiate(selectedProjectile, transform.position, Quaternion.identity);

        // 투사체 저장
        projectileQueue.Enqueue(projectile);

        // 투사체에 타겟 설정
        projectile projScript = projectile.GetComponent<projectile>();
        if (projScript != null)
        {
            projScript.target = playerManager.transform; // 플레이어를 타겟으로 설정
            projScript.owner = this;   // 소유자로 현재 스트라이커 설정
            projScript.arriveTime = time;
            projScript.type = index;
        }
    }

    public void Initialize(int _initialHp, int initialBpm, PlayerManager targetPlayer, Direction direction, ChartData chart) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
    {
        hp = _initialHp;
        initialHp = _initialHp;
        bpm = initialBpm;
        playerManager = targetPlayer;
        Debug.Log($"{gameObject.name} spawned with HP: {hp}, BPM: {bpm}");
        location = direction;
        chartData = chart; // 채보 데이터 설정

        hpBar = Instantiate(hpBarPrefab, transform);
        hpBar.transform.localPosition = Vector3.down * 2f;
        hpControl = hpBar.transform.GetChild(0);
        hpControl.transform.localScale = new Vector3(0, 1, 1);
    }

    public void ClearProjectiles()
    {
        while (projectileQueue.Count > 0)
        {
            GameObject projectile = projectileQueue.Dequeue();
            if (projectile != null)
            {
                Destroy(projectile); // Projectile 삭제
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (hp >= 0)
        {
            hpControl.transform.localScale = new Vector3(1 - ((float)hp / initialHp), 1, 1);

            hp -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage! Current HP: {hp}");
            StartCoroutine(PlayDamageAnimation());
            if (hp == 0)
            {
                animator.SetBool("isClear", true);
                playerManager.hp =+ 1;
            }
        }
    }

    private IEnumerator PlayDamageAnimation()
    {
        animator.SetBool("isDamaged", true);
        yield return new WaitForSeconds(0.3f); // 피해 애니메이션 시간
        animator.SetBool("isDamaged", false);
    }
}
