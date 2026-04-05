using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note를 읽고 연주하는 역할.
/// 시간에 따라 예고 이펙트, 공격 명령 등을 담당.
/// </summary>
public class NotePerformer : MonoBehaviour
{
    private PlayerManager playerManager;
    [SerializeField] private StrikerManager strikerManager;

    [Header("Notice Handlers")]
    [SerializeField] private IAttackHandler<IAttackContext> commonAttackNoticeHandler;
    [SerializeField] private IAttackHandler<IAttackContext> holdAttackNoticeHandler;

    [Header("Judge Handlers")]
    [SerializeField] private JudgeSystem judgeSystem;
    [SerializeField] private IAttackHandler<IAttackContext> normalAttackJudgeHandler;
    [SerializeField] private IAttackHandler<IAttackContext> strongAttackJudgeHandler;
    [SerializeField] private IAttackHandler<IAttackContext> holdAttackJudgeHandler;

    private Dictionary<AttackType, IAttackHandler<IAttackContext>> attackNoticeHandlerDict;
    private Dictionary<AttackType, IAttackHandler<IAttackContext>> attackJudgeHandlerDict;

    // 노트 관련
    private NoteData[] notes;
    private int nextNoteIndex = 0;

    // Prepare Queue 관련
    private struct PrepareEntry
    {
        public int strikerIndex;
        public float arriveBeat;
        public AttackType attackType;

        public PrepareEntry(int strikerIndex, float arriveBeat, AttackType attackType)
        {
            this.strikerIndex = strikerIndex;
            this.arriveBeat = arriveBeat;
            this.attackType = attackType;
        }
    }

    private Queue<PrepareEntry> prepareQueue = new();

    private void Awake()
    {
        attackNoticeHandlerDict = new()
        {
            { AttackType.Normal, commonAttackNoticeHandler },
            { AttackType.Strong, commonAttackNoticeHandler },
            { AttackType.HoldStart, holdAttackNoticeHandler },
            { AttackType.HoldStop, holdAttackNoticeHandler },
            { AttackType.HoldFinishStrong, holdAttackNoticeHandler },
        };

        attackJudgeHandlerDict = new()
        {
            { AttackType.Normal, normalAttackJudgeHandler },
            { AttackType.Strong, strongAttackJudgeHandler },
            { AttackType.HoldStart, holdAttackJudgeHandler },
            { AttackType.HoldStop, holdAttackJudgeHandler },
            { AttackType.HoldFinishStrong, holdAttackJudgeHandler },
        };

        judgeSystem.Judged -= OnJudge;
        judgeSystem.Judged += OnJudge;
    }

    private void OnDestroy()
    {
        judgeSystem.Judged -= OnJudge;
    }

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
    }

    public void InitNotes(ChartData chart)
    {
        nextNoteIndex = 0;
        prepareQueue.Clear();

        if (chart == null || chart.notes == null)
        {
            Debug.LogWarning("NotePerformer.InitNotes: Chart is null");
            notes = System.Array.Empty<NoteData>();
            
            return;
        }

        StageFlowManager.Instance?.SetBPM(chart.bpm);
        notes = (NoteData[])chart.notes.Clone();
        System.Array.Sort(notes, (a, b) => a.noticeBeat.CompareTo(b.noticeBeat));
    }

    private void Update()
    {
        if (StageFlowManager.Instance == null) return;
        if (notes == null || notes.Length == 0) return;

        float currentSec = StageFlowManager.Instance.currentTime;

        PrepareNextNote(currentSec);
        HandleAttack(currentSec);
    }

    private void PrepareNextNote(float currentSec)
    {
        while (nextNoteIndex < notes.Length &&
               currentSec >= StageFlowManager.Instance.BeatToSec(notes[nextNoteIndex].noticeBeat))
        {
            PrepareForAttack();
            nextNoteIndex++;
        }
    }

    private void PrepareForAttack()
    {
        NoteData note = notes[nextNoteIndex];
        AttackType attackType = (AttackType)note.type;

        if (strikerManager == null || strikerManager.strikerList == null)
        {
            Debug.LogError("NotePerformer.PrepareForAttack: strikerManager or strikerList is null");
            return;
        }

        if (note.strikerIndex < 0 || note.strikerIndex >= strikerManager.strikerList.Count)
        {
            Debug.LogError($"NotePerformer.PrepareForAttack: invalid strikerIndex {note.strikerIndex}");
            return;
        }

        StrikerController striker = strikerManager.strikerList[note.strikerIndex];
        if (striker == null)
        {
            Debug.LogError($"NotePerformer.PrepareForAttack: striker is null at index {note.strikerIndex}");
            return;
        }

        // Judgeable 생성
        List<Judgeable> judgeables = new();
        if (attackJudgeHandlerDict.TryGetValue(attackType, out var attackJudgeHandler))
        {
            if (attackJudgeHandler is NormalAttackJudgeHandler)
            {
                var attackContext = new NormalAttackJudgeContext(note, striker.location);
                attackJudgeHandler.OnNotice(attackContext);
                judgeables.Add(attackContext.judgeable);
            }
            else if (attackJudgeHandler is StrongAttackJudgeHandler)
            {
                var attackContext = new StrongAttackJudgeContext(note, striker.location);
                attackJudgeHandler.OnNotice(attackContext);
                judgeables.Add(attackContext.judgeable);
            }
            else if (attackJudgeHandler is HoldAttackJudgeHandler)
            {
                var attackContext = new HoldAttackJudgeContext(note, notes[nextNoteIndex + 1], striker.location);
                attackJudgeHandler.OnNotice(attackContext);
                judgeables.Add(attackContext.judgeable);
                judgeables.Add(attackContext.nextJudgeable);
            }
        }

        // Notice
        if (attackNoticeHandlerDict.TryGetValue(attackType, out var attackNoticeHandler))
        {
            if (attackNoticeHandler is CommonAttackNoticeHandler)
            {
                Judgeable judgeable = judgeables.Count >= 1 ? judgeables[0] : null;
                attackNoticeHandler.OnNotice(new CommonAttackNoticeContext(note, striker.location, judgeable));
            }
            else if (attackNoticeHandler is HoldAttackNoticeHandler)
            {
                attackNoticeHandler.OnNotice(new HoldAttackNoticeContext(note, judgeables));
            }
        }

        prepareQueue.Enqueue(new PrepareEntry(note.strikerIndex, note.arriveBeat, attackType));

        striker.OnNotice(attackType);
    }

    private void HandleAttack(float currentSec)
    {
        if (strikerManager == null || strikerManager.strikerList == null)
            return;

        while (prepareQueue.Count > 0)
        {
            PrepareEntry prepareEntry = prepareQueue.Peek();

            if (prepareEntry.strikerIndex < 0 || prepareEntry.strikerIndex >= strikerManager.strikerList.Count)
            {
                Debug.LogError($"NotePerformer.HandleAttack: invalid strikerIndex {prepareEntry.strikerIndex}");
                prepareQueue.Dequeue();
                continue;
            }

            StrikerController striker = strikerManager.strikerList[prepareEntry.strikerIndex];
            if (striker == null || striker.Visual == null)
            {
                Debug.LogError($"NotePerformer.HandleAttack: striker or striker.Visual is null at index {prepareEntry.strikerIndex}");
                prepareQueue.Dequeue();
                continue;
            }

            if (currentSec < StageFlowManager.Instance.BeatToSec(prepareEntry.arriveBeat) - striker.Visual.preAttackDelay)
                break;

            prepareQueue.Dequeue();

            bool isLastInBurst = true;
            foreach (PrepareEntry entry in prepareQueue)
            {
                if (entry.strikerIndex == prepareEntry.strikerIndex)
                {
                    isLastInBurst = false;
                    break;
                }
            }

            bool isFinalAttack = isLastInBurst && !HasMoreFutureNotes(prepareEntry.strikerIndex);

            striker.OnAttackStart(
                new StrikerAttackContext(
                    currentSec,
                    prepareEntry.arriveBeat,
                    prepareEntry.attackType,
                    isLastInBurst,
                    isFinalAttack
                )
            );
        }
    }

    public void OnJudge(JudgeContext context)
    {
        Judgeable judgeable = context.judgeable;
        judgeable.strikerController.OnJudge(context);

        // 터치 없이 LateMiss가 난 경우의 처리
        if (attackJudgeHandlerDict.TryGetValue(judgeable.attackType, out var attackJudgeHandler))
        {
            if (attackJudgeHandler is NormalAttackJudgeHandler)
            {
                attackJudgeHandler.OnJudge(context);
            }
            else if (attackJudgeHandler is StrongAttackJudgeHandler)
            {
                attackJudgeHandler.OnJudge(context);
            }
            else if (attackJudgeHandler is HoldAttackJudgeHandler)
            {
                attackJudgeHandler.OnJudge(context);
            }
        }
    }

    private bool HasMoreFutureNotes(int strikerIndex)
    {
        for (int i = nextNoteIndex; i < notes.Length; i++)
        {
            if (notes[i].strikerIndex == strikerIndex)
                return true;
        }

        return false;
    }
}