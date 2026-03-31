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
    [SerializeField] private AttackNoticeController attackNoticeController;

    // 노트 관련
    private NoteData[] notes;
    private int nextNoteIndex = 0;

    // Prepare Queue 관련
    private struct PrepareEntry
    {
        public int strikerIndex;
        public float arriveBeat;
        public float nextArriveBeat;
        public AttackType attackType;

        public PrepareEntry(int strikerIndex, float arriveBeat, float nextArriveBeat, AttackType attackType)
        {
            this.strikerIndex = strikerIndex;
            this.arriveBeat = arriveBeat;
            this.nextArriveBeat = nextArriveBeat;
            this.attackType = attackType;
        }
    }

    private Queue<PrepareEntry> prepareQueue = new();

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
    }

    public void InitNotes(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogWarning("NotePerformer.InitNotes: Chart is null");
            notes = System.Array.Empty<NoteData>();
            nextNoteIndex = 0;
            prepareQueue.Clear();
            return;
        }

        if (StageFlowManager.Instance != null)
        {
            StageFlowManager.Instance.SetBPM(chart.bpm);
        }

        if (chart.notes == null)
        {
            notes = System.Array.Empty<NoteData>();
            nextNoteIndex = 0;
            prepareQueue.Clear();
            return;
        }

        notes = (NoteData[])chart.notes.Clone();

        System.Array.Sort(notes, (a, b) => a.noticeBeat.CompareTo(b.noticeBeat));

        nextNoteIndex = 0;
        prepareQueue.Clear();
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

        float arriveBeat = note.arriveBeat;
        AttackType attackType = (AttackType)note.type;

        float nextArriveBeat = -1f;
        for (int i = nextNoteIndex + 1; i < notes.Length; i++)
        {
            if (notes[i].strikerIndex == note.strikerIndex)
            {
                nextArriveBeat = notes[i].arriveBeat;
                break;
            }
        }

        prepareQueue.Enqueue(new PrepareEntry(note.strikerIndex, arriveBeat, nextArriveBeat, attackType));

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

        striker.OnNotice(arriveBeat, nextArriveBeat, attackType);

        if (attackNoticeController != null)
        {
            float durationSec =
                StageFlowManager.Instance.BeatToSec(arriveBeat) -
                StageFlowManager.Instance.BeatToSec(note.noticeBeat);

            attackNoticeController.ShowNewNotice(attackType, striker.location, durationSec);
        }
    }

    private void HandleAttack(float currentSec)
    {
        while (prepareQueue.Count > 0)
        {
            PrepareEntry prepareEntry = prepareQueue.Peek();

            if (strikerManager == null || strikerManager.strikerList == null)
                break;

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
                    prepareEntry.nextArriveBeat,
                    prepareEntry.attackType,
                    isLastInBurst,
                    isFinalAttack
                )
            );

            if (attackNoticeController != null)
            {
                attackNoticeController.DestroyFirstNotice(prepareEntry.attackType);
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