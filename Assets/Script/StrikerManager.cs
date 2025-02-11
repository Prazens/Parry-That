using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> strikerPrefabs;
    public Transform[] spawnPositions;
    private PlayerManager playerManager; // Player 정보 저장
    [SerializeField] private UIManager uiManager;
    public List<ChartData> charts; // 각 스트라이커의 채보 데이터
    [SerializeField] private TutorialManager tutorialManager;

    // 스트라이커 저장해놓을 공간
    public List<GameObject> strikerList = new List<GameObject>();
    public List<int> strikerStatus = new List<int>();

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        Debug.Log("PlayerManager successfully linked to StrikerManager.");
    }

    //private void Start()
    //{
    //    if (TutorialManager.isTutorial)
    //    {
    //        tutorialManager = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();
    //        if (tutorialManager = null) Debug.LogError("튜토리얼 매니저 못찾음");
    //    }

    //}
    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        if (TutorialManager.isTutorial && !tutorialManager.isDaehwa)
        {
            for (int i = 0; i < TutorialManager.StrikerNum[tutorialManager.daehwaIndex] - TutorialManager.StrikerNum[tutorialManager.daehwaIndex - 1]; i++)
            {
                if (currentTime >= charts[i].appearTime * (60f / charts[i].bpm) + playerManager.musicOffset &&
                    strikerStatus[i] == 0)
                {
                    strikerStatus[i] = 1;
                    Debug.Log($"SpawnStriker({i})");
                    strikerList[i].SetActive(true);
                }
                else if (currentTime >= charts[i].disappearTime * (60f / charts[i].bpm) + playerManager.musicOffset &&
                         strikerStatus[i] == 1)
                {
                    Debug.Log($"beClearedStriker({i})");
                    strikerList[i].GetComponent<StrikerController>().beCleared();
                    strikerStatus[i] = 2;
                }
            }
        }
        else if (!TutorialManager.isTutorial)
        {
            for (int i = 0; i < charts.Count; i++)
            {
                if (currentTime >= charts[i].appearTime * (60f / charts[i].bpm) + playerManager.musicOffset &&
                    strikerStatus[i] == 0)
                {
                    strikerStatus[i] = 1;
                    Debug.Log($"SpawnStriker({i})");
                    strikerList[i].SetActive(true);
                }
                else if (currentTime >= charts[i].disappearTime * (60f / charts[i].bpm) + playerManager.musicOffset &&
                         strikerStatus[i] == 1)
                {
                    Debug.Log($"beClearedStriker({i})");
                    strikerList[i].GetComponent<StrikerController>().beCleared();
                    strikerStatus[i] = 2;
                }
            }
        }

    }

    public void InitStriker(int idx)
    {
        if (TutorialManager.isTutorial)
        {
            int[] StrikerNum = { 0, 1, 2, 3, 7, 11, 13 };    // 각 패턴에 나오는 스크라이커 시작 인덱스 // 총 12개
            strikerStatus.Clear();
            Debug.Log($"InitStriker {charts.Count}");
            // 차트 패턴 나오는 순서 맞춰서 넣어야함
            for (int i = StrikerNum[idx]; i < StrikerNum[idx + 1]; i++)
            {
                strikerStatus.Add(0);
                if (charts[i].appearTime == 0)
                {
                    strikerStatus[i] = 1;
                    Debug.Log($"SpawnStriker({i})");
                    SpawnStriker(i, true);
                }
                else
                {
                    SpawnStriker(i, false);
                }
            }
        }
        else
        {
            strikerStatus.Clear();
            Debug.Log($"InitStriker {charts.Count}");
            for (int i = 0; i < charts.Count; i++)
            {
                strikerStatus.Add(0);
                if (charts[i].appearTime == 0)
                {
                    strikerStatus[i] = 1;
                    Debug.Log($"SpawnStriker({i})");
                    SpawnStriker(i, true);
                }
                else
                {
                    SpawnStriker(i, false);
                }
            }
        }
    }

    // Start is called before the first frame update
    private void SpawnStriker(int chartIndex, bool isActivated) // striker를 원하는 위치에 spawn, 현재 위쪽과 아래 쪽 두곳으로 spawnpoint 지정해놓음
    {
        int hp = charts[chartIndex].notes.Length;
        float bpm = charts[chartIndex].bpm;
        int positionIndex = charts[chartIndex].direction - 1;
        int prepabindex = charts[chartIndex].strikerType;

        // 소환 위치 유효성 검사
        if (positionIndex < 0 || positionIndex >= spawnPositions.Length)
        {
            Debug.LogError("Invalid spawn position index!");
            return;
        }
        if (chartIndex < 0 || chartIndex >= charts.Count) return;
        GameObject selectedStriker = strikerPrefabs[prepabindex];
        // 스트라이커 생성
        GameObject striker = Instantiate(selectedStriker, spawnPositions[positionIndex].position, Quaternion.identity);

        // 스트라이커 저장
        strikerList.Add(striker);

        // 스트라이커 초기화
        StrikerController strikerController = striker.GetComponent<StrikerController>();
        strikerController.uiManager = uiManager;
        
        if (strikerController != null)
        {
            strikerController.Initialize(hp, bpm, playerManager, (Direction)(positionIndex + 1), charts[chartIndex], prepabindex);
        }
        else
        {
            Debug.LogError("Striker prefab is missing StrikerController!");
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
                // Striker가 소유한 Projectile 제거
                StrikerController strikerController = striker.GetComponent<StrikerController>();
                if (strikerController != null && !strikerController.isMelee)
                {
                    strikerController.ClearProjectiles();
                }
                Destroy(striker); // Striker GameObject 삭제
            }
        }

        strikerList.Clear(); // 리스트 초기화
    }
}
