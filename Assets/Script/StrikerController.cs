using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerController : MonoBehaviour
{
    // striker 자체에 들어가는 script
    [SerializeField] private GameObject projectilePrefab; // 투사체 프리팹
    [SerializeField] private Transform player; // 플레이어 위치
    [SerializeField] private int hp; // 스트라이커 HP
    [SerializeField] private int bpm; // BPM
    private double currentTime = 0d; // 시간 계산용 변수
    public Direction location; // 위치 방향

    // 임시로 발사체 저장해놓을 공간
    public Queue<GameObject> projectileQueue = new Queue<GameObject>{};

    private void Update() // 현재 striker 자체에서 투사체 일정 간격으로 발사사
    {
        // 투사체 발사 타이밍 계산
        currentTime += Time.deltaTime;
        if (currentTime >= 60d / bpm)
        {
            FireProjectile();
            currentTime -= 60d / bpm;
        }
    }

    // 투사체 발사
    private void FireProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // 투사체 생성
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 투사체 저장
        projectileQueue.Enqueue(projectile);

        // 투사체에 타겟 설정
        projectile projScript = projectile.GetComponent<projectile>();
        if (projScript != null)
        {
            projScript.target = player; // 플레이어를 타겟으로 설정
            projScript.owner = this;   // 소유자로 현재 스트라이커 설정
        }
    }
    public void Initialize(int initialHp, int initialBpm, Transform targetPlayer, Direction direction) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
    {
        hp = initialHp;
        bpm = initialBpm;
        player = targetPlayer;
        Debug.Log($"{gameObject.name} spawned with HP: {hp}, BPM: {bpm}");
        location = direction;
    }
    
}
