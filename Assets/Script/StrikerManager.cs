using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StrikerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> strikerPrefabs;
    public Transform[] spawnPositions;
    private PlayerManager playerManager; // Player 정보 저장
    [SerializeField] private UIManager uiManager;
    public List<ChartData> charts; // 각 스트라이커의 채보 데이터
    public int disappearedStrikerNum = 0;

    // 스트라이커 저장해놓을 공간
    public List<GameObject> strikerList = new List<GameObject>();

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        Debug.Log("PlayerManager successfully linked to StrikerManager.");
    }

    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        for (int i = strikerList.Count; i < charts.Count; i++)
        {
            if (currentTime >= charts[i].appearTime * (60f / charts[i].bpm) + playerManager.musicOffset)
            {
                SpawnStriker(i);
            }
        }

        for (int i = disappearedStrikerNum; i < strikerList.Count; i++)
        {
            if (currentTime >= charts[i].disappearTime * (60f / charts[i].bpm) + playerManager.musicOffset)
            {
                // disappear
                disappearedStrikerNum++;
            }
        }
    }

    public void InitStriker()
    {
        for (int i = 0; i < charts.Count; i++)
        {
            if (charts[i].disappearTime == 0)
            {
                SpawnStriker(i);
            }
        }
    }

    // Start is called before the first frame update
    public void SpawnStriker(int chartIndex) // striker를 원하는 위치에 spawn, 현재 위쪽과 아래 쪽 두곳으로 spawnpoint 지정해놓음
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

        disappearedStrikerNum = 0;
        strikerList.Clear(); // 리스트 초기화
    }
}
