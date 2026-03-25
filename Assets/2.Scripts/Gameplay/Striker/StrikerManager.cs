using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지에 있는 모든 Striker의 초기화, 등장 및 퇴장 등을 관리.
/// </summary>
public class StrikerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> strikerPrefabs;
    public Transform[] spawnPositions;

    private PlayerManager playerManager;

    private NotePerformer notePerformer;
    private JudgeSystem judgeSystem;
    private DynamicUIManager dynamicUIManager;

    public List<ChartData> charts;

    [SerializeField] public TutorialManager tutorialManager;

    public List<StrikerController> strikerList = new();
    private List<int> strikerStatus = new();

    public BossController bossController;

    public void SetReferences(PlayerManager playerManager, NotePerformer notePerformer, JudgeSystem judgeSystem, DynamicUIManager dynamicUIManager)
    {
        this.playerManager = playerManager;
        this.notePerformer = notePerformer;
        this.judgeSystem = judgeSystem;
        this.dynamicUIManager = dynamicUIManager;
    }

    private void Update()
    {
        if (StageFlowManager.Instance == null) return;
        if (playerManager == null) return;
        if (charts == null) return;
        if (strikerList == null || strikerStatus == null) return;

        int processCount = charts.Count;
        if (strikerList.Count < processCount) processCount = strikerList.Count;
        if (strikerStatus.Count < processCount) processCount = strikerStatus.Count;

        if (processCount <= 0) return;

        float currentTime = StageFlowManager.Instance.currentTime;

        for (int i = 0; i < processCount; i++)
        {
            StrikerController striker = strikerList[i];
            if (striker == null) continue;

            ChartData chart = charts[i];

            // 스트라이커 등장 및 퇴장
            float appearTimeSeconds = StageFlowManager.Instance.BeatToSec(chart.appearTime);
            float disappearTimeSeconds = StageFlowManager.Instance.BeatToSec(chart.disappearTime);
            if (currentTime >= appearTimeSeconds && strikerStatus[i] == 0)
            {
                strikerStatus[i] = 1;
                striker.gameObject.SetActive(true);
            }
            else if (currentTime >= disappearTimeSeconds && strikerStatus[i] == 1)
            {
                striker.OnClear();
                strikerStatus[i] = 2;
            }
        }
    }

    public void InitStriker(int idx)
    {
        if (StageFlowManager.Instance != null) {
            bossController = StageFlowManager.Instance.bossController;
        }
        else
            bossController = null;

        ClearStrikers();

        strikerStatus = new List<int>(new int[charts.Count]);
        strikerList   = new List<StrikerController>(new StrikerController[charts.Count]);

        for (int i = 0; i < charts.Count; i++)
        {
            bool activated = charts[i].appearTime == 0;
            if (activated) strikerStatus[i] = 1;

            SpawnStriker(i, activated);
        }
    }

    private void SpawnStriker(int chartIndex, bool isActivated)
    {
        if (chartIndex < 0 || chartIndex >= charts.Count) return;
        if (spawnPositions == null || spawnPositions.Length == 0) return;
        
        int hp = charts[chartIndex].notes.Length;
        float bpm = charts[chartIndex].bpm;

        int positionIndex = charts[chartIndex].direction - 1;
        int prefabIndex = charts[chartIndex].strikerType;

        if (positionIndex < 0 || positionIndex >= spawnPositions.Length) return;
        if (prefabIndex < 0 || prefabIndex >= strikerPrefabs.Count) return;

        GameObject selectedStriker = strikerPrefabs[prefabIndex];
        GameObject strikerInstance = Instantiate(selectedStriker, spawnPositions[positionIndex].position, Quaternion.identity);
        if (strikerInstance == null) return;
        StrikerController striker = strikerInstance.GetComponent<StrikerController>();

        strikerList[chartIndex] = striker;

        if (striker != null)
        {
            striker.Visual.dynamicUIManager = dynamicUIManager;
            striker.manager = this;
            striker.judgeSystem = judgeSystem;

            if (bossController != null)
            {
                bossController.RegisterStriker(striker);
            }

            striker.Initialize(playerManager, (Direction)(positionIndex + 1));
        }

        if (!isActivated)
        {
            striker.gameObject.SetActive(false);
        }
    }

    public void ClearStrikers()
    {
        foreach (StrikerController striker in strikerList)
        {
            if (striker != null)
            {
                striker.ClearProjectiles();
                Destroy(striker);
            }
        }

        strikerList.Clear();

        var remains = FindObjectsOfType<StrikerController>();
        foreach (var remain in remains)
        {
            Destroy(remain.gameObject);
        }
    }
}
