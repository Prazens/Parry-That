using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private StageFlowManager stageFlowManager;
    [SerializeField] private bool isTouchAvailable = false;

    private bool canCloseTutorialPanel = false;
    private float tutorialTimer;
    private float tutorialInputBlockTime = 1.5f;

    private void Start()
    {
        StartStage();
    }

    public void StartStage()
    {
        if (stageFlowManager == null) return;

        stageFlowManager.FirstStartStage();
        canCloseTutorialPanel = false;
        tutorialTimer = 0f;
    }

    private void Update()
    {
        if (stageFlowManager == null) return;

        if (stageFlowManager.isTutorial && !canCloseTutorialPanel)
        {
            tutorialTimer += Time.deltaTime;

            if (tutorialTimer >= tutorialInputBlockTime)
            {
                canCloseTutorialPanel = true;
            }
        }

        if (stageFlowManager.isTutorial && canCloseTutorialPanel)
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

    private void DetectTouch()
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            CloseTutorialPanel();
        }
    }

    private void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CloseTutorialPanel();
        }
    }

    private void CloseTutorialPanel()
    {
        if (stageFlowManager == null || !stageFlowManager.isTutorial) return;

        stageFlowManager.CloseTutorialPanel();
        canCloseTutorialPanel = false;
        tutorialTimer = 0f;
    }
}
