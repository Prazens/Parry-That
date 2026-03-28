using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager.StartStage()에서 하던 "차트 로딩 + strikerManager.charts에 전달"만 분리.
/// - strikerManager.charts.Clear()
/// - 현재 phase의 chartJson이 있으면 JsonReader.ReadJson<ChartData>(TextAsset) 로드 후 Add
/// - 현재 phase의 dialogue가 있으면 다른 매니저가 꺼내 쓸 수 있도록 반환 가능
/// </summary>
public class StageChartLoader : MonoBehaviour
{
    [SerializeField] private StrikerManager strikerManager;

    public void LoadChartsFromStageData(StageData stageData)
    {
        strikerManager.charts.Clear();

        TextAsset chartJson = GetCurrentPhaseChart(stageData);
        if (chartJson == null) return;

        strikerManager.charts.Add(JsonReader.ReadJson<ChartData>(chartJson));
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

    //페이즈에 채보가 있는지 확인하는 함수
    public bool HasCurrentPhaseChart(StageData stageData)
    {
        return GetCurrentPhaseChart(stageData) != null;
    }

    //페이즈 끝난 뒤 대사가 있는지 확인하는 함수
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
        if (chart == null)
            return 0f;

        return chart.disappearTime * 60f / chart.bpm + 2.5f; //musicoffSet
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