using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager.StartStage()에서 하던 "차트 로딩 + strikerManager.charts에 전달"만 분리.
/// </summary>
public class StageChartLoader : MonoBehaviour
{
    [SerializeField] private NotePerformer notePerformer;
    [SerializeField] private JudgeSystem judgeSystem;

    public void LoadChartsFromStageData(StageData stageData)
    {
        TextAsset chartJson = GetCurrentPhaseChart(stageData);

        if (chartJson == null)
        {
            notePerformer.InitChart(null);
            judgeSystem.InitChart(null);
            return;
        }

        ChartData loadedChart = JsonReader.ReadJson<ChartData>(chartJson);

        notePerformer.InitChart(loadedChart);
        judgeSystem.InitChart(loadedChart);
    }

    public TextAsset GetCurrentPhaseChart(StageData stageData)
    {
        if (stageData == null) return null;

        var phases = stageData.Phases;
        if (phases == null || phases.Count == 0) return null;

        int index = StageFlowManager.Instance.currentPhaseIndex;
        if (index < 0 || index >= phases.Count) return null;

        StagePhase phase = phases[index];
        if (phase == null) return null;

        return phase.ChartJson;
    }

    public DialogueData GetCurrentPhaseDialogue(StageData stageData)
    {
        if (stageData == null) return null;

        var phases = stageData.Phases;
        if (phases == null || phases.Count == 0) return null;

        int index = StageFlowManager.Instance.currentPhaseIndex;
        if (index < 0 || index >= phases.Count) return null;

        StagePhase phase = phases[index];
        if (phase == null) return null;

        return phase.Dialogue;
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
        if (stageData == null) return 0f;

        int index = StageFlowManager.Instance.currentPhaseIndex;

        var phases = stageData.Phases;
        if (phases == null || index < 0 || index >= phases.Count)
            return 0f;

        StagePhase phase = phases[index];
        if (phase == null || phase.ChartJson == null)
            return 0f;

        ChartData chart = JsonReader.ReadJson<ChartData>(phase.ChartJson);
        if (chart == null || chart.strikers == null || chart.strikers.Length == 0)
            return 0f;

        float lastDisappearBeat = 0f;
        for (int i = 0; i < chart.strikers.Length; i++)
        {
            if (chart.strikers[i] != null && chart.strikers[i].disappearTime > lastDisappearBeat)
            {
                lastDisappearBeat = chart.strikers[i].disappearTime;
            }
        }

        return lastDisappearBeat * 60f / chart.bpm; // musicOffset
    }

    public DialogueAudioPolicy GetDialogueAudioPolicy(StageData stageData)
    {
        if (stageData == null) return DialogueAudioPolicy.KeepPlaying;

        var phases = stageData.Phases;
        if (phases == null || phases.Count == 0) return DialogueAudioPolicy.KeepPlaying;

        int index = StageFlowManager.Instance.currentPhaseIndex;
        if (index < 0 || index >= phases.Count) return DialogueAudioPolicy.KeepPlaying;

        StagePhase phase = phases[index];
        if (phase == null) return DialogueAudioPolicy.KeepPlaying;

        return phase.DialogueAudioPolicy;
    }
}