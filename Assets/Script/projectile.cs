using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    // 투사체에 들어가는 script
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker
    public ScoreManager scoreManager;
    public float bpm;
    private float moveLength;
    private float calcSpeed;
    private Vector3 directionVector;
    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 0.6f 거리 목표 위치
    private float startTime; // 이동 시작 시간
    public float moveTime = 0.5f;
    private float journeyLength; // 이동 거리

    private bool hasReachedTarget = false; // 목표 위치 도달 여부
    private Vector3 finalVelocity; // 0.6f 도착 시의 마지막 속도 저장
    public float arriveTime; 
    public int type;
    

    void Start()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            scoreManager = gameController.GetComponent<ScoreManager>();
            bpm = owner.bpm;
            if (scoreManager != null)
            {
                Debug.Log("projectile successfully linked with ScoreManager.");
            }
            else
            {
                Debug.LogError("ScoreManager script is not attached to GameController!");
            }
        }
        else
        {
            Debug.LogError("GameController not found!");
        }
        startPosition = transform.position;
        // 목표 위치 설정 (플레이어에서 0.6f 거리)
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        targetPosition = target.position - directionToPlayer * 0.6f;
        
        // 이동 거리 계산
        journeyLength = Vector3.Distance(startPosition, targetPosition);
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        // Lerp를 이용해 투사체 이동 (0.6f 지점까지)
        if (!hasReachedTarget)
        {
            float fractionOfJourney = (arriveTime * (60f / bpm) + 2f - currentTime) / 0.5f;
            transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);

            if (fractionOfJourney < 0f)
            {
                hasReachedTarget = true;
                finalVelocity = (targetPosition - startPosition) / 0.5f; // Lerp의 속도를 저장
            }
        }
        else
        {
            // 저장된 속도로 계속 이동
            transform.position += finalVelocity * Time.deltaTime;
        }

        // if (currentTime > (arriveTime * 60d / bpm) + scoreManager.musicOffset + 0.12d)
        // {
        //     // Debug.Log($"{StageManager.Instance.currentTime} - {noteData.arriveTime} = {StageManager.Instance.currentTime - noteData.arriveTime}");
            
        //     scoreManager.JudgeManage(owner.location, 0, (AttackType)type, owner, true);
        // }

        // 일정거리 가까워지면 destroy로 임시 구현
        // if (Vector3.Distance(transform.position, target.position) <= 0.6f)
        // {

        //     // 저장 해제
        //     Destroy(owner.projectileQueue.Dequeue());
        // }
    }
}
