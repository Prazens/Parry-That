using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerController : MonoBehaviour
{
    // striker 자체에 들어가는 script
    [SerializeField] private List<GameObject> projectilePrefabs; // 투사체 프리팹
    public PlayerManager playerManager; // Player 정보 저장
    [SerializeField] private ChartData chartData; // 채보 데이터
    [SerializeField] public int hp; // 스트라이커 HP
    [SerializeField] public int bpm; // BPM
    public Direction location; // 위치 방향
    private int currentNoteIndex = 0; // 현재 채보 인덱스
    [SerializeField] private Animator animator;
    // 임시로 발사체 저장해놓을 공간
    private float lastProjectileTime = 0f; // 마지막 투사체 발사 시간

    [SerializeField] public Queue<GameObject> projectileQueue = new Queue<GameObject>{};

    private void Update() // 현재 striker 자체에서 투사체 일정 간격으로 발사
    {
        // 투사체 발사 타이밍 계산
        if (currentNoteIndex >= chartData.notes.Count) return;

        // 현재 시간 가져오기
        float currentTime = StageManager.Instance.currentTime;

        // 채보 시간에 맞춰 발사
        if (currentTime >= (chartData.notes[currentNoteIndex].time * (60d / bpm)) + playerManager.musicOffset)
        {
            FireProjectile(chartData.notes[currentNoteIndex].type);
            currentNoteIndex++;
            lastProjectileTime = currentTime;
        }
        // 마지막 투사체 발사 이후 2초가 지나면 공격 상태 해제
        if (currentTime - lastProjectileTime > 2.0f)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    // 투사체 발사
    private void FireProjectile(int index)
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
        }

        // 투사체에 노트 정보 저장
        projScript.noteData = chartData.notes[currentNoteIndex];
    }
    public void Initialize(int initialHp, int initialBpm, PlayerManager targetPlayer, Direction direction, ChartData chart) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
    {
        hp = initialHp;
        bpm = initialBpm;
        playerManager = targetPlayer;
        Debug.Log($"{gameObject.name} spawned with HP: {hp}, BPM: {bpm}");
        location = direction;
        chartData = chart; // 채보 데이터 설정
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
            hp -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage! Current HP: {hp}");
            StartCoroutine(PlayDamageAnimation());
            if(hp == 0)
            {
                animator.SetBool("isClear", true);
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
