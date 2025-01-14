using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] public PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        KeyChecker();
        // MouseChecker();  // 터치에 중복될 가능성 높음음
        TouchChecker();
    }

    // 임시 조작 확인기
    private void KeyChecker()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerManager.ShieldMove(Direction.Left);
            // scoreManager.Judge(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            playerManager.ShieldMove(Direction.Right);
            // scoreManager.Judge(Direction.Right);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playerManager.ShieldMove(Direction.Up);
            scoreManager.Judge(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playerManager.ShieldMove(Direction.Down);
            scoreManager.Judge(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.Space))  // 방향 스와이프 없이 탭만 할때의 움직임
        {
            playerManager.ShieldMove(Direction.None);
            scoreManager.Judge(Direction.None);
        }
    }


    private Vector3 initialPos;
    private Vector3 lastPos;
    private bool isSwiping = false;
    private double sumLength = 0;

    // 임시 조작 확인기 - 전역 시간 타이머 필요
    private void MouseChecker()
    {
        // if ((Input.GetMouseButtonDown(0) && !isSwiping)
        //     || (Input.GetMouseButton(0) && !isSwiping))  // 재밌?는 옵션
        if (Input.GetMouseButtonDown(0) && !isSwiping)
        {
            initialPos = Input.mousePosition;
            lastPos = Input.mousePosition;
            sumLength = 0;
            isSwiping = true;
        }

        else if (sumLength > 50f && isSwiping)
        {
            lastPos -= initialPos;
            double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;

            isSwiping = false;

            if (angle > 135 || angle <= -135)
            {
                playerManager.ShieldMove(Direction.Left);
                // scoreManager.Judge(Direction.Left);
            }
            else if (angle > 45)
            {
                playerManager.ShieldMove(Direction.Up);
                scoreManager.Judge(Direction.Up);
            }
            else if (angle > -45)
            {
                playerManager.ShieldMove(Direction.Right);
                // scoreManager.Judge(Direction.Right);
            }
            else
            {
                playerManager.ShieldMove(Direction.Down);
                scoreManager.Judge(Direction.Down);
            }
        }

        else if (Input.GetMouseButton(0) && isSwiping)
        {
            sumLength += Vector3.Distance(lastPos, Input.mousePosition);
            lastPos = Input.mousePosition;
            
        }

        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            playerManager.ShieldMove(Direction.None);
            scoreManager.Judge(Direction.None);
            isSwiping = false;
        }
    }

    private Touch tempTouchs;
    // Touch Checker - 전역 시간 타이머로 개선 필요 - 판정 시스템과 연동
    private void TouchChecker()
    {

        if (Input.touchCount > 0)
        {
            // Debug.Log($"{Input.touchCount} / {isSwiping} / {sumLength}");

            tempTouchs = Input.GetTouch(0);
            if (tempTouchs.phase == TouchPhase.Began && !isSwiping)
            {
                // Debug.Log("Touch on");

                initialPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
                sumLength = 0;
                isSwiping = true;
            }

            else if (sumLength > 0.5f && isSwiping)
            {
                lastPos -= initialPos;
                double angle = Mathf.Atan2(lastPos.y, lastPos.x) * Mathf.Rad2Deg;
                
                Debug.Log(sumLength);
                Debug.Log(angle);

                isSwiping = false;

                if (angle > 135 || angle <= -135)
                {
                    playerManager.ShieldMove(Direction.Left);
                    // scoreManager.Judge(Direction.Left);
                }
                else if (angle > 45)
                {
                    playerManager.ShieldMove(Direction.Up);
                    scoreManager.Judge(Direction.Up);
                }
                else if (angle > -45)
                {
                    playerManager.ShieldMove(Direction.Right);
                    // scoreManager.Judge(Direction.Right);
                }
                else
                {
                    playerManager.ShieldMove(Direction.Down);
                    scoreManager.Judge(Direction.Down);
                }
            }

            else if (isSwiping)
            {
                sumLength += Vector3.Distance(lastPos, Camera.main.ScreenToWorldPoint(tempTouchs.position));
                lastPos = Camera.main.ScreenToWorldPoint(tempTouchs.position);
            }
        }

        if (tempTouchs.phase == TouchPhase.Ended && isSwiping)
        {
            // Debug.Log("Touch out");

            playerManager.ShieldMove(Direction.None);
            scoreManager.Judge(Direction.None);
            isSwiping = false;
        }

        // else
        // {
        //     isSwiping = false;
        // }
    }
}
