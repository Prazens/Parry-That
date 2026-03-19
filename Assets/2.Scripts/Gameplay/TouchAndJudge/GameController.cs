using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private StageFlowManager stageFlowManager; // StageFlowManager 연결

    void Start()
    {
        StartStage();
    }

    public void StartStage()
    {
        if (stageFlowManager != null)
        {
            stageFlowManager.FirstStartStage(); // 스테이지 시작
        }
        else
        {
            // Debug.LogError("StageFlowManager is not assigned!");
        }
    }

    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private bool isSwiping = false;
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private bool isTouchAvailable = false;

    void Update()
    {
        if (stageFlowManager == null) return;

        if (stageFlowManager.is_over)
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

        if (stageFlowManager.isPaused)
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

                Vector2 direction = touchEndPosition - touchStartPosition;

                if (direction.magnitude > swipeThreshold)
                {
                    float verticalSwipe = direction.y;

                    if (verticalSwipe > 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
                    {
                        OnSwipeUp();
                    }

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
        Vector2 direction = touchEndPosition - touchStartPosition;

        if (direction.magnitude > swipeThreshold)
        {
            float verticalSwipe = direction.y;

            if (verticalSwipe > 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
            {
                OnSwipeUp();
            }

            if (verticalSwipe < 0 && Mathf.Abs(verticalSwipe) > Mathf.Abs(direction.x))
            {
                OnSwipeDown();
            }
        }
    }

    private void OnSwipeUp()
    {
        if (stageFlowManager != null && stageFlowManager.is_over)
        {
            stageFlowManager.RestartStage();
        }
        if (stageFlowManager != null && stageFlowManager.isPaused)
        {
            stageFlowManager.RestartStage();
        }
    }

    private void OnSwipeDown()
    {
        if (stageFlowManager.currentStageData.StageId == 4)
        {
            StageSelection.SetSelection(6, 0);
            SceneManager.LoadScene("CutScene");
        }
        else SceneManager.LoadScene("Main");
        Time.timeScale = 1f;
    }
}
