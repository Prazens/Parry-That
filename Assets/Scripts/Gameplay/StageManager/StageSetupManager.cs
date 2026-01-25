using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSetupManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject guideboxTopPrefab;
    [SerializeField] private GameObject guideboxBottomPrefab;

    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private StageLevelManager stageLevelManager;
    [SerializeField] private StageFlowManager stageFlowManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private JudgeSystem judgeSystem;

    [Header("Tutorial Modules")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private DialogueManager dialogueManager;

    private GameObject playerObject;
    private GameObject guideboxTop;
    private GameObject guideboxBottom;

    private readonly List<GameObject> spawnedBossObjects = new List<GameObject>();

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[StageSetupManager] playerPrefab is not assigned.");
            return;
        }

        if (playerObject != null)
        {
            Destroy(playerObject);
            playerObject = null;
        }

        playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("[StageSetupManager] Spawned player does not have PlayerManager.");
            return;
        }

        playerManager.stageFlowManager = StageFlowManager.Instance;

        if (gameController != null)
        {
            TouchManager touchManager = gameController.GetComponent<TouchManager>();
            if (touchManager != null) touchManager.playerManager = playerManager;

            if (judgeSystem != null) judgeSystem.playerManager = playerManager;
        }
        else
        {
            Debug.LogWarning("[StageSetupManager] gameController is not assigned.");
        }

        if (strikerManager != null)
        {
            strikerManager.SetPlayer(playerManager);
        }
    }

    public void SpawnGuideboxes()
    {
        if (guideboxTopPrefab == null || guideboxBottomPrefab == null)
        {
            Debug.LogWarning("[StageSetupManager] Guidebox prefabs are not assigned.");
            return;
        }

        if (guideboxTop != null) Destroy(guideboxTop);
        if (guideboxBottom != null) Destroy(guideboxBottom);

        guideboxTop = Instantiate(guideboxTopPrefab, new Vector3(0f, 0.6f, 0f), Quaternion.identity);
        guideboxBottom = Instantiate(guideboxBottomPrefab, new Vector3(0f, -0.6f, 0f), Quaternion.identity);
    }

    public void ApplyStageModules()
    {
        if (stageLevelManager == null) stageLevelManager = FindObjectOfType<StageLevelManager>();
        if (stageFlowManager == null) stageFlowManager = FindObjectOfType<StageFlowManager>();
        if (strikerManager == null) strikerManager = FindObjectOfType<StrikerManager>();

        if (stageLevelManager == null || stageFlowManager == null || strikerManager == null)
        {
            Debug.LogError("[StageSetupManager] Core references missing.");
            return;
        }

        // -------------------------
        // 공통: 기존 스폰 보스 전부 정리
        // -------------------------
        for (int i = 0; i < spawnedBossObjects.Count; i++)
        {
            if (spawnedBossObjects[i] != null)
            {
                Destroy(spawnedBossObjects[i]);
            }
        }
        spawnedBossObjects.Clear();

        stageFlowManager.bossController = null;

        // 선택값 없으면 Normal 취급
        if (!StageSelection.HasValidSelection())
        {
            Debug.LogWarning("[StageSetupManager] StageSelection is not valid. Treating as Normal.");

            if (tutorialManager != null) tutorialManager.gameObject.SetActive(false);
            if (dialogueManager != null) dialogueManager.gameObject.SetActive(false);
            strikerManager.tutorialManager = null;

            return;
        }

        StageData stageData = stageLevelManager.GetStageData(
            StageSelection.SelectedStageId,
            StageSelection.SelectedDifficulty
        );

        if (stageData == null)
        {
            Debug.LogError("[StageSetupManager] StageData not found. Treating as Normal.");

            if (tutorialManager != null) tutorialManager.gameObject.SetActive(false);
            if (dialogueManager != null) dialogueManager.gameObject.SetActive(false);
            strikerManager.tutorialManager = null;

            return;
        }

        // =========================
        // Tutorial
        // =========================
        if (tutorialManager == null) tutorialManager = FindObjectOfType<TutorialManager>(true);
        if (dialogueManager == null) dialogueManager = FindObjectOfType<DialogueManager>(true);

        if (stageData.Category == StageCategory.Tutorial)
        {
            if (tutorialManager != null) tutorialManager.gameObject.SetActive(true);
            if (dialogueManager != null) dialogueManager.gameObject.SetActive(true);

            strikerManager.tutorialManager = tutorialManager;
        }
        else
        {
            if (tutorialManager != null) tutorialManager.gameObject.SetActive(false);
            if (dialogueManager != null) dialogueManager.gameObject.SetActive(false);

            strikerManager.tutorialManager = null;
        }

        // =========================
        // Boss (BossSpawnPrefabs 전부 스폰, 루트 생성)
        // =========================
        BossController resolvedBossController = null;

        if (stageData.Category == StageCategory.Boss)
        {
            if (stageData.BossSpawnPrefabs != null && stageData.BossSpawnPrefabs.Count > 0)
            {
                for (int i = 0; i < stageData.BossSpawnPrefabs.Count; i++)
                {
                    var entry = stageData.BossSpawnPrefabs[i];
                    if (entry == null || entry.prefab == null) continue;

                    // 부모 없음: 씬 루트에 생성
                    GameObject obj = Instantiate(entry.prefab);
                    spawnedBossObjects.Add(obj);

                    // 로컬이 아니라 월드 기준으로 적용 (부모가 없으니까)
                    Transform t = obj.transform;
                    t.position = entry.localPosition;
                    t.rotation = Quaternion.Euler(entry.localEulerAngles);
                    t.localScale = entry.localScale;

                    if (resolvedBossController == null)
                    {
                        resolvedBossController = obj.GetComponent<BossController>();
                    }
                }
            }

            if (resolvedBossController == null)
            {
                Debug.LogWarning("[StageSetupManager] Boss stage but no spawned prefab has BossController.");
            }
        }

        stageFlowManager.bossController = resolvedBossController;
    }
}
