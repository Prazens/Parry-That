using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostProjectile : Projectile
{
    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 목표 위치

    private Vector3 reverseStartPosition; // 가짜 시작 위치
    private Vector3 reverseTargetPosition; // 가짜 목표 위치

    private bool hasReachedTarget = false; // 목표 위치 도달 여부
    private Vector3 finalVelocity; // 도착 시의 마지막 속도 저장

    private Direction location;
    private float arriveSec; // 도착 시각
    private float duration;

    private bool hasStoppedFake = false;
    private float stopFakeRatio => 0.5f; // 진짜 방향으로 순간이동할 타이밍

    public override void Setup(ProjectileSetupContext context)
    {
        arriveSec = context.arriveSec;
        location = context.location;
        startPosition = context.startPos;
        targetPosition = context.targetPos;
        reverseStartPosition = context.reverseStartPos;
        reverseTargetPosition = context.reverseTargetPos;

        Rotate(DirTool.ReverseDir(location));
        transform.position = reverseStartPosition;

        duration = arriveSec - StageFlowManager.Instance.currentTime;
    }

    public override void OnJudge(JudgeContext context)
    {
        if (context.isMiss)
            return;

        Destroy(gameObject);
        if (!context.isParried)
            return;

        // 위로 반격
        ParriedProjectileManager parriedProjectileManager = FindAnyObjectByType<ParriedProjectileManager>();
        if (parriedProjectileManager == null)
            return;

        Judgeable judgeable = context.judgeable;
        int fixRandom = 0;
        if (judgeable.noteDirection == Direction.Left)
            fixRandom = 1;
        else if (judgeable.noteDirection == Direction.Right)
            fixRandom = 2;
        parriedProjectileManager.ParryProjectile(Direction.Up, judgeable.attackType, fixRandom);
    }

    void Update()
    {
        LerpPosition(StageFlowManager.Instance.currentTime);
    }

    private void LerpPosition(float currentSec)
    {
        if (!hasReachedTarget)
        {
            float fractionOfJourney = (arriveSec - currentSec) / duration;
            if (fractionOfJourney > 1 - stopFakeRatio)
            {
                transform.position = Vector3.Lerp(reverseTargetPosition, reverseStartPosition, fractionOfJourney);
            }
            else
            {
                if (!hasStoppedFake)
                {
                    Rotate(location);
                    hasStoppedFake = true;
                }
                transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);
            }

            if (fractionOfJourney < 0f)
            {
                hasReachedTarget = true;
                finalVelocity = (targetPosition - startPosition) / duration;
            }
        }
        else
        {
            transform.position = targetPosition + finalVelocity * (currentSec - arriveSec);
            if (currentSec - arriveSec > 5f)
                Destroy(gameObject);
        }
    }

    private void Rotate(Direction location)
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
    }
}
