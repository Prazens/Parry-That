using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> strikerPrefabs;
    public Transform[] spawnPositions;

    private PlayerManager playerManager;

    [SerializeField] private DynamicUIManager dynamicUIManager;

    public List<ChartData> charts;

    [SerializeField] public TutorialManager tutorialManager;

    [SerializeField] private GameObject holdExclamationPrefab;
    private GameObject holdExclamation;
    [SerializeField] private AudioClip holdingSound;

    public List<GameObject> strikerList = new List<GameObject>();
    public List<int> strikerStatus = new List<int>();

    public BossController bossController;

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
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
            GameObject striker = strikerList[i];
            if (striker == null) continue;

            ChartData chart = charts[i];

            float appearTimeSeconds = chart.appearTime * (60f / chart.bpm) + playerManager.musicOffset;
            float disappearTimeSeconds = chart.disappearTime * (60f / chart.bpm) + playerManager.musicOffset;

            if (currentTime >= appearTimeSeconds && strikerStatus[i] == 0)
            {
                strikerStatus[i] = 1;
                striker.SetActive(true);
            }
            else if (currentTime >= disappearTimeSeconds && strikerStatus[i] == 1)
            {
                striker.GetComponent<StrikerController>().strikerExit();
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

        if (holdExclamation != null) Destroy(holdExclamation);
        holdExclamation = Instantiate(holdExclamationPrefab);

        var audioSourceObject = GameObject.Find("Audio Source");
        if (audioSourceObject != null)
            holdExclamation.GetComponent<holdExclamation>().audioSource = audioSourceObject.GetComponent<AudioSource>();

        strikerStatus = new List<int>(new int[charts.Count]);
        strikerList   = new List<GameObject>(new GameObject[charts.Count]);

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
        GameObject striker = Instantiate(selectedStriker, spawnPositions[positionIndex].position, Quaternion.identity);

        strikerList[chartIndex] = striker;

        StrikerController strikerController = striker.GetComponent<StrikerController>();
        if (strikerController != null)
        {
            strikerController.dynamicUIManager = dynamicUIManager;
            strikerController.holdExclamation = holdExclamation;
            strikerController.Sound.SetHoldingSound(holdingSound);

            if (bossController != null)
            {
                bossController.RegisterStriker(strikerController);
            }

            int isMelee = 0;
            if (prefabIndex == 1) isMelee = 1;

            strikerController.Initialize(
                hp,
                bpm,
                playerManager,
                (Direction)(positionIndex + 1),
                charts[chartIndex],
                isMelee
            );
        }

        if (!isActivated)
        {
            striker.SetActive(false);
        }
    }

    public void ClearStrikers()
    {
        foreach (GameObject striker in strikerList)
        {
            if (striker != null)
            {
                StrikerController strikerController = striker.GetComponent<StrikerController>();
                if (strikerController != null && !strikerController.isMelee)
                {
                    strikerController.ClearProjectiles();
                }
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
