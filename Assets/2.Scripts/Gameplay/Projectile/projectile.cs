using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker

    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 0.6f 거리 목표 위치
    private float journeyLength; // 이동 거리

    private bool hasReachedTarget = false; // 목표 위치 도달 여부
    private Vector3 finalVelocity; // 0.6f 도착 시의 마지막 속도 저장

    public float arriveTime;
    public int type;

    void Start()
    {
        startPosition = transform.position;

        // 목표 위치 설정 (플레이어에서 0.6f 거리)
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        targetPosition = target.position - directionToPlayer * 0.6f;

        journeyLength = Vector3.Distance(startPosition, targetPosition);
    }

    void Update()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        if (!hasReachedTarget)
        {
            float fractionOfJourney = (arriveTime - currentTime) / 0.5f;
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
