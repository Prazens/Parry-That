using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
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

        if (stageFlowManager.is_over || stageFlowManager.isPaused)
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

        if (stageFlowManager.isTutorial)
        {
            if (isTouchAvailable)
            {
                DetectTouch();
            }
            else
            {
                DetectMouseClick();
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

    private void DetectTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (stageFlowManager != null && stageFlowManager.isTutorial)
                {
                    stageFlowManager.CloseTutorialPanel();
                }
            }
        }
    }

    private void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (stageFlowManager != null && stageFlowManager.isTutorial)
            {
                stageFlowManager.CloseTutorialPanel();
            }
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
        if (stageFlowManager.is_over) 
        {
            SceneManager.LoadScene("testMain"); //게임 오버 시에는 로비로
        }
        else
        {
            if (stageFlowManager.currentStageData.Category == StageCategory.Boss) //보스 클리어 시 에필로그
            {
                CutSceneSelection.SetSelection(stageFlowManager.currentStageData.StageId, CutSceneCategory.Epilogue);
                SceneManager.LoadScene("CutScene");
            }
            else SceneManager.LoadScene("testMain");
        }
        Time.timeScale = 1f;
    }
}
