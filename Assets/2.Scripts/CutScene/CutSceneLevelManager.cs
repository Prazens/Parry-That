using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneLevelManager : MonoBehaviour
{
    [SerializeField] private List<CutSceneData> cutScenes = new List<CutSceneData>();

    private Dictionary<(int stageId, CutSceneCategory category), CutSceneData> cutSceneCache;

    private void Awake()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        cutSceneCache = new Dictionary<(int stageId, CutSceneCategory category), CutSceneData>();

        for (int cutSceneIndex = 0; cutSceneIndex < cutScenes.Count; cutSceneIndex++)
        {
            CutSceneData cutSceneData = cutScenes[cutSceneIndex];
            if (cutSceneData == null)
            {
                continue;
            }

            var key = (cutSceneData.StageId, cutSceneData.Category);

            if (cutSceneCache.ContainsKey(key))
            {
                Debug.LogWarning($"CutSceneLevelManager: 중복 키 발견 (stageId={cutSceneData.StageId}, category={cutSceneData.Category})");
                continue;
            }

            cutSceneCache.Add(key, cutSceneData);
        }
    }

    public CutSceneData GetCutSceneData(int stageId, CutSceneCategory category)
    {
        if (cutSceneCache == null)
        {
            BuildCache();
        }

        if (cutSceneCache.TryGetValue((stageId, category), out CutSceneData cutSceneData))
        {
            return cutSceneData;
        }

        Debug.LogError($"CutSceneLevelManager: CutSceneData를 찾지 못함 (stageId={stageId}, category={category})");
        return null;
    }

    public bool ContainsCutSceneData(int stageId, CutSceneCategory category)
    {
        if (cutSceneCache == null)
        {
            BuildCache();
        }

        return cutSceneCache.ContainsKey((stageId, category));
    }
}