using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
        if (isTouchAvailable)
        {
            TouchChecker();
        }
        else
        {
            KeyChecker();
            MouseChecker();  // 터치에 중복될 가능성 높음
        }

        // judgeDirection에 값이 null이 아니라 존재할 경우 : 판정 실시, null로 값 삭제
        // 임시로 좌우 입력은 막아둠
        if (judgeDirection.HasValue && (judgeDirection == Direction.Up || judgeDirection == Direction.Down || judgeDirection == Direction.None))
        {
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
            // playerManager.ShieldMove((Direction)judgeDirection);

            scoreManager.Judge((Direction)judgeDirection, judgeTime, 0);

            judgeDirection = null;
        }
    }

    // 임시 조작 확인기
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
            scoreManager.Judge(Direction.Up, StageManager.Instance.currentTime, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            scoreManager.Judge(Direction.Down, StageManager.Instance.currentTime, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Space))  // 방향 스와이프 없이 탭만 할때의 움직임
        {
            scoreManager.Judge(playerManager.currentDirection, StageManager.Instance.currentTime, 0);
        }
    }


    private Vector3 initialPos;
    private Vector3 lastPos;
    private bool isSwiping = false;
    private double sumLength = 0;
    private float judgeTime;
    private Direction? judgeDirection = null; // null이면 판정하지 않고, 실제 값을 가진 경우 판정

    // 임시 조작 확인기
    private void MouseChecker()
    {
        // if ((Input.GetMouseButtonDown(0) && !isSwiping)
        //     || (Input.GetMouseButton(0) && !isSwiping))  // 옵션

        // 조작 시작 : 초기화, 시작점 기록
        if (Input.GetMouseButtonDown(0) && !isSwiping)
        {
            initialPos = Input.mousePosition;
            lastPos = Input.mousePosition;
            sumLength = 0;
            isSwiping = true;

            judgeTime = StageManager.Instance.currentTime;
        }

        // 설정된 조작 길이를 넘었을 경우 : 스와이프한 것으로 취급, 판정 실시
        else if (sumLength > 50f && isSwiping)
        {
            lastPos -= initialPos;
            double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

            isSwiping = false;

            if (angle > 135 || angle <= -135)
            {
                judgeDirection = Direction.Left;

                // playerManager.ShieldMove(Direction.Left);
                // scoreManager.Judge(Direction.Left);
            }
            else if (angle > 45)
            {
                judgeDirection = Direction.Up;

                // playerManager.ShieldMove(Direction.Up);
                // scoreManager.Judge(Direction.Up);
            }
            else if (angle > -45)
            {
                judgeDirection = Direction.Right;

                // playerManager.ShieldMove(Direction.Right);
                // scoreManager.Judge(Direction.Right);
            }
            else
            {
                judgeDirection = Direction.Down;

                // playerManager.ShieldMove(Direction.Down);
                // scoreManager.Judge(Direction.Down);
            }
        }

        // 조작 한계 시간을 초과한 경우 : 무방향 조작으로 취급, 판정 실시
        else if (StageManager.Instance.currentTime - judgeTime > 0.15f && isSwiping)
        {
            Debug.Log("Maximum swipe time exceeded.");

            judgeDirection = Direction.None;
            
            // playerManager.ShieldMove(Direction.None);
            // scoreManager.Judge(Direction.None);
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
            judgeDirection = Direction.None;
            
            // playerManager.ShieldMove(Direction.None);
            // scoreManager.Judge(Direction.None);
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

            // 터치 시작 : 초기화, 시작점 기록
            if (tempTouchs.phase == TouchPhase.Began && !isSwiping)
            {
                // Debug.Log("Touch on");

                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;
                isSwiping = true;
                
                judgeTime = StageManager.Instance.currentTime;
            }

            // 설정된 터치 길이를 넘었을 경우 : 스와이프한 것으로 취급, 판정 실시
            else if (sumLength > 0.5f && isSwiping)
            {
                lastPos -= initialPos;
                double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
                
                Debug.Log(sumLength);
                Debug.Log(angle);

                isSwiping = false;

                if (angle > 135 || angle <= -135)
                {
                    judgeDirection = Direction.Left;

                    // playerManager.ShieldMove(Direction.Left);
                    // scoreManager.Judge(Direction.Left);
                }
                else if (angle > 45)
                {
                    judgeDirection = Direction.Up;
                    
                    // playerManager.ShieldMove(Direction.Up);
                    // scoreManager.Judge(Direction.Up);
                }
                else if (angle > -45)
                {
                    judgeDirection = Direction.Right;
                    
                    // playerManager.ShieldMove(Direction.Right);
                    // scoreManager.Judge(Direction.Right);
                }
                else
                {
                    judgeDirection = Direction.Down;
                    
                    // playerManager.ShieldMove(Direction.Down);
                    // scoreManager.Judge(Direction.Down);
                }
            }

            // 터치 한계 시간을 초과한 경우 : 무방향 터치로 취급, 판정 실시
            else if (StageManager.Instance.currentTime - judgeTime > 0.15f && isSwiping)
            {
                judgeDirection = Direction.None;
                    
                // playerManager.ShieldMove(Direction.None);
                // scoreManager.Judge(Direction.None);
                isSwiping = false;
            }

            // 터치가 계속 진행 중인 경우 : 터치 길이를 매 프레임 마다 더하기
            else if (isSwiping)
            {
                sumLength += Vector3.Distance(lastPos, Camera.main.ScreenToWorldPoint(tempTouchs.position));
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            }
        }

        // (터치 길이를 채우지 못하고) 터치를 종료했을 경우 : 무방향 터치로 취급, 판정 실시
        if (tempTouchs.phase == TouchPhase.Ended && isSwiping)
        {
            judgeDirection = Direction.None;
            
            // Debug.Log("Touch out");

            // playerManager.ShieldMove(Direction.None);
            // scoreManager.Judge(Direction.None);
            isSwiping = false;
        }
    }
}
