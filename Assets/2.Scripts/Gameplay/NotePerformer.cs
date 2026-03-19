using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note를 읽고 연주하는 역할.
/// 시간에 따라 예고 이펙트, 공격 명령 등을 담당.
/// </summary>
public class NotePerformer : MonoBehaviour
{
    // 기존 ChartData와 NoteData의 구조를 변경하지 않고 임시로 이렇게 갑니다.
    private class NoteDataNew
    {
        public int strikerIndex;
        
        public float noticeBeat; // 예고 발생 시간 (박자 단위)
        public float arriveBeat; // 도착 시간 (박자 단위)

        public int type;
    }

    private PlayerManager playerManager;
    [SerializeField] private StrikerManager strikerManager;

    // 노트 관련
    private float bpm = 60f;
    private NoteDataNew[] notes;
    private int nextNoteIndex = 0;

    // Prepare Queue 관련
    private struct PrepareEntry
    {
        public float arriveBeat;
        public float nextArriveBeat;
        public AttackType attackType;
        public PrepareEntry(float _arriveBeat, float _nextArriveBeat, AttackType _attackType)
        {
            arriveBeat = _arriveBeat;
            nextArriveBeat = _nextArriveBeat;
            attackType = _attackType;
        }
    }
    // strikerIndex별로 각각의 Prepare Queue를 가짐
    private List<Queue<PrepareEntry>> prepareQueues = new();

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
    }

    // 기존 ChartData와 NoteData의 구조를 변경하지 않고 임시로 이렇게 갑니다.
    public void InitNotes(List<ChartData> charts)
    {
        if (charts == null)
        {
            Debug.LogWarning("NotePlayer.InitNotes: Charts is null");
            notes = System.Array.Empty<NoteDataNew>();
            return;
        }

        // 전체 노트 개수 미리 계산 (성능 향상)
        int totalCount = 0;
        for (int i = 0; i < charts.Count; i++)
        {
            if (charts[i]?.notes != null)
                totalCount += charts[i].notes.Length;
        }

        List<NoteDataNew> allNotes = new(totalCount);

        // Chart index를 strikerIndex로 저장하면서 변환
        for (int chartIndex = 0; chartIndex < charts.Count; chartIndex++)
        {
            ChartData chart = charts[chartIndex];
            if (chart == null || chart.notes == null)
                continue;

            // 임시로 bpm 설정하는 코드
            if (bpm != chart.bpm)
                bpm = chart.bpm;

            for (int j = 0; j < chart.notes.Length; j++)
            {
                NoteData note = chart.notes[j];

                allNotes.Add(new NoteDataNew
                {
                    strikerIndex = chartIndex,
                    noticeBeat = note.time,
                    arriveBeat = note.arriveTime,
                    type = note.type
                });
            }
        }

        // noticeBeat 기준 정렬
        allNotes.Sort((a, b) =>
        {
            if (a.noticeBeat < b.noticeBeat) return -1;
            if (a.noticeBeat > b.noticeBeat) return 1;
            return 0; // 같으면 순서 무관
        });

        notes = allNotes.ToArray();
        nextNoteIndex = 0;

        // 스트라이커별 큐 초기화
        prepareQueues.Clear();
        for (int i = 0; i < charts.Count; i++)
        {
            prepareQueues.Add(new Queue<PrepareEntry>());
        }
    }

    void Update()
    {
        if (StageFlowManager.Instance == null) return;
        float currentSec = StageFlowManager.Instance.currentTime;

        // 공격 준비
        //PrepareNextNote(currentSec);

        // 공격
        //HandleAttack(currentSec);
    }

    private void PrepareNextNote(float currentSec)
    {
        // 같은 타이밍에 발생할 수 있는 모든 노트를 처리
        while (nextNoteIndex < notes.Length && 
               currentSec >= BeatToSec(notes[nextNoteIndex].noticeBeat, playerManager.musicOffset))
        {
            // 공격 준비 로직 실행
            PrepareForAttack();
            nextNoteIndex++;
        }
    }

    private void PrepareForAttack()
    {
        NoteDataNew note = notes[nextNoteIndex];

        float arriveBeat = note.arriveBeat;
        AttackType attackType = (AttackType)note.type;

        // 다음 노트 박자 찾기
        float nextArriveBeat = -1f;
        for (int i = nextNoteIndex + 1; i < notes.Length; i++)
        {
            if (notes[i].strikerIndex == note.strikerIndex)
            {
                nextArriveBeat = notes[i].arriveBeat;
                break;
            }
        }

        // 해당 스트라이커의 큐에 저장
        if (note.strikerIndex >= 0 && note.strikerIndex < prepareQueues.Count)
        {
            prepareQueues[note.strikerIndex].Enqueue(new PrepareEntry(arriveBeat, nextArriveBeat, attackType));
        }

        StrikerController striker = strikerManager.strikerList[note.strikerIndex];
        striker.OnNotice(arriveBeat, nextArriveBeat, attackType);
    }

    private void HandleAttack(float currentSec)
    {
        for (int i = 0; i < prepareQueues.Count; i++)
        {
            var queue = prepareQueues[i];
            var striker = strikerManager.strikerList[i];

            while (queue.Count > 0 && currentSec >= BeatToSec(queue.Peek().arriveBeat, playerManager.musicOffset) - striker.Visual.preAttackDelay)
            {
                var prepare = queue.Dequeue();
                bool isLastInBurst = (queue.Count == 0);
                // 퇴장 조건: Queue가 비었으며, 앞으로 등장할 노트도 없음
                bool isFinalAttack = isLastInBurst && !HasMoreFutureNotes(i);

                striker.OnAttackStart(new StrikerAttackContext(currentSec, prepare.arriveBeat, prepare.nextArriveBeat, prepare.attackType, isLastInBurst, isFinalAttack));
            }
        }
    }

    private bool HasMoreFutureNotes(int strikerIndex)
    {
        for (int i = nextNoteIndex; i < notes.Length; i++)
        {
            if (notes[i].strikerIndex == strikerIndex) return true;
        }
        return false;
    }

    /// <summary>
    /// beatIndex를 sec로 단위 변환 후 offset을 더함.
    /// </summary>
    /// <returns></returns>
    public float BeatToSec(float beatIndex, float offset = 0)
    => beatIndex * (60f / bpm) + offset;
}
