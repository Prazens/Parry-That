using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 직렬화를 위한 Wrapper 클래스들

[System.Serializable]
public class IntArr
{
    public int[] v;

    public int this[int index]
    {
        get => v[index];
        set => v[index] = value;
    }
}

[System.Serializable]
public class BoolArr
{
    public bool[] v;

    public bool this[int index]
    {
        get => v[index];
        set => v[index] = value;
    }
}

public class StageDBManager : Singleton<StageDBManager>
{
    public const int MAX_DIFFS = 2;  // e.g., Normal, Hard
    public int stageNumbers;
    
    public string[] StageName;
    public int[] diffNumbers;  // Number of difficulties per stage
    public List<IntArr> highScores;  // [stageIndex][difficultyIndex]
    public List<IntArr> starRatings;  // [stageIndex][difficultyIndex]
    public List<BoolArr> stageCompletion;  // [stageIndex][difficultyIndex], 스테이지 해금에도 이 값이 사용됨

    public bool isFirstLaunch = true;

    protected override void OnAwake()
    {
        Debug.Log("StageDBManager Awake");

        DontDestroyOnLoad(gameObject);

        // InitDatabase(10);
        // 나중에 JSON이나 ScriptableObject로 관리하는 방안 고려
        TestInitDatabase();
    }

    // // 현재 선택된 스테이지와 난이도 인덱스
    // // MenuManager와 연동됨 -> StageSelection으로 기능 이전 확인, 안씀
    // private int[] currentStage = new int[2] { 1, 0 };  // [stageIndex, difficultyIndex]

    // private int[] CurrentStage
    // {
    //     get => currentStage;
    //     set
    //     {
    //         currentStage[0] = value[0];
    //         currentStage[1] = Mathf.Min(value[1], diffNumbers[value[0]] - 1);
    //     }
    // }

    public void InitDatabase(int totalStages)
    {
        stageNumbers = totalStages;
        StageName = new string[stageNumbers];
        diffNumbers = new int[stageNumbers];
        highScores = new List<IntArr>();
        starRatings = new List<IntArr>();
        stageCompletion = new List<BoolArr>();

        for (int i = 0; i < stageNumbers; i++)
        {
            highScores.Add(new IntArr { v = new int[MAX_DIFFS] });
            starRatings.Add(new IntArr { v = new int[MAX_DIFFS] });
            stageCompletion.Add(new BoolArr { v = new bool[MAX_DIFFS] });
        }

        LoadStageData();
    }

    public void SaveStageData()
    {
        for (int stage = 0; stage < stageNumbers; stage++)
        {
            for (int diff = 0; diff < diffNumbers[stage]; diff++)
            {
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Score", highScores[stage][diff]);
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Star", starRatings[stage][diff]);
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Completion", stageCompletion[stage][diff] ? 1 : 0);
            }
        }
        PlayerPrefs.Save();
    }

    public void LoadStageData()
    {
        if (PlayerPrefs.HasKey("Score1"))
        {
            for (int stage = 0; stage < stageNumbers; stage++)
            {
                for (int diff = 0; diff < diffNumbers[stage]; diff++)
                {
                    highScores[stage][diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Score", 0);
                    starRatings[stage][diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Star", 0);
                    stageCompletion[stage][diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Completion", 0) == 1;
                }
            }
        }
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
        InitDatabase(stageNumbers);
    }

    public void TestInitDatabase()
    {
        // 스테이지 이름과 난이도 수를 하드코딩하여 초기화
        // JSON이나 ScriptableObject로 관리하는 방안 고려
        stageNumbers = 8;
        StageName = new string[8] {
            "Tutorial",
            "1.The First Beat",
            "2.Echoing Strikes",
            "3.Beat Master",
            "4.Final Encore",
            "5.Schizophrenia",
            "6.Hallucination",
            "7.Neurosis"

        };
        diffNumbers = new int[8]
        {
            1,  // Tutorial
            2,  // Stage 1
            2,  // Stage 2
            2,  // Stage 3
            2,  // Stage 4
            2,  // Stage 5
            2,  // Stage 6
            2   // Stage 7
        };
        highScores = new List<IntArr>();
        starRatings = new List<IntArr>();
        stageCompletion = new List<BoolArr>();

        for (int i = 0; i < stageNumbers; i++)
        {
            highScores.Add(new IntArr { v = new int[MAX_DIFFS] });
            starRatings.Add(new IntArr { v = new int[MAX_DIFFS] });
            stageCompletion.Add(new BoolArr { v = new bool[MAX_DIFFS] });
        }

        stageCompletion[0][0] = true;  // 튜토리얼은 기본적으로 클리어된 상태로 시작
        highScores[0][0] = 1;

        LoadStageData();
    }

    // public void TestSaveStageData()
    // {
    //     for (int stage = 0; stage < stageNumbers; stage++)
    //     {
    //         for (int diff = 0; diff < diffNumbers[stage]; diff++)
    //         {
    //             int oldIndex = SceneLinkage.ConvertToOldIndex(stage, diff);
    //             PlayerPrefs.SetInt($"Stage{oldIndex}", highScores[stage][diff]);
    //             PlayerPrefs.SetInt($"Stage{oldIndex}_Star", starRatings[stage][diff]);
    //             PlayerPrefs.SetInt($"Stage{oldIndex}Done", stageCompletion[stage][diff] ? 1 : 0);
    //         }
    //     }
    // }

    // public void TestLoadStageData()
    // {
    //     if (PlayerPrefs.HasKey("Score1"))
    //     {
    //         for (int stage = 0; stage < stageNumbers; stage++)
    //         {
    //             for (int diff = 0; diff < diffNumbers[stage]; diff++)
    //             {
    //                 int oldIndex = SceneLinkage.ConvertToOldIndex(stage, diff);
    //                 highScores[stage][diff] = PlayerPrefs.GetInt($"Stage{oldIndex}", 0);
    //                 starRatings[stage][diff] = PlayerPrefs.GetInt($"Stage{oldIndex}_Star", 0);
    //                 stageCompletion[stage][diff] = PlayerPrefs.GetInt($"Stage{oldIndex}Done", 0) == 1;
    //             }
    //         }
    //     }
    // }

    [ContextMenu("Clear Data")]
    public void DebugClearData()
    {
        ClearData();
    }

    [ContextMenu("Save Data")]
    public void DebugSaveData()
    {
        SaveStageData();
    }

    [ContextMenu("Load Data")]
    public void DebugLoadData()
    {
        LoadStageData();
    }

    [ContextMenu("Refresh Disk Swipe UI")]
    public void DebugRefreshDiskSwipeUI()
    {
        MenuManager.Instance.diskSwipeUI.UpdateDifficulty((int)StageSelection.SelectedDifficulty);
    }

    [ContextMenu("All Unlock & Refresh Disk Swipe UI")]
    public void DebugAllUnlock()
    {
        for (int stage = 0; stage < stageNumbers; stage++)
        {
            for (int diff = 0; diff < diffNumbers[stage]; diff++)
            {
                stageCompletion[stage][diff] = true;
            }
        }
        
        MenuManager.Instance.diskSwipeUI.UpdateDifficulty((int)StageSelection.SelectedDifficulty);
    }
}