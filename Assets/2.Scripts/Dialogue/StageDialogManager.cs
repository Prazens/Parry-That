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
    // private int maxCharsPerLine = 0;
    private Sequence dialogSequence;
    private float listWidth;  // 대화 UI 리스트의 너비 (대화창 크기 계산용)

    [Header("대화 재생 설정")]
    public float playSpeed = 1f; // 대화 재생 속도 (1f = 정상 속도)
    public float speakerCreation = 0.1f; // 발화자 등장 시간 (초)
    public float speakerDuration = 0.1f; // 발화자 등장 후 줄 생성까지의 시간 (초)
    public float lineCreation = 0.1f; // 한 줄이 만들어지는 시간 (초)
    public float lineDuration = 0.1f; // 줄이 완성된 후 대사가 출력되기까지의 시간 (초)
    public float lineUpDuration = 0.1f; // 줄이 올라가는 시간 (초)

    private float dialogTimer = 0f; // 대화용 타이머

    [Header("발화자 아이콘")]
    public List<SpeakerEmotionIcons> speakerIcons; // 발화자 아이콘 배열 (speakerTag, expressionTag 순서로 접근)

    [Header("대화 UI 프리팹")]
    public GameObject dialogPrefab; // 대화 UI 프리팹
    public GameObject dialogPrefabR; // 대화 UI 프리팹 (오른쪽 발화자용)

    [Header("리스트 오브젝트")]
    public Transform dialogList; // 대화 UI 오브젝트를 담을 부모 오브젝트

    [Header("테스트용 대화 데이터")]
    public DialogueData testDialogueData;

    protected override void OnAwake()
    {
        InitDialog();

        // 테스트용
        // 실 적용시에는 외부에서 바로 StartDialog() 호출
#if UNITY_EDITOR
        Debug.Log("StageDialogManager: Start");

        // 테스트용 대화 재생
        if (testDialogueData != null)
        {
            StartDialog(testDialogueData);
        }
#endif
    }

    /// <summary>
    /// 대화 초기화
    /// <para>대화창 크기 계산, 대화 시퀀스 초기화</para>
    /// </summary>
    public void InitDialog()
    {
        dialogSequence = null;
        listWidth = dialogList.GetComponent<RectTransform>().rect.width
                    - dialogList.GetComponent<VerticalLayoutGroup>().padding.right
                    - dialogList.GetComponent<VerticalLayoutGroup>().padding.left;
        // 대화 UI 리스트의 너비 계산 (대화창 크기에서 여백 제외)
    }

    /// <summary>
    /// 대화 시작
    /// <para>대사 데이터에 따라 대화 UI 생성 및 시퀀스 구성</para>
    /// <para>외부에서 호출</para>
    /// </summary>
    /// <param name="dialogueData">대화 데이터</param>
    public void StartDialog(DialogueData dialogueData)
    {
        if (dialogSequence != null)
        {
            Debug.LogWarning("StageDialogManager: 이미 대화가 진행 중입니다. 기존 대화를 종료하고 새 대화를 시작합니다.");
            dialogSequence.Kill();
            dialogSequence = null;
        }

        Debug.Log("StageDialogManager: Starting dialog with " + dialogueData.Lines.Count + " lines");

        currentDialogueData = dialogueData;
        if (currentDialogueData == null)
        {
            Debug.LogError("StageDialogManager: currentDialogueData is null. Cannot start dialog.");
            return;
        }

        dialogSequence = DOTween.Sequence();
        dialogSequence.OnComplete(() => FinishDialog());
        dialogTimer = 0f;

        foreach (var line in currentDialogueData.Lines)
        {
            Debug.Log($"StageDialogManager: Processing line - Speaker: {line.speakerTag}, Expression: {line.expressionTag}, Text: {line.text}");

            // 각 대사 라인마다 DialogLineController를 생성하고 초기화
            GameObject dialogGO = Instantiate(line.speakerTag == DialogueSpeakerTag.Sori ? dialogPrefabR : dialogPrefab, dialogList);

            RectTransform rt = dialogGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(listWidth, rt.sizeDelta.y);

            DialogLineController dlc = dialogGO.GetComponent<DialogLineController>();
            dlc.Init(line);
            dialogSequence.Insert(dialogTimer, dlc.MakeSequence());
            dialogTimer += line.waitBeforeReveal + line.textRevealDuration;
        }
    }

    /// <summary>
    /// 대화 속도 조절
    /// <para>현재 재생 중인 대화 시퀀스의 속도를 변경</para>
    /// <para>빨리감기 같은 효과 구현 시 빨리감기 시작, 끝에 각각 한번씩 호출</para>
    /// </summary>
    /// <param name="speed">대화 속도</param>
    public void SetDialogSpeed(float speed)
    {
        Debug.Log($"StageDialogManager: Setting dialog speed to {speed}");

        if (dialogSequence != null)
        {
            dialogSequence.timeScale = speed;
        }
    }

    /// <summary>
    /// 대화 라인 종료 처리
    /// </summary>
    public void FinishDialogLine()
    {
        Debug.Log("StageDialogManager: FinishDialogLine");

        // 대화 라인 하나 종료 (완전 사라질 때) 처리
        // 자동으로 올라가는 애니메이션은 이미 VerticalLayoutGroup과 ContentSizeFitter로 구현되어 있음
        // 언젠가 다른 처리가 필요하다면 여기에 추가
    }

    /// <summary>
    /// 모든 대화 라인이 종료된 후 호출
    /// </summary>
    public void FinishDialog()
    {
        Debug.Log("StageDialogManager: FinishDialog");
        dialogSequence = null;

        // 대화 종료 (완전 사라질 때) 처리
        // 이 밑에 외부 호출 추가
    }

}
