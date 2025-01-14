using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager; // StageManager 연결
    // Start is called before the first frame update
    void Start()
    {
        if (stageManager != null)
        {
            stageManager.ResetStage(); // 스테이지 초기화
        }
        if (stageManager != null)
        {
           stageManager.StartStage(); // 스테이지 시작
        }
        else
        {
           Debug.LogError("StageManager is not assigned!");
        }
    }
    public void StartStage()
    {
        if (stageManager != null)
        {
            stageManager.StartStage(); // 스테이지 시작
        }
        else
        {
            Debug.LogError("StageManager is not assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
