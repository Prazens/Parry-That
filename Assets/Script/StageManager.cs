using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance; // 전역 접근용 싱글턴
    public float currentTime { get; private set; } // 현재 스테이지 시간
    public float stageDuration = 60f; // 스테이지 전체 길이 (초)
    [SerializeField] private StrikerManager strikerManager;
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
        isActive = true;
        currentTime = 0f;
        strikerManager.SpawnStriker(0,0,10,120); // 위쪽에 체력10, bpm120인 striker 소환
        strikerManager.SpawnStriker(1,1,15,120); // 아래쪽에 체력 15, bpm120인 striker 소환환
    }

    // Update is called once per frame
    void Update()
    {
        // 현재 시간 증가
        currentTime += Time.deltaTime;

        // 스테이지 종료 시 처리
        if (currentTime >= stageDuration)
        {
            EndStage();
        }
    }
    private void EndStage()
    {
        Debug.Log("Stage Complete!");
        // 스테이지 종료 로직 추가
        currentTime = stageDuration; // 시간 고정
    }
    public void ResetStage()
    {
        currentTime = 0f; // 스테이지 시간 초기화
    }
}
