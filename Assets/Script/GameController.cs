using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager; // StageManager 연결
    // Start is called before the first frame update
    void Start()
    {
        //if (stageManager != null)
        //{
        //    stageManager.ResetStage(); // 스테이지 초기화
        //}
        //if (stageManager != null)
        //{
        //   stageManager.StartStage(); // 스테이지 시작
        //}
        //else
        //{
        //   Debug.LogError("StageManager is not assigned!");
        //}

        StartStage();
        Debug.Log("게임시작");
        // switch (SceneLinkage.StageLV)
        // {
        //     case 1:
        //         StartStage();
        //         Debug.Log("게임시작");
        //         break;
        //     case 2:
        //         //StartStage()
        //         break;
        //     case 3:
        //         //StartStage2()
        //         break;
        //     default:
        //         // Debug.Log($"{SceneLinkage.StageLV}");
        //         break;
        // }
    }
    public void StartStage()
    {
        if (stageManager != null)
        {
            stageManager.FirstStartStage(); // 스테이지 시작
        }
        else
        {
            Debug.LogError("StageManager is not assigned!");
        }
    }
    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private bool isSwiping = false;
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private bool isTouchAvailable = false;

    // Update is called once per frame
    void Update()
    {
        if(stageManager.is_over)
        {
            if (isTouchAvailable)   
            {
                DetectSwipe();
            }
            else    
            { 
                DetectMouseSwipe();
            }
        }
        if(stageManager.isPaused)
        {
            if (isTouchAvailable)   
            {
                DetectSwipe();
            }
            else    
            { 
                DetectMouseSwipe();
            }
        }
    }
    private void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                touchEndPosition = touch.position;

                // 스와이프 방향 확인
                Vector2 direction = touchEndPosition - touchStartPosition;

                if (direction.magnitude > swipeThreshold)
                {
                    float verticalSwipe = direction.y;

                    // 위로 스와이프 감지
                    if (verticalSwipe > 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
                    {
                        OnSwipeUp();
                    }

                    // 아래로 스와이프 감지
                    if (verticalSwipe < 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
                    {
                        OnSwipeDown();
                    }
                }

                isSwiping = false;
            }
        }
    }
    private void DetectMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPosition = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            touchEndPosition = Input.mousePosition;

            ProcessSwipe();
            isSwiping = false;
        }
    }
    private void ProcessSwipe()
    {
        // 스와이프 방향 및 거리 계산
        Vector2 direction = touchEndPosition - touchStartPosition;

        if (direction.magnitude > swipeThreshold)
        {
            float verticalSwipe = direction.y;

            // 위로 스와이프 감지
            if (verticalSwipe > 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
            {
                OnSwipeUp();
            }

            // 아래로 스와이프 감지
            if (verticalSwipe < 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
            {
                OnSwipeDown();
            }
        }
    }
    private void OnSwipeUp()
    {
        Debug.Log("Swipe Up Detected");

        // 결과창 활성화 상태에서만 RestartStage 호출
        if (stageManager != null && stageManager.is_over)
        {
            stageManager.RestartStage();
        }
        if (stageManager != null && stageManager.isPaused)
        {
            stageManager.RestartStage();
        }
    }

    private void OnSwipeDown()
    {
        SceneManager.LoadScene("Main");
        Time.timeScale = 1f;
    }


}
