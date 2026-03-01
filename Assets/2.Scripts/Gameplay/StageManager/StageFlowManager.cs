using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StageFlowManager : MonoBehaviour
{
    public static StageFlowManager Instance;

    public float currentTime { get; private set; }
    public float stageDuration = 180f;

    public static bool isActive = false;
    public bool is_over = false;

    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] public BossController bossController;

    [Header("Managers")]
    [SerializeField] private StageSetupManager stageSetupManager;
    [SerializeField] private StageChartLoader stageChartLoader;
    [SerializeField] private StageAudioManager stageAudioManager;
    [SerializeField] private StaticUIManager staticUIManager;
    [SerializeField] private DynamicUIManager dynamicUIManager;
    [SerializeField] private StageResultManager stageResultManager;
    [SerializeField] private JudgeSystem judgeSystem;

    [Header("Stage Data")]
    [SerializeField] private StageLevelManager stageLevelManager;
    public StageData currentStageData;

    private bool button_active = true;

    public bool isPaused = false;

    private bool victorySequenceTriggered = false;
    private bool victoryStarted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResolveStageData();

        if (stageAudioManager != null)
        {
            stageAudioManager.ApplyStageData(currentStageData);
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null && stageAudioManager.musicSource.clip != null)
        {
            stageDuration = stageAudioManager.musicSource.clip.length + stageAudioManager.musicOffset + 1f;
        }
    }

    private void Update()
    {
        if (!isActive) return;
        if (isPaused) return;
        if (is_over) return;

        currentTime += Time.deltaTime;

        // 음악 재생 조건(기존 로직 유지)
        if (stageAudioManager != null && stageAudioManager.musicSource != null && !stageAudioManager.musicPlayed)
        {
            if (stageAudioManager.musicSource.clip != null)
            {
                if (currentStageData != null && currentStageData.Category != StageCategory.Tutorial)
                {
                    if (currentTime >= 2f)
                    {
                        stageAudioManager.musicSource.Play();
                        stageAudioManager.musicPlayed = true;
                    }
                }
                else
                {
                    stageAudioManager.musicSource.Play();
                    stageAudioManager.musicPlayed = true;
                }
            }
            else
            {
                stageAudioManager.musicPlayed = true;
            }
        }

        if (currentTime >= stageDuration)
        {
            if (!victorySequenceTriggered)
            {
                victorySequenceTriggered = true;

                if (staticUIManager != null)
                {
                    staticUIManager.StartVictoryAnimation();
                    victoryStarted = true;
                    stageAudioManager.StopAudio();
                }
            }

            if (victoryStarted && staticUIManager != null)
            {
                if (staticUIManager.VictoryAnimationFinished())
                {
                    EndStage();
                }
            }
        }
    }

    public void FirstStartStage()
    {
        GameObject InGameScreen = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(obj => obj.name == "InGameScreen");
        if (InGameScreen != null)
        {
            InGameScreen.SetActive(true);
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            var musicSource = stageAudioManager.musicSource;

            musicSource.Play();
            musicSource.Stop();

            musicSource.time = 0f;
            stageAudioManager.musicPlayed = false;
        }

        StartStage();
    }

    public void StartStage()
    {
        ResolveStageData();

        if (stageAudioManager != null)
        {
            stageAudioManager.ApplyStageData(currentStageData);
        }

        if (stageSetupManager != null)
        {
            stageSetupManager.ApplyStageModules();
            stageSetupManager.SetCutIn(currentStageData);
            stageSetupManager.SpawnPlayer();
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null && stageAudioManager.musicSource.clip != null)
        {
            stageDuration = stageAudioManager.musicSource.clip.length + stageAudioManager.musicOffset + 1f;
        }

        currentTime = 0f;
        is_over = false;
        isPaused = false;
        button_active = true;
        victorySequenceTriggered = false;
        victoryStarted = false;

        Time.timeScale = 1f;

        if (dynamicUIManager != null)
        {
            dynamicUIManager.Setup_UI();
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            stageAudioManager.musicSource.time = 0f;
            stageAudioManager.musicPlayed = false;
        }

        if (bossController != null)
        {
            bossController.clearHp();
        }

        if (stageChartLoader != null)
        {
            if (currentStageData != null)
            {
                stageChartLoader.LoadChartsFromStageData(currentStageData);
            }
        }
        else
        {
            if (strikerManager != null) strikerManager.charts.Clear();
        }

        if (strikerManager != null)
        {
            strikerManager.ClearStrikers();
            strikerManager.InitStriker(0);
        }

        if (judgeSystem != null)
        {
            judgeSystem.Initialize();
        }

        if (staticUIManager != null)
        {
            staticUIManager.Setup_UI();
        }

        isActive = true;
    }

    public void RestartStage()
    {
        victorySequenceTriggered = false;
        victoryStarted = false;

        Time.timeScale = 1f;

        if (strikerManager != null)
        {
            strikerManager.ClearStrikers();
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            if (stageAudioManager.musicSource.isPlaying)
            {
                stageAudioManager.musicSource.Stop();
            }
        }

        if (staticUIManager != null)
        {
            staticUIManager.ToggleOverlay(false);
            staticUIManager.ToggleClearPanel(false);
            staticUIManager.ToggleGameOverPanel(false);
            staticUIManager.TogglePausePanel(false);
            staticUIManager.UpdatePauseButtonSprite(false);
            staticUIManager.ResetVictoryAnimation();
        }

        if (dynamicUIManager != null)
        {
            dynamicUIManager.CutInDisplay(0f, true);
        }

        StartStage();
    }

    public void GameOver()
    {
        isActive = false;
        is_over = true;
        button_active = false;

        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            if (stageAudioManager.musicSource.isPlaying)
            {
                stageAudioManager.musicSource.Stop();
            }
        }

        if (stageResultManager != null)
        {
            stageResultManager.ProcessGameOverResult();
        }

        if (staticUIManager != null)
        {
            staticUIManager.ToggleGameOverPanel(true);
            staticUIManager.ToggleOverlay(true);
        }
    }

    private void EndStage()
    {
        currentTime = stageDuration;
        isActive = false;
        is_over = true;
        button_active = false;


        if (stageResultManager != null)
        {
            stageResultManager.ProcessClearResult();
        }

        if (staticUIManager != null)
        {
            staticUIManager.ToggleClearPanel(true);
            staticUIManager.ToggleOverlay(true);
        }
    }

    public void TogglePause()
    {
        if (!button_active) return;

        if (isPaused) ResumeStage();
        else PauseStage();
    }

    public void PauseStage()
    {
        if (is_over) return;

        isPaused = true;
        isActive = false;
        Time.timeScale = 0f;

        if (stageAudioManager != null)
        {
            stageAudioManager.AudioPause();
        }

        if (staticUIManager != null)
        {
            staticUIManager.TogglePausePanel(true);
            staticUIManager.ToggleOverlay(true);
            staticUIManager.UpdatePauseButtonSprite(true);
        }
    }

    public void ResumeStage()
    {
        if (!isPaused) return;

        button_active = false;
        isPaused = false;

        if (staticUIManager != null)
        {
            staticUIManager.TogglePausePanel(false);
            staticUIManager.ToggleOverlay(false);
            staticUIManager.UpdatePauseButtonSprite(false);
        }

        StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        if (staticUIManager != null)
        {
            yield return StartCoroutine(staticUIManager.ResumeCountDown());
        }

        isActive = true;
        button_active = true;
        Time.timeScale = 1f;

        if (stageAudioManager != null)
        {
            stageAudioManager.AudioUnPause();
        }
    }

    public void ChangeTime(float time)
    {
        currentTime = time;
    }

    private void ResolveStageData()
    {
        if (stageLevelManager == null)
        {
            currentStageData = null;
            return;
        }

        if (!StageSelection.HasValidSelection())
        {
            currentStageData = null;
            return;
        }

        currentStageData = stageLevelManager.GetStageData(
            StageSelection.SelectedStageId,
            StageSelection.SelectedDifficulty
        );
    }
}
