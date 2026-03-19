using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public struct JudgeFormat
{
    public Direction direction;
    public double timing;
    public AttackType type;

    public JudgeFormat(Direction _direction, double _timing, AttackType _type)
    {
        direction = _direction;
        timing = _timing;
        type = _type;
    }
}

public class TouchManager : MonoBehaviour
{
    [SerializeField] private JudgeSystem judgeSystem;   // ScoreManager -> JudgeSystem
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private bool isTouchAvailable = false;

    private Vector3 initialPos;
    private Vector3 lastPos;
    private bool isSwiping = false;
    private double sumLength = 0;
    private double judgeTime;

    private bool isTapAndSwipe = false;
    private Direction previousDirection;

    private Touch tempTouchs;

    void Update()
    {
        if (StageFlowManager.isActive)
        {
            MouseChecker();
            //if (isTouchAvailable)
            //{
            //    TouchChecker();
            //}
            //else
            //{
            //    //KeyChecker();  // Legacy
            //    MouseChecker();
            //}
        }
    }

    private void KeyChecker()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendJudge(Direction.Left, StageFlowManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SendJudge(Direction.Right, StageFlowManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            SendJudge(Direction.Up, StageFlowManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SendJudge(Direction.Down, StageFlowManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SendJudge(Direction.None, StageFlowManager.Instance.currentTime, AttackType.Normal);
        }
    }

    private void MouseChecker()
    {
        if (Input.GetMouseButtonDown(0) && !isSwiping)
        {
            initialPos = Input.mousePosition;
            lastPos = Input.mousePosition;
            sumLength = 0;
            isSwiping = true;
            isTapAndSwipe = true;
            judgeTime = StageFlowManager.Instance.currentTime;

            SendJudge(Direction.None, judgeTime, AttackType.Normal);
            previousDirection = Direction.None;
        }
        else if (Input.GetMouseButton(0) && !isSwiping)
        {
            initialPos = Input.mousePosition;
            lastPos = Input.mousePosition;
            sumLength = 0;
            isSwiping = true;
            isTapAndSwipe = false;

            judgeTime = StageFlowManager.Instance.currentTime;
        }
        else if (sumLength > 40f && isSwiping)
        {
            lastPos -= initialPos;
            double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

            isSwiping = false;
            Direction tempDirection;

            if (angle > 150 || angle <= -150) tempDirection = Direction.Left;
            else if (angle > 30) tempDirection = Direction.Up;
            else if (angle > -30) tempDirection = Direction.Right;
            else tempDirection = Direction.Down;

            if (isTapAndSwipe || previousDirection != tempDirection)
            {
                SendJudge(tempDirection, judgeTime, AttackType.Strong);
                previousDirection = tempDirection;
            }
        }
        else if (StageFlowManager.Instance.currentTime - judgeTime > 0.1f && isSwiping)
        {
            isSwiping = false;
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            sumLength += Vector3.Distance(lastPos, Input.mousePosition);
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);

            if (judgeSystem != null && judgeSystem.isHolding)
            {
                print("HoldStop");
                judgeSystem.isHolding = false;
            }
        }
        else if (judgeSystem != null && judgeSystem.isHolding && !Input.GetMouseButtonUp(0))
        {
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
            print("HoldStop");
            judgeSystem.isHolding = false;
        }
    }

    private void TouchChecker()
    {
        if (Input.touchCount > 0)
        {
            tempTouchs = Input.GetTouch(0);

            if (tempTouchs.phase == TouchPhase.Began && !isSwiping)
            {
                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;

                isSwiping = true;
                isTapAndSwipe = true;

                judgeTime = StageFlowManager.Instance.currentTime;

                SendJudge(Direction.None, judgeTime, AttackType.Normal);
                previousDirection = Direction.None;
            }
            else if (!isSwiping)
            {
                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;

                judgeTime = StageFlowManager.Instance.currentTime;

                isSwiping = true;
                isTapAndSwipe = false;
            }
            else if (sumLength > 0.25f && isSwiping)
            {
                lastPos -= initialPos;
                double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

                isSwiping = false;
                Direction tempDirection;

                if (angle > 150 || angle <= -150) tempDirection = Direction.Left;
                else if (angle > 30) tempDirection = Direction.Up;
                else if (angle > -30) tempDirection = Direction.Right;
                else tempDirection = Direction.Down;

                if (isTapAndSwipe || previousDirection != tempDirection || (judgeSystem != null && judgeSystem.isHolding))
                {
                    SendJudge(tempDirection, judgeTime, AttackType.Strong);
                    previousDirection = tempDirection;
                }
            }
            else if (StageFlowManager.Instance.currentTime - judgeTime > 0.1f && isSwiping)
            {
                isSwiping = false;
            }
            else if (isSwiping)
            {
                sumLength += Vector3.Distance(lastPos, Camera.main.ScreenToWorldPoint(tempTouchs.position));
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            }
        }

        if (tempTouchs.phase == TouchPhase.Ended && isSwiping)
        {
            isSwiping = false;
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }
        else if (judgeSystem != null && judgeSystem.isHolding && Input.touchCount == 0)
        {
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }
    }

    private void SendJudge(Direction? _judgeDirection, double _judgeTime, AttackType _type)
    {
        if (_judgeDirection.HasValue)
        {
            if (judgeSystem == null) return;
            judgeSystem.judgeQueue.Enqueue(new JudgeFormat((Direction)_judgeDirection, _judgeTime, _type));
        }
    }
}
