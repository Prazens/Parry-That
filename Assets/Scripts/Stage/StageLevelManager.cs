using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLevelManager : MonoBehaviour
{
    [SerializeField] private List<StageData> stages = new List<StageData>();

    private Dictionary<(int stageId, Difficulty difficulty), StageData> stageCache;

    private void Awake()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        stageCache = new Dictionary<(int stageId, Difficulty difficulty), StageData>();

        for (int stageIndex = 0; stageIndex < stages.Count; stageIndex++)
        {
            StageData stageData = stages[stageIndex];
            if (stageData == null) continue;

            var key = (stageData.StageId, stageData.Difficulty);

            stageCache.Add(key, stageData);
        }
    }

    public StageData GetStageData(int stageId, Difficulty difficulty)
    {
        if (stageCache == null)
        {
            BuildCache();
        }

        if (stageCache.TryGetValue((stageId, difficulty), out StageData stageData))
        {
            return stageData;
        }

        Debug.LogError($"StageLevelManager: StageData를 찾지 못함 (stageId={stageId}, difficulty={difficulty}).");
        return null;
    }

    public bool ContainsStageData(int stageId, Difficulty difficulty)
    {
        if (stageCache == null)
        {
            BuildCache();
        }

        return stageCache.ContainsKey((stageId, difficulty));
    }
}
