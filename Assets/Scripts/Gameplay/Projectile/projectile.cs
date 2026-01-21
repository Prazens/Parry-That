using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker

    // ScoreManager 삭제 -> StageAudioManager로 대체(오프셋만 필요)
    [SerializeField] private StageAudioManager stageAudioManager;

    public float bpm;

    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 0.6f 거리 목표 위치
    private float journeyLength; // 이동 거리

    private bool hasReachedTarget = false; // 목표 위치 도달 여부
    private Vector3 finalVelocity; // 0.6f 도착 시의 마지막 속도 저장

    public float arriveTime;
    public int type;

    void Start()
    {
        bpm = owner.bpm;

        // stageAudioManager 미할당이면 씬에서 찾아서 연결(최소 수정 + 안전장치)
        if (stageAudioManager == null)
        {
            stageAudioManager = FindObjectOfType<StageAudioManager>();
        }

        startPosition = transform.position;

        // 목표 위치 설정 (플레이어에서 0.6f 거리)
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        targetPosition = target.position - directionToPlayer * 0.6f;

        journeyLength = Vector3.Distance(startPosition, targetPosition);
    }

    void Update()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        float musicOffset = 0f;
        if (stageAudioManager != null)
        {
            musicOffset = stageAudioManager.musicOffset;
        }

        if (!hasReachedTarget)
        {
            float fractionOfJourney = (arriveTime * (60f / bpm) + musicOffset - currentTime) / 0.5f;
            transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);

            if (fractionOfJourney < 0f)
            {
                hasReachedTarget = true;
                finalVelocity = (targetPosition - startPosition) / 0.5f;
            }
        }
        else
        {
            transform.position += finalVelocity * Time.deltaTime;
        }
    }
}
