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

public enum StrikerType
{
    DRUMMON = 1,
    GUITARMON = 2
}

public class DirTool
{
    static public Vector3 TranstoVec(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            
            case Direction.Down:
                return Vector3.down;
            
            case Direction.Left:
                return Vector3.left;
            
            case Direction.Right:
                return Vector3.right;

            default:
                return Vector3.zero;
        }
    }

    static public Direction ReverseDir(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            
            case Direction.Down:
                return Direction.Up;
            
            case Direction.Left:
                return Direction.Right;
            
            case Direction.Right:
                return Direction.Left;

            default:
                return Direction.None;
        }
    }
}