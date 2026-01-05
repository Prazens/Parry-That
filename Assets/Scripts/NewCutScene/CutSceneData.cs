using System;
using System.Collections.Generic;
using UnityEngine;

public enum CutSceneActionType
{
    ShowPanel,
    HidePanel,
    FadeIn,
    FadeOut,
    ShowTextAppend,
    ShowTextReset,
    TriggerAnimator
}

[Serializable]
public class CutSceneAction
{
    public CutSceneActionType actionType;

    public int panelIndex;      // FadeIn/FadeOut 대상
    public float waitseconds;       // WaitSeconds
    public string animatorTrigger;
}

public enum CutSceneType
{
    Prologue,
    Stage1,
    Stage2,
    Stage3,
    Stage4or5,
    Ending
}

[CreateAssetMenu(menuName = "Cutscene/CutSceneData")]
public class CutSceneData : ScriptableObject
{
    [TextArea(1, 10)]
    public string[] textSet;

    public List<CutSceneClickStep> clickSteps = new List<CutSceneClickStep>();
    public CutSceneType cutsceneType;
}


[Serializable]
public class CutSceneClickStep
{
    public List<CutSceneAction> actions = new List<CutSceneAction>();
}
