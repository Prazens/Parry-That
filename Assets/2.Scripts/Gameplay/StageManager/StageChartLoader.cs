using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageData에서 현재 페이즈의 데이터(차트/대사/튜토리얼 패널/오디오 정책)를 읽는다.
/// </summary>
public class StageChartLoader : MonoBehaviour
{
    [SerializeField] private StrikerManager strikerManager;
    [SerializeField] private NotePerformer notePerformer;

    public void LoadChartsFromStageData(StageData stageData)
    {
        TextAsset chartJson = GetCurrentPhaseChart(stageData);

        if (chartJson == null)
        {
            strikerManager.charts = null;
            notePerformer.InitNotes(null);
            return;
        }

        ChartData loadedChart = JsonReader.ReadJson<ChartData>(chartJson);

        strikerManager.charts = loadedChart;
        notePerformer.InitNotes(loadedChart);
    }

    private StagePhase GetCurrentPhase(StageData stageData)
    {
        if (stageData == null)
            return null;

        IReadOnlyList<StagePhase> phases = stageData.Phases;
        if (phases == null || phases.Count == 0)
            return null;

        if (StageFlowManager.Instance == null)
            return null;

        int index = StageFlowManager.Instance.currentPhaseIndex;
        if (index < 0 || index >= phases.Count)
            return null;

        return phases[index];
    }

    public GameObject GetCurrentPhaseTutorialPanel(StageData stageData)
    {
        StagePhase phase = GetCurrentPhase(stageData);
        if (phase == null)
            return null;

        return phase.TutorialPanelPrefab;
    }

    public TextAsset GetCurrentPhaseChart(StageData stageData)
    {
        StagePhase phase = GetCurrentPhase(stageData);
        if (phase == null)
            return null;

        return phase.ChartJson;
    }

    public DialogueData GetCurrentPhaseDialogue(StageData stageData)
    {
        StagePhase phase = GetCurrentPhase(stageData);
        if (phase == null)
            return null;

        return phase.Dialogue;
    }

    public DialogueAudioPolicy GetDialogueAudioPolicy(StageData stageData)
    {
        StagePhase phase = GetCurrentPhase(stageData);
        if (phase == null)
            return DialogueAudioPolicy.KeepPlaying;

        return phase.DialogueAudioPolicy;
    }

    public bool HasCurrentPhaseTutorialPanel(StageData stageData)
    {
        return GetCurrentPhaseTutorialPanel(stageData) != null;
    }

    public bool HasCurrentPhaseChart(StageData stageData)
    {
        return GetCurrentPhaseChart(stageData) != null;
    }

    public bool HasCurrentPhaseDialogue(StageData stageData)
    {
        return GetCurrentPhaseDialogue(stageData) != null;
    }

    public float PhaseEndTime(StageData stageData)
    {
        TextAsset chartJson = GetCurrentPhaseChart(stageData);
        if (chartJson == null)
            return 0f;

        ChartData chart = JsonReader.ReadJson<ChartData>(chartJson);
        if (chart == null || chart.strikers == null || chart.strikers.Length == 0)
            return 0f;

        float lastDisappearBeat = 0f;

        for (int index = 0; index < chart.strikers.Length; index++)
        {
            if (chart.strikers[index] != null &&
                chart.strikers[index].disappearTime > lastDisappearBeat)
            {
                lastDisappearBeat = chart.strikers[index].disappearTime;
            }
        }

        return lastDisappearBeat * 60f / chart.bpm;
    }
}