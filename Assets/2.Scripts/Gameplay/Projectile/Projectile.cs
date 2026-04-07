using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSetupContext
{
    public Direction location;
    public Vector3 startPos;
    public Vector3 targetPos;
    public float arriveSec;
    public AttackType attackType;

    public ProjectileSetupContext(Direction location, Vector3 startPos, Vector3 targetPos, float arriveSec, AttackType attackType)
    {
        this.location = location;
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.arriveSec = arriveSec;
        this.attackType = attackType;
    }
}

public abstract class Projectile : MonoBehaviour
{
    public abstract void Setup(ProjectileSetupContext context);
    public abstract void OnJudge(JudgeContext context);
}
