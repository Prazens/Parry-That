using System.Collections;
using UnityEngine;

public enum CutSceneCategory
{
    Prologue = 0,
    Epilogue = 1,
}

public static class CutSceneSelection
{
    public static int SelectedStageId { get; private set; } = 0;
    public static CutSceneCategory SelectedCategory { get; private set; } = CutSceneCategory.Prologue;

    public static void SetSelection(int stageId, CutSceneCategory category)
    {
        SelectedStageId = stageId;
        SelectedCategory = category;
    }

    public static bool HasValidSelection()
    {
        return SelectedStageId >= 0;
    }

    public static void ResetSelection()
    {
        SelectedStageId = -1;
        SelectedCategory = CutSceneCategory.Prologue;
    }
}