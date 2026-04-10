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
    [SerializeField] private NotePerformer notePerformer;

    [Header("CutIn UI")]
    [SerializeField] private DynamicUIManager dynamicUIManager;
    [SerializeField] private RectTransform cutInParent; // 반드시 Canvas 아래 컨테이너
    private GameObject spawnedCutInUp;
    private GameObject spawnedCutInDown;

    [Header("Tutorial Modules")]
    [SerializeField] private TutorialManager tutorialManager;
    
    private GameObject playerObject;
    private GameObject guideboxTop;
    private GameObject guideboxBottom;

    private readonly List<GameObject> spawnedBackgroundObjects = new List<GameObject>();

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
            strikerManager.SetReferences(playerManager, dynamicUIManager);
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

    public void SetCutIn(StageData stageData)
    {
        if (dynamicUIManager == null) dynamicUIManager = FindObjectOfType<DynamicUIManager>(true);

        // 기존 컷인 정리
        if (spawnedCutInUp != null) Destroy(spawnedCutInUp);
        if (spawnedCutInDown != null) Destroy(spawnedCutInDown);
        spawnedCutInUp = null;
        spawnedCutInDown = null;

        // StageData에 저장된 프리팹
        GameObject upPrefab = stageData.CutInUpPrefab;
        GameObject downPrefab = stageData.CutInDownPrefab;

        if (upPrefab != null)
            spawnedCutInUp = Instantiate(upPrefab, cutInParent, false);

        if (downPrefab != null)
            spawnedCutInDown = Instantiate(downPrefab, cutInParent, false);

        dynamicUIManager.SetCutInObjects(spawnedCutInUp, spawnedCutInDown);
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

        // 기존 배경 프리팹 정리
        for (int i = 0; i < spawnedBackgroundObjects.Count; i++)
        {
            if (spawnedBackgroundObjects[i] != null)
            {
                Destroy(spawnedBackgroundObjects[i]);
            }
        }
        spawnedBackgroundObjects.Clear();

        // 선택값 없으면 Normal 취급
        if (!StageSelection.HasValidSelection())
        {
            Debug.LogWarning("[StageSetupManager] StageSelection is not valid. Treating as Normal.");

            if (tutorialManager != null) tutorialManager.gameObject.SetActive(false);
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
            strikerManager.tutorialManager = null;

            return;
        }

        // =========================
        // Background Prefabs
        // =========================
        if (stageData.SpawnPrefabs != null && stageData.SpawnPrefabs.Count > 0)
        {
            for (int i = 0; i < stageData.SpawnPrefabs.Count; i++)
            {
                BackgroundPrefabs entry = stageData.SpawnPrefabs[i];
                if (entry == null || entry.prefab == null) continue;

                Transform parent = null;

                if (!string.IsNullOrEmpty(entry.attachPointName))
                {
                    GameObject attachObject = GameObject.Find(entry.attachPointName);
                    if (attachObject != null)
                    {
                        parent = attachObject.transform;
                    }
                }

                GameObject spawnedObject;
                var chartData = JsonUtility.FromJson<ChartData>(stageData.Phases[0].ChartJson.text);
                float bpm = chartData.bpm;

                if (parent != null)
                {
                    spawnedObject = Instantiate(entry.prefab, parent);
                    spawnedObject.transform.localPosition = entry.localPosition;
                    spawnedObject.transform.localRotation = Quaternion.Euler(entry.localEulerAngles);
                    spawnedObject.transform.localScale = entry.localScale;
                    spawnedObject.GetComponent<Animator>().speed = bpm / 60f;
                }
                else
                {
                    spawnedObject = Instantiate(entry.prefab);
                    spawnedObject.transform.position = entry.localPosition;
                    spawnedObject.transform.rotation = Quaternion.Euler(entry.localEulerAngles);
                    spawnedObject.transform.localScale = entry.localScale;
                    spawnedObject.GetComponent<Animator>().speed = bpm / 60f;
                }

                spawnedBackgroundObjects.Add(spawnedObject);
            }
        }

        // =========================
        // Tutorial
        // =========================
        if (tutorialManager == null) tutorialManager = FindObjectOfType<TutorialManager>(true);

        if (stageData.Category == StageCategory.Tutorial)
        {
            if (tutorialManager != null) tutorialManager.gameObject.SetActive(true);

            strikerManager.tutorialManager = tutorialManager;
        }
        else
        {
            if (tutorialManager != null) tutorialManager.gameObject.SetActive(false);

            strikerManager.tutorialManager = null;
        }

        // =========================
        // Boss (시작 시 생성)
        // =========================
        if (stageData.Category == StageCategory.Boss)
        {
            strikerManager.isBossStage = true;
            strikerManager.AppearStriker(stageData.BossStrikerType);
        }
    }
}