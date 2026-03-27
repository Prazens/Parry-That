using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager.StartStage()에서 하던 "차트 로딩 + strikerManager.charts에 전달"만 분리.
/// - strikerManager.charts.Clear()
/// - TutorialManager.isTutorial / TutorialManager.phase 분기
/// - JsonReader.ReadJson<ChartData>(TextAsset) 로드 후 Add
///
/// </summary>
public class StageChartLoader : MonoBehaviour
{
    [SerializeField] private StrikerManager strikerManager;

    public void LoadChartsFromStageData(StageData stageData)
    {
        strikerManager.charts.Clear();

        if (stageData == null) return;

        var chartJsons = stageData.ChartJsons;
        if (chartJsons == null || chartJsons.Count == 0) return;

        int index = StageFlowManager.Instance.currentPhaseIndex;

        if (index >= 0 && index < chartJsons.Count)
        {
            var chartJson = chartJsons[index];
            if (chartJson != null)
            {
                strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(chartJson));
            }
        }
    }
}
