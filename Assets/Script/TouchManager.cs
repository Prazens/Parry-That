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
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private bool isTouchAvailable = false;

    private Vector3 initialPos;
    private Vector3 lastPos;
    private bool isSwiping = false;
    private double sumLength = 0;
    private double judgeTime;

    private bool isTapAndSwipe = false;  // 같은 방향 연속 스와이프 입력 방지
    private Direction previousDirection;  // 같은 방향 연속 스와이프 입력 방지

    private Touch tempTouchs;

    void Update()
    {
        if (StageManager.isActive)
        {
            if (isTouchAvailable)
            {
                TouchChecker();
            }
            else
            {
                //KeyChecker();  // Legacy
                MouseChecker();  // 터치에 중복
            }
        }
    }

    // 임시 조작 확인기
    private void KeyChecker()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendJudge(Direction.Left, StageManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SendJudge(Direction.Right, StageManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            SendJudge(Direction.Up, StageManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SendJudge(Direction.Down, StageManager.Instance.currentTime, AttackType.Strong);
        }
        else if (Input.GetKeyDown(KeyCode.Space))  // 방향 스와이프 없이 탭만 할때의 움직임
        {
            SendJudge(Direction.None, StageManager.Instance.currentTime, AttackType.Normal);
        }
    }

    // 임시 조작 확인기
    private void MouseChecker()
    {

        // 최초 입력시 : 스와이프 판별 시작
        if (Input.GetMouseButtonDown(0) && !isSwiping)
        {
            initialPos = Input.mousePosition;
            lastPos = Input.mousePosition;
            sumLength = 0;
            isSwiping = true;
            isTapAndSwipe = true;
            judgeTime = StageManager.Instance.currentTime;

            // // Debug.Log("input: 마우스 입력 시작");
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

            judgeTime = StageManager.Instance.currentTime;
        }
        
        // 설정된 조작 길이를 넘었을 경우 : 스와이프한 것으로 취급, 판정 실시
        else if (sumLength > 40f && isSwiping)
        {
            lastPos -= initialPos;
            double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

            isSwiping = false;
            Direction tempDirection;

            if (angle > 150 || angle <= -150)
            {
                tempDirection = Direction.Left;
            }
            else if (angle > 30)
            {
                tempDirection = Direction.Up;
            }
            else if (angle > -30)
            {
                tempDirection = Direction.Right;
            }
            else
            {
                tempDirection = Direction.Down;
            }

            if (isTapAndSwipe || previousDirection != tempDirection)
            {
                SendJudge(tempDirection, judgeTime, AttackType.Strong);
                previousDirection = tempDirection;
            }
        }

        // 조작 한계 시간을 초과한 경우 : 무방향 조작으로 취급, 판정 실시
        else if (StageManager.Instance.currentTime - judgeTime > 0.1f && isSwiping)
        {
            isSwiping = false;
        }

        // 조작이 계속 진행 중인 경우 : 조작 길이를 매 프레임 마다 더하기
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            sumLength += Vector3.Distance(lastPos, Input.mousePosition);
            lastPos = Input.mousePosition;
        }

        // (조작 길이를 채우지 못하고) 조작을 종료했을 경우 : 무방향 조작으로 취급, 판정 실시
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }

        // 그냥 뗐을 때
        else if (scoreManager.isHolding && !Input.GetMouseButtonUp(0))
        {
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }
    }

    // Touch Checker
    private void TouchChecker()
    {
        // 터치 입력이 있을 경우
        if (Input.touchCount > 0)
        {
            // // Debug.Log($"{Input.touchCount} / {isSwiping} / {sumLength}");

            tempTouchs = Input.GetTouch(0);

            // 최초 입력시 : 스와이프 판별 시작
            if (tempTouchs.phase == TouchPhase.Began && !isSwiping)
            {
                // // Debug.Log("Touch on");

                // 스와이프 시작점 기록, 스와이프 길이 초기화
                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;

                // 스와이프하는지, 터치 후 첫 스와이프 체크인지 초기화
                isSwiping = true;
                isTapAndSwipe = true;
                
                // 판별 시작 시간 기록
                judgeTime = StageManager.Instance.currentTime;

                // 무방향 입력 판정
                SendJudge(Direction.None, judgeTime, AttackType.Normal);
                previousDirection = Direction.None;
            }

            // 스와이프가 판별되었거나 시간 초과 후에도 입력 홀드시 : 스와이프 판별 재시작
            else if (!isSwiping)
            {
                // 스와이프 시작점 기록, 스와이프 길이 초기화
                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;

                // 판별 시작 시간 기록
                judgeTime = StageManager.Instance.currentTime;

                // 스와이프하는지, 터치 후 첫 스와이프 체크인지 초기화
                isSwiping = true;
                isTapAndSwipe = false;
            }


            // 설정된 터치 길이를 넘었을 경우 : 스와이프한 것으로 취급, 판정 실시
            else if (sumLength > 0.25f && isSwiping)
            {
                // 시작점으로부터 끝점까지의 각도 측정
                lastPos -= initialPos;
                double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

                // 스와이프 판별 끝
                isSwiping = false;
                Direction tempDirection;

                // 각도에 따라 방향 산출
                if (angle > 150 || angle <= -150)
                {
                    tempDirection = Direction.Left;
                }
                else if (angle > 30)
                {
                    tempDirection = Direction.Up;
                }
                else if (angle > -30)
                {
                    tempDirection = Direction.Right;
                }
                else
                {
                    tempDirection = Direction.Down;
                }

                // 홀드 중에는 같은 방향이라도 홀드 종료를 위해 판정을 보냄
                if (isTapAndSwipe || previousDirection != tempDirection || scoreManager.isHolding)
                {
                    // 판정을 보냄
                    SendJudge(tempDirection, judgeTime, AttackType.Strong);
                    // 이전 판정과 같은 스와이프로 판정이 연사되지 않게 기록
                    previousDirection = tempDirection;
                }
            }

            // 터치 한계 시간을 초과한 경우 : 스와이프 판별 종료, 롱노트 판별 계속
            else if (StageManager.Instance.currentTime - judgeTime > 0.1f && isSwiping)
            {
                // 스와이프 판별 끝
                isSwiping = false;
            }

            // 터치가 계속 진행 중인 경우 : 터치 길이를 매 프레임 마다 더하기
            else if (isSwiping)
            {
                sumLength += Vector3.Distance(lastPos, Camera.main.ScreenToWorldPoint(tempTouchs.position));
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            }
        }

        // 터치 길이를 채우지 못하고 터치를 종료했을 경우 : 스와이프 판별 종료, 롱노트 떼는 판정
        if (tempTouchs.phase == TouchPhase.Ended && isSwiping)
        {
            // 스와이프 판별 끝
            isSwiping = false;
            
            // 롱노트 끝판정 전송
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }

        // 그냥 뗐을 때
        else if (scoreManager.isHolding && Input.touchCount == 0)
        {
            SendJudge(Direction.None, judgeTime, AttackType.HoldStop);
        }
    }

    private void SendJudge(Direction? _judgeDirection, double _judgeTime, AttackType _type)
    {
        if (_judgeDirection.HasValue)
        {
            // Debug.Log($"판정 전송");

            scoreManager.judgeQueue.Enqueue(new JudgeFormat((Direction)_judgeDirection, _judgeTime, _type));
        }
    }
}
