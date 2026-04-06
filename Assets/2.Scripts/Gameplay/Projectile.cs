using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 목표 위치

    private bool hasReachedTarget = false; // 목표 위치 도달 여부
    private Vector3 finalVelocity; // 도착 시의 마지막 속도 저장

    private float arriveSec; // 도착 시각
    private float duration;

    public void Setup(Direction location, Vector3 startPos, Vector3 targetPos, float arriveSec)
    {
        switch (location)
        {
            case Direction.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Right:
                transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            default:
                break;
        }
        transform.position = startPos;

        startPosition = startPos;
        targetPosition = targetPos;
        this.arriveSec = arriveSec;
        duration = arriveSec - StageFlowManager.Instance.currentTime;
    }

    void Update()
    {
        float currentSec = StageFlowManager.Instance.currentTime;

        if (!hasReachedTarget)
        {
            float fractionOfJourney = (arriveSec - currentSec) / duration;
            transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);

            if (fractionOfJourney < 0f)
            {
                hasReachedTarget = true;
                finalVelocity = (targetPosition - startPosition) / duration;
            }
        }
        else
        {
            transform.position = targetPosition + finalVelocity * (arriveSec - currentSec);
        }
    }
}
