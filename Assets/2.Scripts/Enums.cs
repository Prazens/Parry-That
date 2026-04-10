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

public enum AttackType
{
    Normal = 0,
    Strong = 1,
    HoldStart = 2,
    HoldStop = 3,
    Ghost = 4,
}

public enum JudgeType
{
    None = 0,
    LateMiss = 1,
    LateBlocked = 2,
    LateParried = 3,
    Perfect = 4,
    EarlyParried = 5,
    EarlyBlocked = 6,
    EarlyMiss = 7
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