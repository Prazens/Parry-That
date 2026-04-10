using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageFlowManager : MonoBehaviour
{
    public static StageFlowManager Instance;

    public float currentTime { get; private set; }
    public float bpm { get; private set; } = 60f; // 기본값
    public float stageDuration = 180f;

    public static bool isActive = false;
    public bool is_over = false;
    public bool isDaehwa = false;
    public bool isTutorial = false; //튜토리얼 패널이 띄워져 있으면 true, 아니면 false
    public bool isClear = false;
    private bool tutorialPanelShown = false;

    private float phaseEndTime = -1f;
    public int currentPhaseIndex { get; private set; } = 0;

    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private StageDialogManager dialogueManager;

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

    // currentTime이 (-1 * STAGE_READY_TIME)부터 시작함.
    public const float STAGE_READY_TIME = 2.0f;

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
            stageDuration = stageAudioManager.musicSource.clip.length + 1f;
        }
    }

    private void Update()
    {
        if (isDaehwa) return;
        if (!isActive) return;
        if (isPaused) return;
        if (isTutorial) return;
        if (isClear) return;


        // 음악이 재생 중일 때는 오디오 소스의 시간을 직접 참조하여 음악과 동기화
        if (stageAudioManager != null && stageAudioManager.musicSource.isPlaying)
        {
            currentTime = stageAudioManager.musicSource.time + stageAudioManager.bgmOffset;
        }
        else
        {
            currentTime += Time.deltaTime;
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null && !stageAudioManager.musicPlayed)
        {
            if (stageAudioManager.musicSource.clip != null)
            {
                // 튜토리얼이 아닐 때만 오프셋을 적용하여 재생
                if (currentStageData != null && currentStageData.Category != StageCategory.Tutorial)
                {
                    // currentTime이 bgmOffset에 도달하면 음악 재생
                    if (currentTime >= stageAudioManager.bgmOffset)
                    {
                        stageAudioManager.musicSource.Play();
                        stageAudioManager.musicPlayed = true;
                    }
                }
                else
                {
                    // 튜토리얼은 즉시 재생
                    stageAudioManager.bgmOffset = 0f;
                    stageAudioManager.musicSource.Play();
                    stageAudioManager.musicPlayed = true;
                }
            }
            else
            {
                stageAudioManager.musicPlayed = true;
            }
        }

        if (phaseEndTime >= 0f && currentTime >= phaseEndTime)
        {
            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null && tutorialManager.ShouldRestartCurrentPhase())
            {
                RestartPhase();
                return;
            }

            isActive = false;
            isDaehwa = true;
            StartCoroutine(RunCurrentPhaseDialogue());
            return;
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null && currentTime >= stageAudioManager.musicSource.clip.length - 0.05f)
        {
            if (strikerManager != null && strikerManager.isBossStage)
            {
                strikerManager.ClearBoss();
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
                    if (stageAudioManager != null)
                    {
                        stageAudioManager.StopAudio();
                    }
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

    public void SetBPM(float _bpm)
    {
        bpm = _bpm;
    }

    /// <summary>
    /// 박자(Beat)를 시간(Seconds)으로 단위 변환
    /// </summary>
    public float BeatToSec(float beatIndex)
    => beatIndex * (60f / bpm);

    public void FirstStartStage()
    {
        GameObject inGameScreen = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(obj => obj.name == "InGameScreen");

        if (inGameScreen != null)
        {
            inGameScreen.SetActive(true);
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            AudioSource musicSource = stageAudioManager.musicSource;
            musicSource.Play();
            musicSource.Stop();
            musicSource.time = 0f;
            stageAudioManager.musicPlayed = false;
        }

        StartStage();
    }

    public void StartStage()
    {
        currentPhaseIndex = 0;

        ResolveStageData();

        if (stageAudioManager != null)
        {
            stageAudioManager.ApplyStageData(currentStageData);
        }

        if (stageSetupManager != null)
        {
            stageSetupManager.SpawnPlayer();
            stageSetupManager.ApplyStageModules();
            stageSetupManager.SetCutIn(currentStageData);
        }

        if (stageAudioManager != null && stageAudioManager.musicSource != null && stageAudioManager.musicSource.clip != null)
        {
            stageDuration = stageAudioManager.musicSource.clip.length + 1f;
        }

        currentTime = -STAGE_READY_TIME;
        is_over = false;
        isPaused = false;
        isDaehwa = false;
        isTutorial = false;
        isClear = false;
        tutorialPanelShown = false;
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

        if (strikerManager != null)
        {
            strikerManager.ClearImmediately();
        }

        if (staticUIManager != null)
        {
            staticUIManager.Setup_UI();
        }

        StartCurrentPhase();
    }

    public void RestartStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void StartCurrentPhase()
    {
        if (currentStageData == null)
        {
            EndStage();
            return;
        }
        

        IReadOnlyList<StagePhase> phases = currentStageData.Phases;
        if (phases == null)
        {
            EndStage();
            return;
        }

        if (currentPhaseIndex < 0)
        {
            EndStage();
            return;
        }

        if (currentPhaseIndex >= phases.Count)
        {
            if (currentStageData.Category == StageCategory.Tutorial)
            {
                EndStage();
            }
            else
            {
                isActive = true;
                isDaehwa = false;
                phaseEndTime = -1f;
            }
            return;
        }

        StagePhase phase = phases[currentPhaseIndex];
        if (phase == null)
        {
            GoToNextPhase();
            return;
        }

        bool hasTutorialPanel = !tutorialPanelShown && stageChartLoader != null && stageChartLoader.HasCurrentPhaseTutorialPanel(currentStageData);
        bool hasChart = stageChartLoader != null && stageChartLoader.HasCurrentPhaseChart(currentStageData);
        bool hasDialogue = stageChartLoader != null && stageChartLoader.HasCurrentPhaseDialogue(currentStageData);

        if (!hasTutorialPanel && !hasChart && !hasDialogue)
        {
            GoToNextPhase();
            return;
        }

        if (hasTutorialPanel)
        {
            tutorialPanelShown = true;
            isTutorial = true;
            isActive = false;
            isDaehwa = false;
            phaseEndTime = -1f;

            if (stageAudioManager != null)
            {
                stageAudioManager.AudioPause();
            }

            if (staticUIManager != null && stageChartLoader != null)
            {
                GameObject tutorialPanelPrefab = stageChartLoader.GetCurrentPhaseTutorialPanel(currentStageData);
                staticUIManager.ShowTutorialPanel(tutorialPanelPrefab);
            }

            return;
        }

        if (hasChart)
        {
            phaseEndTime = -1f;

            if (stageChartLoader != null)
            {
                phaseEndTime = stageChartLoader.PhaseEndTime(currentStageData);
                stageChartLoader.LoadChartsFromStageData(currentStageData);
            }

            ApplyDialogueAudioPolicyAfterDialogue();
            
            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null && currentStageData.Category == StageCategory.Tutorial)
            {
                tutorialManager.OnPhaseStarted();
            }

            isDaehwa = false;
            isActive = true;
            isTutorial = false;
            return;
        }

        isActive = false;
        isDaehwa = true;
        StartCoroutine(RunCurrentPhaseDialogue());
    }

    public void RestartPhase()
    {
        isActive = false;
        isDaehwa = false;
        isTutorial = false;
        isClear = false;
        phaseEndTime = -1f;

        if (stageAudioManager != null)
        {
            stageAudioManager.RestartAudioFromSavedTime();
        }

        currentTime = stageAudioManager.musicSource.time + stageAudioManager.bgmOffset;

        StartCurrentPhase();
    }

    public void GameOver()
    {
        isActive = false;
        is_over = true;
        button_active = false;

        if (stageAudioManager != null && stageAudioManager.musicSource != null && stageAudioManager.musicSource.isPlaying)
        {
            stageAudioManager.musicSource.Stop();
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
        //튜토리얼이면 메인으로
        if (currentStageData.Category == StageCategory.Tutorial)
        {
            FindObjectOfType<TutorialManager>().SkipOn();
        }
        else
        {
            currentTime = stageDuration;
            isActive = false;
            button_active = false;
            isClear = true;

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
    }

    public void CloseTutorialPanel()
    {
        if (!isTutorial)
            return;

        isTutorial = false;

        if (staticUIManager != null)
        {
            staticUIManager.HideTutorialPanel();
            print(1);
        }

        if (stageAudioManager != null)
        {
            stageAudioManager.AudioUnPause();
            print(2);
        }

        StartCurrentPhase();
    }

    private IEnumerator RunCurrentPhaseDialogue()
    {
        isDaehwa = true;
        ApplyDialogueAudioPolicyBeforeDialogue();

        DialogueData dialogue = null;

        if (stageChartLoader != null)
        {
            dialogue = stageChartLoader.GetCurrentPhaseDialogue(currentStageData);
        }

        if (dialogue != null && dialogueManager != null)
        {
            dialogueManager.StartDialog(dialogue);
            yield return new WaitUntil(() => !dialogueManager.isDialogPlaying);
        }

            isDaehwa = false;
            GoToNextPhase();
        }

    private void ApplyDialogueAudioPolicyBeforeDialogue()
    {
        if (stageChartLoader == null)
            return;

        DialogueAudioPolicy policy = stageChartLoader.GetDialogueAudioPolicy(currentStageData);

        switch (policy)
        {
            case DialogueAudioPolicy.KeepPlaying:
                break;

            case DialogueAudioPolicy.PauseAndResume:
                if (stageAudioManager != null)
                {
                    stageAudioManager.AudioPause();
                }
                break;

            case DialogueAudioPolicy.ResetAndReplay:
                if (stageAudioManager != null && stageAudioManager.musicSource != null)
                {
                    stageAudioManager.musicSource.Stop();
                    stageAudioManager.musicSource.time = 0f;
                    stageAudioManager.musicPlayed = false;
                }

                currentTime = 0f;
                break;
        }
    }

    private void ApplyDialogueAudioPolicyAfterDialogue()
    {
        if (stageChartLoader == null)
            return;

        DialogueAudioPolicy policy = stageChartLoader.GetDialogueAudioPolicy(currentStageData);

        switch (policy)
        {
            case DialogueAudioPolicy.KeepPlaying:
                break;

            case DialogueAudioPolicy.PauseAndResume:
                if (stageAudioManager != null)
                {
                    stageAudioManager.AudioUnPause();
                }
                break;

            case DialogueAudioPolicy.ResetAndReplay:
                break;
        }
    }

    private void GoToNextPhase()
    {
        phaseEndTime = -1f;
        tutorialPanelShown = false;
        currentPhaseIndex++;
        StartCurrentPhase();
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

        if (stageResultManager != null)
        {
            stageResultManager.ProcessPauseResult();
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