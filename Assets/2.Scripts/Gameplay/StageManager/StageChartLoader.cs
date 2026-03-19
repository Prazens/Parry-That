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
    [SerializeField] private NotePerformer notePerformer;
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

        // 모든 차트 로드 후 NotePerformer 초기화
        if (notePerformer != null)
        {
            notePerformer.InitNotes(strikerManager.charts);
        }
    }

    public void LoadChartsFromStageData(StageData stageData)
    {
        if (strikerManager == null)
        {
            Debug.LogError("[StageChartLoader] strikerManager is not assigned.");
            return;
        }

        strikerManager.charts.Clear();

        if (stageData == null)
        {
            LoadChartsIntoStrikerManager();
            return;
        }

        IReadOnlyList<TextAsset> chartJsons = stageData.ChartJsons;
        if (chartJsons == null || chartJsons.Count == 0)
        {
            Debug.LogWarning("[StageChartLoader] stageData.ChartJsons is empty. Fallback to jsonCharts.");
            LoadChartsIntoStrikerManager();
            return;
        }

        if (TutorialManager.isTutorial)
        {
            void AddRange(int start, int endExclusive)
            {
                int safeStart = Mathf.Max(start, 0);
                int safeEnd = Mathf.Min(endExclusive, chartJsons.Count);

                for (int i = safeStart; i < safeEnd; i++)
                {
                    TextAsset chartJson = chartJsons[i];
                    if (chartJson == null) continue;
                    strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(chartJson));
                }
            }

            switch (TutorialManager.phase)
            {
                case 0: AddRange(0, 1); break;
                case 1: AddRange(1, 2); break;
                case 2: AddRange(2, 3); break;
                case 3: AddRange(3, 5); break;
                case 4: AddRange(5, 7); break;
                case 5: AddRange(7, 9); break;
                default: AddRange(0, chartJsons.Count); break;
            }

            return;
        }

        // 일반 스테이지는 전부 로드
        for (int chartIndex = 0; chartIndex < chartJsons.Count; chartIndex++)
        {
            TextAsset chartJson = chartJsons[chartIndex];
            if (chartJson == null) continue;

            strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(chartJson));
        }

        // 모든 차트 로드 후 NotePerformer 초기화
        if (notePerformer != null)
        {
            notePerformer.InitNotes(strikerManager.charts);
        }
    }
}
