using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager의 SpawnPlayer / SpawnGuideboxes 역할만 분리.
/// - 프리팹 생성(플레이어, 가이드박스)
/// - 생성 후 기존처럼 레퍼런스 와이어링
/// 
/// 원래 스크립트에 없던 기능(추가 편의 함수 등)은 최대한 넣지 않음.
/// </summary>
public class StageSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject guideboxTopPrefab;
    [SerializeField] private GameObject guideboxBottomPrefab;

    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private JudgeSystem judgeSystem;

    private GameObject playerObject;
    private GameObject guideboxTop;
    private GameObject guideboxBottom;

    // 기존 StageManager.SpawnPlayer()를 그대로 이동
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[StageSpawnManager] playerPrefab is not assigned.");
            return;
        }

        if (playerObject != null)
        {
            Destroy(playerObject);
            playerObject = null;
        }

        playerObject = Instantiate(playerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);

        PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("[StageSpawnManager] Spawned player does not have PlayerManager.");
            return;
        }

        // 기존: playerManager.stageManager = this(StageManager)
        // 변경: StageFlowManager를 참조하도록 리팩토링 했으니 stageFlowManager로 연결
        playerManager.stageFlowManager = StageFlowManager.Instance;

        // 기존 StageManager: GameController에서 TouchManager/judgeSystem 찾아 playerManager 연결 :contentReference[oaicite:0]{index=0}
        if (gameController != null)
        {
            TouchManager touchManager = gameController.GetComponent<TouchManager>();
            if (touchManager != null)
            {
                touchManager.playerManager = playerManager;
            }
            if (judgeSystem != null)
            {
                judgeSystem.playerManager = playerManager;
            }
        }
        else
        {
            Debug.LogWarning("[StageSpawnManager] gameController is not assigned.");
        }

        // 기존 StageManager: strikerManager.SetPlayer(playerManager) :contentReference[oaicite:1]{index=1}
        if (strikerManager != null)
        {
            strikerManager.SetPlayer(playerManager);
        }
    }

    // 기존 StageManager.SpawnGuideboxes()를 그대로 이동
    public void SpawnGuideboxes()
    {
        if (guideboxTopPrefab == null || guideboxBottomPrefab == null)
        {
            Debug.LogWarning("[StageSpawnManager] Guidebox prefabs are not assigned.");
            return;
        }

        if (guideboxTop != null)
        {
            Destroy(guideboxTop);
            guideboxTop = null;
        }

        if (guideboxBottom != null)
        {
            Destroy(guideboxBottom);
            guideboxBottom = null;
        }

        // 기존 StageManager에서 사용하던 위치 그대로 :contentReference[oaicite:2]{index=2}
        guideboxTop = Instantiate(guideboxTopPrefab, new Vector3(0f, 0.6f, 0f), Quaternion.identity);
        guideboxBottom = Instantiate(guideboxBottomPrefab, new Vector3(0f, -0.6f, 0f), Quaternion.identity);
    }
}
