using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSetupContext
{
    public AttackType attackType;
    public float arriveSec;
    public Direction location;
    public Vector3 startPos;
    public Vector3 targetPos;
    public Vector3 reverseStartPos;
    public Vector3 reverseTargetPos;

    public ProjectileSetupContext(AttackType attackType, float arriveSec, Direction location,
        Vector3 startPos, Vector3 targetPos, Vector3 reverseStartPos, Vector3 reverseTargetPos)
    {
        this.attackType = attackType;
        this.arriveSec = arriveSec;
        this.location = location;
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.reverseStartPos = reverseStartPos;
        this.reverseTargetPos = reverseTargetPos;
    }
}

public abstract class Projectile : MonoBehaviour
{
    public abstract void Setup(ProjectileSetupContext context);
    public abstract void OnJudge(JudgeContext context);
}
