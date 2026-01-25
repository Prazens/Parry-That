using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager.StartStage()에서 하던 "차트 로딩 + strikerManager.charts에 전달"만 분리.
/// - strikerManager.charts.Clear()
/// - TutorialManager.isTutorial / TutorialManager.phase 분기
/// - JsonReader.ReadJson<ChartData>(TextAsset) 로드 후 Add
/// 
/// 원래 StageManager에 없던 기능/함수는 최대한 추가하지 않음.
/// </summary>
public class StageChartLoader : MonoBehaviour
{
    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private TextAsset[] jsonCharts;

    public void LoadChartsIntoStrikerManager()
    {
        if (strikerManager == null)
        {
            Debug.LogError("[StageChartLoader] strikerManager is not assigned.");
            return;
        }

        strikerManager.charts.Clear();

        if (jsonCharts == null || jsonCharts.Length == 0)
        {
            Debug.LogWarning("[StageChartLoader] jsonCharts is empty.");
            return;
        }

        if (TutorialManager.isTutorial)
        {
            switch (TutorialManager.phase)
            {
                case 0:
                    for (int i = 0; i < 1; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;

                case 1:
                    for (int i = 1; i < 2; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;

                case 2:
                    for (int i = 2; i < 3; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;

                case 3:
                    for (int i = 3; i < 5; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;

                case 4:
                    for (int i = 5; i < 7; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;

                case 5:
                    for (int i = 7; i < 9; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;
                    

                default:
                    for (int i = 0; i < jsonCharts.Length; i++)
                    {
                        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
                    }
                    break;
            }
        }
        else
        {
            for (int i = 0; i < jsonCharts.Length; i++)
            {
                strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(jsonCharts[i]));
            }
        }
    }
}
