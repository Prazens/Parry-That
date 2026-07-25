using DG.Tweening;
using System.Collections.Generic;
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
    private DialogueData currentDialogueData;
    private Sequence dialogSequence;

    private float listWidth;
    private float dialogTimer;

    private int remainingLineCount;
    private int nextControllerIndex;
    private readonly List<DialogLineController> activeControllers = new();
    private readonly List<DialogLineController> pendingControllers = new();


    public bool isDialogPlaying => dialogSequence != null;

    [Header("대화 재생 설정")]
    public float playSpeed = 1f;
    public float speakerCreation = 0.1f;
    public float speakerDuration = 0.1f;
    public float lineCreation = 0.1f;
    public float lineDuration = 0.1f;
    public float lineUpDuration = 0.1f;

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

    private void Update()
    {
        if (!isDialogPlaying)
            return;

    #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            SkipCurrentDialog();
        }
    #else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SkipCurrentDialog();
        }
    #endif
    }

    public void InitDialog()
    {
        dialogSequence?.Kill();
        dialogSequence = null;

        RectTransform listRect = dialogList.GetComponent<RectTransform>();
        VerticalLayoutGroup layoutGroup =
            dialogList.GetComponent<VerticalLayoutGroup>();

        listWidth = listRect.rect.width
                    - layoutGroup.padding.right
                    - layoutGroup.padding.left;
    }

    public void StartDialog(DialogueData dialogueData)
    {
        if (dialogSequence != null)
        {
            Debug.LogWarning(
                "StageDialogManager: 이미 대화가 진행 중입니다."
            );

            return;
        }

        currentDialogueData = dialogueData;

        if (currentDialogueData == null)
        {
            Debug.LogError(
                "StageDialogManager: currentDialogueData is null. Cannot start dialog."
            );

            return;
        }

        if (currentDialogueData.Lines == null ||
            currentDialogueData.Lines.Count == 0)
        {
            Debug.LogWarning(
                "StageDialogManager: 재생할 대사가 없습니다."
            );

            currentDialogueData = null;
            return;
        }

        Debug.Log(
            "StageDialogManager: Starting dialog with "
            + currentDialogueData.Lines.Count
            + " lines"
        );
        
        activeControllers.Clear();
        pendingControllers.Clear();
        nextControllerIndex = 0;

        remainingLineCount = currentDialogueData.Lines.Count;
        dialogSequence = DOTween.Sequence();
        dialogTimer = 0f;

        foreach (DialogueLine line in currentDialogueData.Lines)
        {
            Debug.Log(
                $"StageDialogManager: Processing line - " +
                $"Speaker: {line.speakerTag}, " +
                $"Expression: {line.expressionTag}, " +
                $"Text: {line.text}"
            );

            bool isLeft = IsLeft(line);
            string speakerName = GetSpeakerName(line);
            Sprite speakerSprite = GetCharacterSprite(line);

            GameObject dialogGO = Instantiate(
                isLeft ? dialogPrefab : dialogPrefabR,
                dialogList
            );

            RectTransform rectTransform =
                dialogGO.GetComponent<RectTransform>();

            rectTransform.sizeDelta = new Vector2(
                listWidth,
                rectTransform.sizeDelta.y
            );

            DialogLineController dialogLineController =
                dialogGO.GetComponent<DialogLineController>();

            dialogLineController.Init(
                line,
                speakerName,
                speakerSprite,
                isLeft
            );

            dialogLineController.MakeSequence();

            pendingControllers.Add(dialogLineController);

            int controllerIndex = pendingControllers.Count - 1;
            DialogLineController controller = dialogLineController;

            dialogSequence.InsertCallback(dialogTimer, () =>
            {
                PlayController(controller, controllerIndex);
            });

            // waitAfterReveal은 의도적으로 포함하지 않는다.
            // 이전 대사가 사라지기 전에 다음 대사가 시작될 수 있다.
            dialogTimer +=
                line.waitBeforeReveal
                + line.textRevealDuration;
        }

        dialogSequence.Play();
    }

    private void PlayController(DialogLineController controller, int controllerIndex)
    {
        // 스킵 때문에 이미 수동 재생된 대사라면 다시 실행하지 않는다.
        if (controllerIndex < nextControllerIndex)
            return;

        if (controller == null)
        {
            nextControllerIndex = Mathf.Max(
                nextControllerIndex,
                controllerIndex + 1
            );

            return;
        }

        nextControllerIndex = controllerIndex + 1;

        if (!activeControllers.Contains(controller))
        {
            activeControllers.Add(controller);
        }

        controller.Play();
    }

    private void PlayNextControllerImmediately()
    {
        if (nextControllerIndex >= pendingControllers.Count)
            return;

        DialogLineController controller =
            pendingControllers[nextControllerIndex];

        int controllerIndex = nextControllerIndex;

        PlayController(controller, controllerIndex);
    }

    public void SetDialogSpeed(float speed)
    {
        Debug.Log(
            $"StageDialogManager: Setting dialog speed to {speed}"
        );

        playSpeed = speed;

        if (dialogSequence != null)
        {
            dialogSequence.timeScale = speed;
        }
    }

    public void SkipCurrentDialog()
    {
        StageFlowManager flowManager = StageFlowManager.Instance;

        if (flowManager != null && !flowManager.CanSkipDialogue())
        {
            return;
        }

        for (int i = activeControllers.Count - 1; i >= 0; i--)
        {
            DialogLineController controller = activeControllers[i];

            if (controller == null)
            {
                activeControllers.RemoveAt(i);
                continue;
            }

            controller.SkipCurrentStep();
        }
    }

    public void FinishDialogLine(DialogLineController controller)
    {
        activeControllers.Remove(controller);

        remainingLineCount--;

        if (remainingLineCount <= 0)
        {
            FinishDialog();
            return;
        }

        // 화면에 남은 대사가 없으면 다음 대사를 즉시 시작한다.
        if (activeControllers.Count == 0)
        {
            PlayNextControllerImmediately();
        }
    }

    public void FinishDialog()
    {
        Debug.Log("StageDialogManager: FinishDialog");

        dialogSequence?.Kill();
        dialogSequence = null;

        activeControllers.Clear();
        pendingControllers.Clear();
        nextControllerIndex = 0;

        currentDialogueData = null;
        remainingLineCount = 0;
        dialogTimer = 0f;
    }

    private string GetSpeakerName(DialogueLine line)
    {
        if (!string.IsNullOrEmpty(line.speakerNameOverride))
        {
            return line.speakerNameOverride;
        }

        return line.speakerTag switch
        {
            DialogueSpeakerTag.Sori => "소리",
            DialogueSpeakerTag.Spirit => "정령",
            DialogueSpeakerTag.Narration => "",
            _ => ""
        };
    }

    private bool IsLeft(DialogueLine line)
    {
        return line.speakerTag switch
        {
            DialogueSpeakerTag.Spirit => true,
            DialogueSpeakerTag.Sori => false,
            DialogueSpeakerTag.Narration => true,
            _ => true
        };
    }

    private Sprite GetCharacterSprite(DialogueLine line)
    {
        if (line.speakerTag == DialogueSpeakerTag.Narration)
        {
            return null;
        }

        int speakerIndex = (int)line.speakerTag;

        if (speakerIcons == null ||
            speakerIndex < 0 ||
            speakerIndex >= speakerIcons.Count)
        {
            return null;
        }

        SpeakerEmotionIcons iconSet = speakerIcons[speakerIndex];

        if (iconSet == null)
        {
            return null;
        }

        return iconSet[line.expressionTag];
    }

    private void OnDisable()
    {
        dialogSequence?.Kill();
        dialogSequence = null;
    }

    private void OnDestroy()
    {
        dialogSequence?.Kill();
        dialogSequence = null;
    }
}