using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

// 너무 헷갈려서 만듬
public enum AttackType
{
    Normal = 0,
    Strong = 1,
    HoldStart = 2,
    HoldFinishStrong = 3,
    HoldStop = 4  // 홀드 끝에서 스와이프하지 않고 그냥 손을 뗐을 경우
}