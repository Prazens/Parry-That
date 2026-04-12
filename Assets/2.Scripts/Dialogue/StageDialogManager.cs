using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

[System.Serializable]
public class SpeakerEmotionIcons
{
    public Sprite idleIcon;
    public Sprite happyIcon;
    public Sprite angryIcon;
    public Sprite sadIcon;
    public Sprite surprisedIcon;

    public Sprite this[DialogueExpressionTag index]
    {
        get
        {
            return index switch
            {
                DialogueExpressionTag.Idle => idleIcon,
                DialogueExpressionTag.Happy => happyIcon,
                DialogueExpressionTag.Angry => angryIcon,
                DialogueExpressionTag.Sad => sadIcon,
                DialogueExpressionTag.Surprised => surprisedIcon,
                _ => null
            };
        }
    }
}

public class StageDialogManager : Singleton<StageDialogManager>
{
    private DialogueData currentDialogueData = null;
    private Sequence dialogSequence;
    private float listWidth;
    public bool isDialogPlaying => dialogSequence != null;

    [Header("대화 재생 설정")]
    public float playSpeed = 1f;
    public float speakerCreation = 0.1f;
    public float speakerDuration = 0.1f;
    public float lineCreation = 0.1f;
    public float lineDuration = 0.1f;
    public float lineUpDuration = 0.1f;

    private float dialogTimer = 0f;

    [Header("발화자 아이콘")]
    public List<SpeakerEmotionIcons> speakerIcons;

    [Header("대화 UI 프리팹")]
    public GameObject dialogPrefab;
    public GameObject dialogPrefabR;

    [Header("리스트 오브젝트")]
    public Transform dialogList;

    protected override void OnAwake()
    {
        InitDialog();
    }

    public void InitDialog()
    {
        dialogSequence?.Kill();
        dialogSequence = null;
        listWidth = dialogList.GetComponent<RectTransform>().rect.width
                    - dialogList.GetComponent<VerticalLayoutGroup>().padding.right
                    - dialogList.GetComponent<VerticalLayoutGroup>().padding.left;
    }

    public void StartDialog(DialogueData dialogueData)
    {
        if (dialogSequence != null)
        {
            Debug.LogWarning("StageDialogManager: 이미 대화가 진행 중입니다. 기존 대화를 종료하고 새 대화를 시작합니다.");
            dialogSequence.Kill();
            dialogSequence = null;
        }

        currentDialogueData = dialogueData;
        if (currentDialogueData == null)
        {
            Debug.LogError("StageDialogManager: currentDialogueData is null. Cannot start dialog.");
            return;
        }

        Debug.Log("StageDialogManager: Starting dialog with " + currentDialogueData.Lines.Count + " lines");

        dialogSequence = DOTween.Sequence();
        dialogSequence.OnComplete(() => FinishDialog());
        dialogTimer = 0f;

        foreach (var line in currentDialogueData.Lines)
        {
            Debug.Log($"StageDialogManager: Processing line - Speaker: {line.speakerTag}, Expression: {line.expressionTag}, Text: {line.text}");

            bool isLeft = IsLeft(line);
            string speakerName = GetSpeakerName(line);
            Sprite speakerSprite = GetCharacterSprite(line);

            GameObject dialogGO = Instantiate(isLeft ? dialogPrefab : dialogPrefabR, dialogList);

            RectTransform rt = dialogGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(listWidth, rt.sizeDelta.y);

            DialogLineController dialogLineController = dialogGO.GetComponent<DialogLineController>();
            dialogLineController.Init(line, speakerName, speakerSprite, isLeft);

            dialogSequence.Insert(dialogTimer, dialogLineController.MakeSequence());
            dialogTimer += line.waitBeforeReveal + line.textRevealDuration;
        }
    }

    public void SetDialogSpeed(float speed)
    {
        Debug.Log($"StageDialogManager: Setting dialog speed to {speed}");

        if (dialogSequence != null)
        {
            dialogSequence.timeScale = speed;
        }
    }

    public void FinishDialogLine()
    {
        Debug.Log("StageDialogManager: FinishDialogLine");
    }

    public void FinishDialog()
    {
        Debug.Log("StageDialogManager: FinishDialog");
        dialogSequence?.Kill();
        dialogSequence = null;
    }

    private string GetSpeakerName(DialogueLine line)
    {
        if (!string.IsNullOrEmpty(line.speakerNameOverride))
            return line.speakerNameOverride;

        switch (line.speakerTag)
        {
            case DialogueSpeakerTag.Sori:
                return "소리";
            case DialogueSpeakerTag.Spirit:
                return "정령";
            case DialogueSpeakerTag.Narration:
                return "";
            default:
                return "";
        }
    }

    private bool IsLeft(DialogueLine line)
    {
        switch (line.speakerTag)
        {
            case DialogueSpeakerTag.Spirit:
                return true;
            case DialogueSpeakerTag.Sori:
                return false;
            case DialogueSpeakerTag.Narration:
                return true;
            default:
                return true;
        }
    }

    private Sprite GetCharacterSprite(DialogueLine line)
    {
        if (line.speakerTag == DialogueSpeakerTag.Narration)
            return null;

        int speakerIndex = (int)line.speakerTag;
        if (speakerIcons == null || speakerIndex < 0 || speakerIndex >= speakerIcons.Count)
            return null;

        SpeakerEmotionIcons iconSet = speakerIcons[speakerIndex];
        if (iconSet == null)
            return null;

        return iconSet[line.expressionTag];
    }

    void OnDisable()
    {
        dialogSequence?.Kill();
    }

    void OnDestroy()
    {
        dialogSequence?.Kill();
    }
}