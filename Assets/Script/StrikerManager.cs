using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerManager : MonoBehaviour
{
    [SerializeField] private GameObject strikerPrefab;
    public Transform[] spawnPositions;
    private PlayerManager playerManager; // Player 정보 저장
    [SerializeField] private UIManager uiManager;
    public List<ChartData> charts; // 각 스트라이커의 채보 데이터

    // 스트라이커 저장해놓을 공간
    public List<GameObject> strikerList = new List<GameObject>();
    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        Debug.Log("PlayerManager successfully linked to StrikerManager.");
    }

    // Start is called before the first frame update
    public void SpawnStriker(int positionIndex, int chartIndex, int hp = 10, int bpm = 120) // striker를 원하는 위치에 spawn, 현재 위쪽과 아래 쪽 두곳으로 spawnpoint 지정해놓음
    {
        // 소환 위치 유효성 검사
        if (positionIndex < 0 || positionIndex >= spawnPositions.Length)
        {
            Debug.LogError("Invalid spawn position index!");
            return;
        }
        if (chartIndex < 0 || chartIndex >= charts.Count) return;

        // 스트라이커 생성
        GameObject striker = Instantiate(strikerPrefab, spawnPositions[positionIndex].position, Quaternion.identity);

        // 스트라이커 저장
        strikerList.Add(striker);

        // 스트라이커 초기화
        StrikerController strikerController = striker.GetComponent<StrikerController>();
        strikerController.uiManager = uiManager;
        
        if (strikerController != null)
        {
            strikerController.Initialize(hp, bpm, playerManager, (Direction)(positionIndex + 1),charts[chartIndex]);
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
                if (strikerController != null)
                {
                    strikerController.ClearProjectiles();
                }
                Destroy(striker); // Striker GameObject 삭제
            }
        }

        strikerList.Clear(); // 리스트 초기화
    }
}
