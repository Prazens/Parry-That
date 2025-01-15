using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameController gameController; // GameController 참조
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
    public void StartStage()
    {
        Debug.Log("Stage Started!");
        isActive = true; // 스테이지 활성화
        currentTime = 0f; // 시간 초기화
        SpawnPlayer();
        SpawnGuideboxes();
        strikerManager.SpawnStriker(0,0,10,107); // 위쪽에 체력10, bpm120인 striker 소환
        strikerManager.SpawnStriker(1,1,15,107); // 아래쪽에 체력 15, bpm120인 striker 소환환
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
        if (currentTime >= 1f && !musicPlayed)
        {
            SpawnTransparentProjectile();
            musicPlayed = true;
        }
    }
    private void EndStage()
    {
        Debug.Log("Stage Complete!");
        // 스테이지 종료 로직 추가
        currentTime = stageDuration; // 시간 고정
        isActive = false;
    }
    public void ResetStage()
    {
        currentTime = 0f; // 스테이지 시간 초기화
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
}
