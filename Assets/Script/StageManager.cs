using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance; // 전역 접근용 싱글턴
    public float currentTime { get; private set; } // 현재 스테이지 시간
    public float stageDuration = 180f; // 스테이지 전체 길이 (초)
    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private GameObject playerPrefab; // Player 프리팹
    [SerializeField] private GameObject guidebox1Prefab; // Guidebox1 프리팹
    [SerializeField] private GameObject guidebox2Prefab; // Guidebox2 프리팹
    private GameObject playerInstance; // 생성된 Player 인스턴스
    private GameObject guidebox1Instance; // 생성된 Guidebox1 인스턴스
    private GameObject guidebox2Instance; // 생성된 Guidebox2 인스턴스
    [SerializeField] private GameObject clearPanelPrefab; // Clear 창 Prefab
    private GameObject clearPanelInstance;
    [SerializeField] private GameObject gameOverPanelPrefab; // GameOver 창 Prefab
    private GameObject gameOverPanelInstance;
    [SerializeField] private GameObject pausePanelPrefab; // Pause 창 Prefab
    private GameObject PausePanelInstance;
    [SerializeField] private Transform canvasTransform; // Canvas의 Transform
    [SerializeField] private GameController gameController; // GameController 참조
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private AudioSource musicSource; // 음악 재생을 위한 AudioSource
    private bool musicPlayed = false; // 음악이 재생되었는지 확인
    public static bool isActive = false; // 스테이지 활성화 여부
    private int clearStrikers = 0;
    public bool is_over = false;
    [SerializeField] private TextMeshProUGUI countdownText; // 카운트다운 표시용 Text UI
    private GameObject overlay; // 검은 필터
    private bool button_active = true;

    [SerializeField] private TextAsset[] jsonCharts;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // 검은 필터 오버레이 생성
        overlay = new GameObject("BlackOverlay");
        overlay.transform.SetParent(canvasTransform, false);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.7f);
        overlayImage.raycastTarget = false;
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlay.SetActive(false);

        if (clearPanelPrefab != null && canvasTransform != null)
        {
            // Clear 창 인스턴스 생성
            clearPanelInstance = Instantiate(clearPanelPrefab, canvasTransform);
            clearPanelInstance.SetActive(false);
        }
        if (gameOverPanelPrefab != null && canvasTransform != null)
        {
            gameOverPanelInstance = Instantiate(gameOverPanelPrefab, canvasTransform);
            gameOverPanelInstance.SetActive(false); // 초기 비활성화
        }
        if (pausePanelPrefab != null && canvasTransform != null)
        {
            // Clear 창 인스턴스 생성
            PausePanelInstance = Instantiate(pausePanelPrefab, canvasTransform);
            PausePanelInstance.SetActive(false);
        }
        // 카운트다운 UI 숨김
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
    public void FirstStartStage()
    {
        // GameObject Menu = GameObject.Find("Menu");
        // Menu.SetActive(false);
        GameObject InGameScreen = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "InGameScreen");
        // Debug.Log(InGameScreen != null ? "InGameScreen found" : "InGameScreen not found");
        InGameScreen.SetActive(true);
        musicSource.Play(); // 음악 재생
        musicSource.Stop(); 
        musicSource.time = 0f;
        StartStage();
    }
    private void StartStage()
    {
        currentTime = 0f; // 시간 초기화
        musicPlayed = false;
        clearStrikers = 0;
        is_over = false;
        isPaused = false;
        button_active = true;
        scoreUI.Initialize_UI();
        musicSource.time = 0f;
        SpawnPlayer();
        // SpawnGuideboxes();
        for (int i = 0; i < 2; i++)
        {
            strikerManager.charts[i] = JsonReader.ReadJson(jsonCharts[i]);
        }
        strikerManager.SpawnStriker(0,0,1,108,107); 
        strikerManager.SpawnStriker(1,1,1,110,107);
        isActive = true; // 스테이지 활성화
        scoreManager.Initialize();
        Debug.Log("Stage Started!");
    }
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned in StageManager!");
            return;
        }

        // 기존 Player 제거
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        // Player 생성 및 중앙에 배치
        playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // PlayerManager 가져오기
        PlayerManager playerManager = playerInstance.GetComponent<PlayerManager>();
        if (playerManager != null)
        {
            // StageManager를 PlayerManager에 설정
            playerManager.stageManager = this;
            // GameController의 TouchManager와 ScoreManager에 PlayerManager 설정
            if (gameController != null)
            {
                TouchManager touchManager = gameController.GetComponent<TouchManager>();
                ScoreManager scoreManager = gameController.GetComponent<ScoreManager>();

                if (touchManager != null)
                {
                    touchManager.playerManager = playerManager;
                }
                else
                {
                    Debug.LogError("TouchManager script is not attached to GameController!");
                }

                if (scoreManager != null)
                {
                    scoreManager.playerManager = playerManager;
                }
                else
                {
                    Debug.LogError("ScoreManager script is not attached to GameController!");
                }
            }
            else
            {
                Debug.LogError("GameController is not assigned in StageManager!");
            }
            if (strikerManager != null)
            {
                strikerManager.SetPlayer(playerManager);
            }

            Debug.Log("PlayerManager successfully linked to TouchManager and ScoreManager.");
        }
        else
        {
            Debug.LogError("PlayerPrefab is missing PlayerManager component!");
        }
    }
    private void SpawnGuideboxes()
    {
        if (guidebox1Prefab == null || guidebox2Prefab == null)
        {
            Debug.LogError("Guidebox prefabs are not assigned in StageManager!");
            return;
        }

        // 기존 Guidebox 제거
        if (guidebox1Instance != null)
        {
            Destroy(guidebox1Instance);
        }
        if (guidebox2Instance != null)
        {
            Destroy(guidebox2Instance);
        }

        // Guidebox1 생성
        guidebox1Instance = Instantiate(guidebox1Prefab, new Vector3(0, 0.6f, 0), Quaternion.identity);
        Debug.Log("Guidebox1 spawned at (0, 0.6, 0).");

        // Guidebox2 생성
        guidebox2Instance = Instantiate(guidebox2Prefab, new Vector3(0, -0.6f, 0), Quaternion.identity);
        Debug.Log("Guidebox2 spawned at (0, -0.6, 0).");
    }


    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        currentTime += Time.deltaTime;

        if (currentTime >= stageDuration)
        {
            EndStage();
        }
        if (currentTime >= 2d && !musicPlayed)
        {
            //SpawnTransparentProjectile();
            musicSource.Play();
            musicPlayed = true;
            Debug.Log("Music Start!");
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!");
        isActive = false;
        is_over = true;
        button_active = false;

        // 음악 정지
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        CalculateStars();

        // GameOver 창 활성화
        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.SetActive(true);
            UpdatePanelScores(gameOverPanelInstance); // 점수 및 판정 업데이트
            UpdateStar_Over();
        }
        overlay.SetActive(true);
    }
    private void EndStage()
    {
        Debug.Log("Stage Complete!");
        // 스테이지 종료 로직 추가
        currentTime = stageDuration; // 시간 고정
        isActive = false;
        is_over = true;
        button_active = false;
        // Clear 창 활성화 및 점수 업데이트
        if (clearPanelInstance != null)
        {
            clearPanelInstance.SetActive(true); // Clear 창 활성화
        }
        CalculateStars();
        UpdatePanelScores(clearPanelInstance);
        UpdateStar_Clear();
        overlay.SetActive(true);

        // 최고 기록 경신하면 데이터베이스에 업데이트
        ScoreManager scoreManager = gameController.GetComponent<ScoreManager>();
        DatabaseManager theDatabase = FindObjectOfType<DatabaseManager>();
        if (scoreManager.score > theDatabase.score[SceneLinkage.StageLV])
        {
            theDatabase.score[SceneLinkage.StageLV] = scoreManager.score;
            theDatabase.SaveScoreData();
            Debug.Log($"최고기록 경신: {theDatabase.score[SceneLinkage.StageLV]}");
        }
    }
    public void RestartStage()
    {
        Debug.Log("Restarting Stage...");
        Time.timeScale = 1f;
        // 기존 Striker 삭제
        strikerManager.ClearStrikers();
        // 음악 정지
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // Clear 창 비활성화
        if (clearPanelInstance != null)
        {
            clearPanelInstance.SetActive(false);
        }
        // Gameover 창 비활성화
        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.SetActive(false);
        }
        if(PausePanelInstance != null) PausePanelInstance.SetActive(false);
        overlay.SetActive(false);

        UIManager.Instance.isStop1 = false;
        UIManager.Instance.isStop2 = false;

        // 새로운 스테이지 시작
        StartStage();
    }
    public bool isPaused = false;
    private float savedMusicTime;
    [SerializeField] private GameObject pauseButton;
    public void TogglePause()
    {
        if(!button_active) return;
        if (isPaused)
        {
            ResumeStage();
        }
        else
        {
            PauseStage();
        }
    }

    public void PauseStage()
    {
        if (isPaused) return;
        isPaused = true;
        isActive = false;
        //투사체 멈추기
        Time.timeScale = 0f;
        // 음악 멈춤 및 재생 시간 저장
        if (musicSource != null && musicPlayed)
        {
            savedMusicTime = musicSource.time;
            musicSource.Pause();
        }
        if(PausePanelInstance != null) PausePanelInstance.SetActive(true);
        UpdatePanelScores(PausePanelInstance);
        overlay.SetActive(true);
        Debug.Log("Stage Paused!");
    }
    public void ResumeStage()
    {
        if (!isPaused) return;
        button_active = false;
        isPaused = false;
        if(PausePanelInstance != null) PausePanelInstance.SetActive(false);
        overlay.SetActive(false);
        StartCoroutine(ResumeAfterDelay());
    }
    private IEnumerator ResumeAfterDelay()
    {
        Debug.Log("Resuming Stage in 3 seconds...");
        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f); // Time.timeScale이 0이어도 실행
        }
        countdownText.gameObject.SetActive(false);
        isActive = true;
        button_active = true;
        Time.timeScale = 1f;
        // 음악 재개
        if (musicSource != null && musicPlayed)
        {
            musicSource.time = savedMusicTime;
            musicSource.Play();
        }
        //투사체 이동 재개

        Debug.Log("Stage Resumed!");
    }
    private void UpdatePanelScores(GameObject panelInstance)
    {
        if (panelInstance != null)
        {
            // 텍스트 컴포넌트 가져오기
            TextMeshProUGUI parfectText = panelInstance.transform.Find("ParfectText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bounceText = panelInstance.transform.Find("BounceText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI guardText = panelInstance.transform.Find("GuardText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hitText = panelInstance.transform.Find("HitText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = panelInstance.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();

            if (scoreManager != null && scoreManager.judgeDetails != null)
            {
                List<int[]> judgeDetails = scoreManager.judgeDetails;

                // 각각의 값을 최소 4자리 정수로 포맷팅하여 텍스트에 설정
                if (parfectText != null) 
                    parfectText.text = judgeDetails[0][4].ToString("D4");

                if (bounceText != null) 
                    bounceText.text = (judgeDetails[0][3] + judgeDetails[0][5]).ToString("D4");

                if (guardText != null) 
                    guardText.text = (judgeDetails[0][2] + judgeDetails[0][6]).ToString("D4");

                if (hitText != null) 
                    hitText.text = judgeDetails[0][1].ToString("D4");

                if (scoreText != null) 
                    scoreText.text = Convert.ToString(scoreManager.score);
            }
            else
            {
                Debug.LogError("ScoreManager or judgeDetails is null!");
            }
        }
        else
        {
            Debug.LogError("Panel instance is null!");
        }
    }
    private void UpdateStar_Clear()
    {
        if (clearPanelInstance != null)
        {
            // 노란 별 오브젝트 찾기
            GameObject star1 = clearPanelInstance.transform.Find("Star1").gameObject;
            GameObject star2 = clearPanelInstance.transform.Find("Star2").gameObject;
            GameObject star3 = clearPanelInstance.transform.Find("Star3").gameObject;

            // 별 활성화/비활성화
            if (star1 != null) star1.SetActive(true); // 1개 조건
            if (star2 != null) star2.SetActive(clearStrikers >= 1); // 2개 조건 
            if (star3 != null) star3.SetActive(clearStrikers >= 2); // 3개 조건 

            int currentStars = 0;
            if (clearStrikers >= 0) currentStars = 1;
            if (clearStrikers >= 1) currentStars = 2;
            if (clearStrikers >= 2) currentStars = 3;

            // 데이터베이스에 별 개수 저장
            DatabaseManager theDatabase = FindObjectOfType<DatabaseManager>();
            if (theDatabase != null)
            {
                // 현재 저장된 별 개수보다 클 경우 업데이트
                if (currentStars > theDatabase.star[SceneLinkage.StageLV])
                {
                    theDatabase.star[SceneLinkage.StageLV] = currentStars;
                    theDatabase.SaveStarData();
                    Debug.Log($"별 개수 갱신: 스테이지 {SceneLinkage.StageLV}에 {currentStars}개 달성");
                }
            }
        }
    }
    private void UpdateStar_Over()
    {
        if (clearPanelInstance != null)
        {
            // 노란 별 오브젝트 찾기
            GameObject star1 = gameOverPanelInstance.transform.Find("Star1").gameObject;
            GameObject star2 = gameOverPanelInstance.transform.Find("Star2").gameObject;
            GameObject star3 = gameOverPanelInstance.transform.Find("Star3").gameObject;

            // 별 활성화/비활성화
            if (star1 != null) star1.SetActive(false); // 1개 조건
            if (star2 != null) star2.SetActive(clearStrikers >= 1); // 2개 조건 
            if (star3 != null) star3.SetActive(clearStrikers >= 2); // 3개 조건 
        }
    }
    private void CalculateStars()
    {
        // StrikerManager에서 모든 Striker 확인
        foreach (GameObject striker in strikerManager.strikerList)
        {
            StrikerController strikerController = striker.GetComponent<StrikerController>();
            if (strikerController != null && strikerController.hp <= 0)
            {
                clearStrikers++;
            }
        }
    }
}
