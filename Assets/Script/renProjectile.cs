using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renProjectile : MonoBehaviour
{
    // 연타 투사체에 들어가는 script
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker
    public ScoreManager scoreManager;
    public float bpm;
    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 0.6f 거리 목표 위치
    public float moveTime = 0.5f;

    public float arriveTime; 
    public int type;
    

    void Start()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            scoreManager = gameController.GetComponent<ScoreManager>();
            bpm = owner.bpm;
        }

        startPosition = transform.position;
        // 목표 위치 설정 (플레이어에서 0.6f 거리)
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        targetPosition = target.position - directionToPlayer * 0.6f;
    }

    void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        // Lerp를 이용해 투사체 이동 (0.6f 지점까지)
        float fractionOfJourney = (arriveTime * (60f / bpm) + scoreManager.musicOffset - currentTime) / 0.5f;
        transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);

        if (fractionOfJourney < 0f)
        {
            Destroy(this.gameObject);
        }
    }
}
