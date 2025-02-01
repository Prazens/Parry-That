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
    public int type;

    public JudgeFormat(Direction _direction, double _timing, int _type)
    {
        this.direction = _direction;
        this.timing = _timing;
        this.type = _type;
    }
}


public class TouchManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private bool isTouchAvailable = false;

    int type;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
                // KeyChecker();  // Legacy
                MouseChecker();  // 터치에 중복
            }

            // judgeDirection에 값이 null이 아니라 존재할 경우 : 판정 실시, null로 값 삭제
            // 수정 예정: Update에서는 연산하지 않고 큐에 넣는것만,
            //          Direction.None을 해석하는 것은 ScoreManager에,
            //          JudgeFormat을 만드는 것은 ~~Checker에 맡길 계획,
            //          애니메이션 재생은 ScoreManager의 Judge 맨 끝으로?
            if (judgeDirection.HasValue)
            {
                Debug.Log($"판정 전송 : {judgeDirection}");
                if (judgeDirection == Direction.None)
                {
                    type = 0;
                    judgeDirection = playerManager.currentDirection;
                }
                else
                {
                    type = 1;
                }

                playerManager.Operate((Direction)judgeDirection, type);
                // playerManager.ShieldMove((Direction)judgeDirection);  // Legacy

                scoreManager.judgeQueue.Enqueue(new JudgeFormat((Direction)judgeDirection, judgeTime, type));
                // scoreManager.Judge((Direction)judgeDirection, judgeTime, type);  // Legacy

                // Debug.Log("판정 전송 : 널됨");
                judgeDirection = null;
            }

            // if (judgeFormat.HasValue)
            // {
            //     Debug.Log($"판정 전송");


            //     scoreManager.judgeQueue.Enqueue((JudgeFormat)judgeFormat);
            //     judgeFormat = null;
            // }
        }
    }

    // 임시 조작 확인기
    // Legacy
    private void KeyChecker()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // scoreManager.Judge(Direction.Left, StageManager.Instance.currentTime);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // scoreManager.Judge(Direction.Right, StageManager.Instance.currentTime);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playerManager.Operate(Direction.Up, 1);
            scoreManager.Judge(Direction.Up, StageManager.Instance.currentTime, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playerManager.Operate(Direction.Down, 1);
            scoreManager.Judge(Direction.Down, StageManager.Instance.currentTime, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Space))  // 방향 스와이프 없이 탭만 할때의 움직임
        {
            playerManager.Operate(playerManager.currentDirection, 0);
            scoreManager.Judge(playerManager.currentDirection, StageManager.Instance.currentTime, 0);
        }
    }


    private Vector3 initialPos;
    private Vector3 lastPos;
    private bool isSwiping = false;
    private double sumLength = 0;
    private double judgeTime;

    private Direction? judgeDirection = null; // null이면 판정하지 않고, 실제 값을 가진 경우 판정
    // private JudgeFormat? judgeFormat = null; // null이면 판정하지 않고, 실제 값을 가진 경우 판정

    private bool isTapAndSwipe = false;  // 같은 방향 연속 스와이프 입력 방지
    private Direction previousDirection;  // 같은 방향 연속 스와이프 입력 방지

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

            // Debug.Log("input: 마우스 입력 시작");
            judgeDirection = Direction.None;
            previousDirection = Direction.None;

            judgeTime = StageManager.Instance.currentTime;
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
        else if (sumLength > 20f && isSwiping)
        {
            lastPos -= initialPos;
            double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

            isSwiping = false;

            if (angle > 135 || angle <= -135)
            {
                if (isTapAndSwipe || previousDirection != Direction.Left)
                {
                    judgeDirection = Direction.Left;
                    previousDirection = Direction.Left;
                }
            }
            else if (angle > 45)
            {
                if (isTapAndSwipe || previousDirection != Direction.Up)
                {
                    judgeDirection = Direction.Up;
                    previousDirection = Direction.Up;
                }
            }
            else if (angle > -45)
            {
                if (isTapAndSwipe || previousDirection != Direction.Right)
                {
                    judgeDirection = Direction.Right;
                    previousDirection = Direction.Right;
                }
            }
            else
            {
                if (isTapAndSwipe || previousDirection != Direction.Down)
                {
                    judgeDirection = Direction.Down;
                    previousDirection = Direction.Down;
                }
            }
        }

        // 조작 한계 시간을 초과한 경우 : 무방향 조작으로 취급, 판정 실시
        else if (StageManager.Instance.currentTime - judgeTime > 0.0625f && isSwiping)
        {
            Debug.Log("Maximum swipe time exceeded.");

            // if (isTapAndSwipe)  // Legacy
            // {
            //     judgeDirection = Direction.None;
            // }
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
            // if (isTapAndSwipe)  // Legacy
            // {
            //     judgeDirection = Direction.None;
            // }
            isSwiping = false;
        }
    }

    private Touch tempTouchs;
    // Touch Checker
    private void TouchChecker()
    {
        // 터치 입력이 있을 경우
        if (Input.touchCount > 0)
        {
            // Debug.Log($"{Input.touchCount} / {isSwiping} / {sumLength}");

            tempTouchs = Input.GetTouch(0);

            // 최초 입력시 : 스와이프 판별 시작
            if (tempTouchs.phase == TouchPhase.Began && !isSwiping)
            {
                // Debug.Log("Touch on");

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
                judgeDirection = Direction.None;
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
            else if (sumLength > 0.1f && isSwiping)
            {
                // 시작점으로부터 끝점까지의 각도 측정
                lastPos -= initialPos;
                double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
                
                // Debug.Log(sumLength);
                // Debug.Log(angle);

                // 스와이프 판별 끝
                isSwiping = false;

                // 각도에 따라 방향 산출
                if (angle > 135 || angle <= -135)
                {
                    if (isTapAndSwipe || previousDirection != Direction.Left)
                    {
                        judgeDirection = Direction.Left;
                        previousDirection = Direction.Left;
                    }
                }
                else if (angle > 45)
                {
                    if (isTapAndSwipe || previousDirection != Direction.Up)
                    {
                        judgeDirection = Direction.Up;
                        previousDirection = Direction.Up;
                    }
                }
                else if (angle > -45)
                {
                    if (isTapAndSwipe || previousDirection != Direction.Right)
                    {
                        judgeDirection = Direction.Right;
                        previousDirection = Direction.Right;
                    }
                }
                else
                {
                    if (isTapAndSwipe || previousDirection != Direction.Down)
                    {
                        judgeDirection = Direction.Down;
                        previousDirection = Direction.Down;
                    }
                }
            }

            // 터치 한계 시간을 초과한 경우 : 스와이프 판별 종료, 롱노트 판별 계속
            else if (StageManager.Instance.currentTime - judgeTime > 0.0625f && isSwiping)
            {
                // if (isTapAndSwipe)  // Legacy
                // {
                //     judgeDirection = Direction.None;
                // }

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
            // if (isTapAndSwipe)  // Legacy
            // {
            //     judgeDirection = Direction.None;
            // }
            
            // Debug.Log("Touch out");

            // 스와이프 판별 끝
            isSwiping = false;
            
            // 롱노트 미구현
        }
    }
}
