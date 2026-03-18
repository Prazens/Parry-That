using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueSpeakerTag
{
    None = 0,
    Sori = 1,
    Spirit = 2,
    Narration = 3,
}

public enum DialogueExpressionTag
{
    Idle = 0,
    Happy = 1,
    Angry = 2,
    Sad = 3,
    Surprised = 4,
}

[Serializable]
public class DialogueLine
{
    [Header("발화자 태그")]
    public DialogueSpeakerTag speakerTag = DialogueSpeakerTag.None;

    [Header("발화자 감정")]
    public DialogueExpressionTag expressionTag = DialogueExpressionTag.Idle;
    
    [TextArea(2, 5)]
    public string text;

    [Header("시간")]
    public float textRevealDuration = 1f;
    public float waitAfterReveal = 1f;
    public float waitBeforeReveal = 0f;

    [Header("이름 Override (선택)")] //일반적으로는 DialogueManager에서 Tag에 따라 이름 결정
    public string speakerNameOverride;
}

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    public IReadOnlyList<DialogueLine> Lines => lines;
}