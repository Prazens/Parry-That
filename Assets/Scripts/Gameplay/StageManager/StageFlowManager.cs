using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StageFlowManager : MonoBehaviour
{
    public static StageFlowManager Instance; // 전역 접근용 싱글턴

    public float currentTime { get; private set; } // 현재 스테이지 시간
    public float stageDuration = 180f; // 스테이지 전체 길이 (초)

    public static bool isActive = false; // 스테이지 활성화 여부
    public bool is_over = false;

    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private BossController boss;

    [Header("Managers")]
    [SerializeField] private StageSpawnManager stageSpawnManager;
    [SerializeField] private StageChartLoader stageChartLoader;
    [SerializeField] private StageAudioManager stageAudioManager;
    [SerializeField] private StaticUIManager staticUIManager;
    [SerializeField] private DynamicUIManager dynamicUIManager;
    [SerializeField] private StageResultManager stageResultManager;
    [SerializeField] private JudgeSystem judgeSystem;

    private int clearStrikers = 0;
    private bool button_active = true;

    private bool AnimationEnable = true;

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

        // 음악 재생 조건(원래 StageManager.Update 로직)
        if (stageAudioManager != null && stageAudioManager.musicSource != null && !stageAudioManager.musicPlayed)
        {
            if (!TutorialManager.isTutorial)
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

        // 스테이지 종료 조건 + 승리 연출 (원래 방식에 맞춰 폴링)
        if (currentTime >= stageDuration && AnimationEnable)
        {
            if (!victorySequenceTriggered)
            {
                victorySequenceTriggered = true;

                if (staticUIManager != null)
                {
                    staticUIManager.StartVictoryAnimation();
                    victoryStarted = true;
                }
                else
                {
                    EndStage();
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
        currentTime = 0f;
        clearStrikers = 0;
        is_over = false;
        isPaused = false;
        button_active = true;
        AnimationEnable = true;
        victorySequenceTriggered = false;
        victoryStarted = false;

        Time.timeScale = 1f;

        if (strikerManager != null)
        {
            strikerManager.ClearStrikers();
        }

        if (dynamicUIManager != null)
        {
            dynamicUIManager.Initialize_UI();
        }

        // 오디오 초기화 (원래 StartStage에서 하던 musicSource.time=0f 대응)
        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            stageAudioManager.musicSource.time = 0f;
            stageAudioManager.musicPlayed = false;
        }

        if (boss != null)
        {
            boss.clearHp();
        }

        if (stageSpawnManager != null)
        {
            stageSpawnManager.SpawnPlayer();
            // stageSpawnManager.SpawnGuideboxes();
        }

        if (stageChartLoader != null)
        {
            stageChartLoader.LoadChartsIntoStrikerManager();
        }
        else
        {
            if (strikerManager != null) strikerManager.charts.Clear();
        }

        if (strikerManager != null)
        {
            strikerManager.InitStriker(0);
        }

        if (judgeSystem != null) 
        {
            judgeSystem.Initialize();
        }

        // 정지 UI 숨김 (토글 기반)
        if (staticUIManager != null)
        {
            staticUIManager.ToggleOverlay(false);
            staticUIManager.ToggleClearPanel(false);
            staticUIManager.ToggleGameOverPanel(false);
            staticUIManager.TogglePausePanel(false);
            staticUIManager.UpdatePauseButtonSprite(false);
        }

        isActive = true;
    }

    public void RestartStage()
    {
        AnimationEnable = true;
        victorySequenceTriggered = false;
        victoryStarted = false;

        Time.timeScale = 1f;

        if (strikerManager != null)
        {
            strikerManager.ClearStrikers();
        }

        // 음악 정지 (StopAudio() 대신 musicSource.Stop())
        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            if (stageAudioManager.musicSource.isPlaying)
            {
                stageAudioManager.musicSource.Stop();
            }
        }

        // 정지 UI 숨김 + 버튼 원복 (토글 기반)
        if (staticUIManager != null)
        {
            staticUIManager.ToggleOverlay(false);
            staticUIManager.ToggleClearPanel(false);
            staticUIManager.ToggleGameOverPanel(false);
            staticUIManager.TogglePausePanel(false);
            staticUIManager.UpdatePauseButtonSprite(false);
        }

        // 컷인 중지
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

        // 음악 정지
        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            if (stageAudioManager.musicSource.isPlaying)
            {
                stageAudioManager.musicSource.Stop();
            }
        }

        // 결과 처리
        if (stageResultManager != null)
        {
            stageResultManager.ProcessGameOverResult();
        }

        // GameOver UI 표시 (토글 기반)
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

        // 음악 정지
        if (stageAudioManager != null && stageAudioManager.musicSource != null)
        {
            if (stageAudioManager.musicSource.isPlaying)
            {
                stageAudioManager.musicSource.Stop();
            }
        }

        // 결과 처리(별 계산 + 저장 + 최고점 갱신)
        if (stageResultManager != null)
        {
            stageResultManager.ProcessClearResult();
        }

        // Clear UI 표시 (토글 기반)
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
}
