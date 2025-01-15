using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance; // 전역 접근용 싱글턴
    public float currentTime { get; private set; } // 현재 스테이지 시간
    public float stageDuration = 180f; // 스테이지 전체 길이 (초)
    [SerializeField] private StrikerManager strikerManager;
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

    [SerializeField] private Transform canvasTransform; // Canvas의 Transform
    [SerializeField] private GameController gameController; // GameController 참조
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameObject transparentProjectilePrefab; // 투명 투사체 프리팹
    [SerializeField] private AudioSource musicSource; // 음악 재생을 위한 AudioSource
    private bool musicPlayed = false; // 음악이 재생되었는지 확인
    private bool isActive = false; // 스테이지 활성화 여부
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
    }
    public void StartStage()
    {
        Debug.Log("Stage Started!");
        isActive = true; // 스테이지 활성화
        currentTime = 0f; // 시간 초기화
        musicPlayed = false;
        SpawnPlayer();
        SpawnGuideboxes();
        strikerManager.SpawnStriker(0,0,10,107); 
        strikerManager.SpawnStriker(1,1,15,107); 
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
        if (currentTime >= 2f && !musicPlayed)
        {
            SpawnTransparentProjectile();
            musicPlayed = true;
            Debug.Log("Spawn musicProjectile!");
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!");
        isActive = false;

        // 음악 정지
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // Striker와 Projectile 제거
        strikerManager.ClearStrikers();

        // GameOver 창 활성화
        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.SetActive(true);
            UpdateGameOverPanel(); // 점수 및 판정 업데이트
            UpdateStar_Over();
        }
    }
    private void EndStage()
    {
        Debug.Log("Stage Complete!");
        // 스테이지 종료 로직 추가
        currentTime = stageDuration; // 시간 고정
        isActive = false;
        // Clear 창 활성화 및 점수 업데이트
        if (clearPanelInstance != null)
        {
            clearPanelInstance.SetActive(true); // Clear 창 활성화
        }
        UpdateClearPanelScores();
        UpdateStar_Clear();
    }
    public void RestartStage()
    {
        Debug.Log("Restarting Stage...");
        
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

        // 새로운 스테이지 시작
        StartStage();
    }
    private void SpawnTransparentProjectile()
    {
        if (transparentProjectilePrefab == null || playerInstance == null)
        {
            Debug.LogError("TransparentProjectilePrefab or PlayerInstance is not assigned!");
            return;
        }

        // 투사체 생성
        GameObject projectile = Instantiate(transparentProjectilePrefab, new Vector3(0, 4.0f, 0), Quaternion.identity);

        TransparentProjectile projectileScript = projectile.GetComponent<TransparentProjectile>();
        if (projectileScript != null)
        {
            projectileScript.target = playerInstance.transform; // 플레이어를 타겟으로 설정
            projectileScript.musicSource = musicSource; // 음악 소스 전달
        }
    }
    private void UpdateClearPanelScores()
    {
        if (clearPanelInstance != null)
        {
            // 텍스트 컴포넌트 가져오기
            TextMeshProUGUI parfectText = clearPanelInstance.transform.Find("ParfectText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bounceText = clearPanelInstance.transform.Find("BounceText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI guardText = clearPanelInstance.transform.Find("GuardText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hitText = clearPanelInstance.transform.Find("HitText").GetComponent<TextMeshProUGUI>();
            if (scoreManager != null && scoreManager.judgeDetails != null)
            {
                int[][] judgeDetails = scoreManager.judgeDetails;

                // 각각의 값을 최소 4자리 정수로 포맷팅하여 텍스트에 설정
                if (parfectText != null) 
                    parfectText.text = judgeDetails[0][3].ToString("D4");

                if (bounceText != null) 
                    bounceText.text = (judgeDetails[0][2] + judgeDetails[0][4]).ToString("D4");

                if (guardText != null) 
                    guardText.text = (judgeDetails[0][1] + judgeDetails[0][5]).ToString("D4");

                if (hitText != null) 
                    hitText.text = judgeDetails[0][0].ToString("D4");
            }
            else
            {
                Debug.LogError("ScoreManager or judgeDetails is null!");
            }
        }
    }
    private void UpdateGameOverPanel()
    {
        // 점수 및 판정 업데이트 로직 (Clear 창과 동일하게 구현 가능)
        if (gameOverPanelInstance != null)
        {
            // 텍스트 컴포넌트 가져오기
            TextMeshProUGUI parfectText = gameOverPanelInstance.transform.Find("ParfectText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bounceText = gameOverPanelInstance.transform.Find("BounceText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI guardText = gameOverPanelInstance.transform.Find("GuardText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hitText = gameOverPanelInstance.transform.Find("HitText").GetComponent<TextMeshProUGUI>();

            if (scoreManager != null && scoreManager.judgeDetails != null)
            {
                int[][] judgeDetails = scoreManager.judgeDetails;

                // 각각의 값을 최소 4자리 정수로 포맷팅하여 텍스트에 설정
                if (parfectText != null) 
                    parfectText.text = judgeDetails[0][3].ToString("D4");

                if (bounceText != null) 
                    bounceText.text = (judgeDetails[0][2] + judgeDetails[0][4]).ToString("D4");

                if (guardText != null) 
                    guardText.text = (judgeDetails[0][1] + judgeDetails[0][5]).ToString("D4");

                if (hitText != null) 
                    hitText.text = judgeDetails[0][0].ToString("D4");
            }
            else
            {
                Debug.LogError("ScoreManager or judgeDetails is null!");
            }
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
            if (star2 != null) star2.SetActive(true); // 2개 조건 아직 생성 안됨
            if (star3 != null) star3.SetActive(false); // 3개 조건 아직 설정 안됨
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
            if (star2 != null) star2.SetActive(true); // 2개 조건 아직 생성 안됨
            if (star3 != null) star3.SetActive(false); // 3개 조건 아직 설정 안됨
        }
    }
}
