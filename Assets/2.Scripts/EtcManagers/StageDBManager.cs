using UnityEngine;

public class StageDBManager : Singleton<StageDBManager>
{
    public const int MAX_DIFFS = 2;  // e.g., Normal, Hard
    public int stageNumbers;
    
    public string[] StageName;
    public int[] diffNumbers;  // Number of difficulties per stage
    public int[,] highScores;  // [stageIndex, difficultyIndex]
    public int[,] starRatings;  // [stageIndex, difficultyIndex]
    public bool[,] stageCompletion;  // [stageIndex, difficultyIndex]

    public bool isFirstLaunch = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // InitDatabase(10);
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
        highScores = new int[stageNumbers, MAX_DIFFS];
        starRatings = new int[stageNumbers, MAX_DIFFS];
        stageCompletion = new bool[stageNumbers, MAX_DIFFS];

        LoadStageData();
    }

    public void SaveStageData()
    {
        for (int stage = 0; stage < stageNumbers; stage++)
        {
            for (int diff = 0; diff < diffNumbers[stage]; diff++)
            {
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Score", highScores[stage, diff]);
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Star", starRatings[stage, diff]);
                PlayerPrefs.SetInt($"Stage_{stage}_{diff}_Completion", stageCompletion[stage, diff] ? 1 : 0);
            }
        }
    }

    public void LoadStageData()
    {
        if (PlayerPrefs.HasKey("Score1"))
        {
            for (int stage = 0; stage < stageNumbers; stage++)
            {
                for (int diff = 0; diff < diffNumbers[stage]; diff++)
                {
                    highScores[stage, diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Score", 0);
                    starRatings[stage, diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Star", 0);
                    stageCompletion[stage, diff] = PlayerPrefs.GetInt($"Stage_{stage}_{diff}_Completion", 0) == 1;
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
        stageNumbers = 6;
        StageName = new string[6] {
            "Tutorial",
            "1.The First Beat",
            "2.Echoing Strikes",
            "3.Beat Master",
            "4.Final Encore",
            "Epilogue"
        };
        diffNumbers = new int[6]
        {
            1,  // Tutorial
            2,  // Stage 1
            2,  // Stage 2
            2,  // Stage 3
            2,  // Stage 4
            1   // Epilogue
        };
        highScores = new int[6, MAX_DIFFS];
        starRatings = new int[6, MAX_DIFFS];
        stageCompletion = new bool[6, MAX_DIFFS];

        TestLoadStageData();
    }

    public void TestSaveStageData()
    {
        for (int stage = 0; stage < stageNumbers; stage++)
        {
            for (int diff = 0; diff < diffNumbers[stage]; diff++)
            {
                int oldIndex = SceneLinkage.ConvertToOldIndex(stage, diff);
                PlayerPrefs.SetInt($"Stage{oldIndex}", highScores[stage, diff]);
                PlayerPrefs.SetInt($"Stage{oldIndex}_Star", starRatings[stage, diff]);
                PlayerPrefs.SetInt($"Stage{oldIndex}Done", stageCompletion[stage, diff] ? 1 : 0);
            }
        }
    }

    public void TestLoadStageData()
    {
        if (PlayerPrefs.HasKey("Score1"))
        {
            for (int stage = 0; stage < stageNumbers; stage++)
            {
                for (int diff = 0; diff < diffNumbers[stage]; diff++)
                {
                    int oldIndex = SceneLinkage.ConvertToOldIndex(stage, diff);
                    highScores[stage, diff] = PlayerPrefs.GetInt($"Stage{oldIndex}", 0);
                    starRatings[stage, diff] = PlayerPrefs.GetInt($"Stage{oldIndex}_Star", 0);
                    stageCompletion[stage, diff] = PlayerPrefs.GetInt($"Stage{oldIndex}Done", 0) == 1;
                }
            }
        }
    }
}