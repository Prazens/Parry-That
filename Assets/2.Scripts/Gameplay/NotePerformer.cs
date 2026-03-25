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
    [SerializeField] private AttackNoticeController attackNoticeController;

    // 노트 관련
    private NoteDataNew[] notes;
    private int nextNoteIndex = 0;

    // Prepare Queue 관련
    private struct PrepareEntry
    {
        public int strikerIndex;
        public float arriveBeat;
        public float nextArriveBeat;
        public AttackType attackType;
        public PrepareEntry(int _strikerIndex, float _arriveBeat, float _nextArriveBeat, AttackType _attackType)
        {
            strikerIndex = _strikerIndex;
            arriveBeat = _arriveBeat;
            nextArriveBeat = _nextArriveBeat;
            attackType = _attackType;
        }
    }
    private Queue<PrepareEntry> prepareQueue = new();

    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
    }

    // 기존 ChartData와 NoteData의 구조를 변경하지 않고 임시로 이렇게 갑니다.
    public void InitNotes(List<ChartData> charts)
    {
        if (charts == null)
        {
            Debug.LogWarning("NotePerformer.InitNotes: Charts is null");
            notes = System.Array.Empty<NoteDataNew>();
            return;
        }

        // BPM 설정 (첫 번째 차트 기준)
        if (charts.Count > 0 && charts[0] != null && StageFlowManager.Instance != null)
        {
            StageFlowManager.Instance.SetBPM(charts[0].bpm);
        }

        // 전체 노트 개수 미리 계산
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
        allNotes.Sort((a, b) => a.noticeBeat.CompareTo(b.noticeBeat));

        notes = allNotes.ToArray();
        nextNoteIndex = 0;

        prepareQueue.Clear();
    }

    void Update()
    {
        if (StageFlowManager.Instance == null) return;
        if (notes == null || notes.Length <= 0) return;
        float currentSec = StageFlowManager.Instance.currentTime;

        // 공격 준비
        PrepareNextNote(currentSec);

        // 공격
        HandleAttack(currentSec);
    }

    private void PrepareNextNote(float currentSec)
    {
        // 같은 타이밍에 발생할 수 있는 모든 노트를 처리
        while (nextNoteIndex < notes.Length && 
               currentSec >= StageFlowManager.Instance.BeatToSec(notes[nextNoteIndex].noticeBeat))
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

        prepareQueue.Enqueue(new PrepareEntry(note.strikerIndex, arriveBeat, nextArriveBeat, attackType));

        StrikerController striker = strikerManager.strikerList[note.strikerIndex];
        striker.OnNotice(arriveBeat, nextArriveBeat, attackType);

        // 예고 이펙트 출력
        float durationSec = StageFlowManager.Instance.BeatToSec(arriveBeat) - StageFlowManager.Instance.BeatToSec(note.noticeBeat);
        attackNoticeController.ShowNewNotice(attackType, striker.location, durationSec);
    }

    private void HandleAttack(float currentSec)
    {
        // 예고 순서와 공격 시작 순서가 반드시 일치한다고 가정함
        // (AttackNotice의 생성/삭제 순서 일관성 보장을 위해)
        while (prepareQueue.Count > 0)
        {
            var prepareEntry = prepareQueue.Peek();
            var striker = strikerManager.strikerList[prepareEntry.strikerIndex];

            // 다음 공격 시작 시각에 도달하지 않았으면 종료
            if (currentSec < StageFlowManager.Instance.BeatToSec(prepareEntry.arriveBeat) - striker.Visual.preAttackDelay)
                break;

            prepareQueue.Dequeue();

            // isLastInBurst: 이 스트라이커의 현재 준비된 공격 중 마지막인지
            bool isLastInBurst = true;
            foreach (var entry in prepareQueue)
            {
                if (entry.strikerIndex == prepareEntry.strikerIndex)
                {
                    isLastInBurst = false;
                    break;
                }
            }
            // isFinalAttack: 이 스트라이커의 마지막 공격인지
            bool isFinalAttack = isLastInBurst && !HasMoreFutureNotes(prepareEntry.strikerIndex);

            striker.OnAttackStart(new StrikerAttackContext(currentSec, prepareEntry.arriveBeat, prepareEntry.nextArriveBeat, prepareEntry.attackType,
                                                            isLastInBurst, isFinalAttack));

            // 예고 이펙트 제거
            attackNoticeController.DestroyFirstNotice(prepareEntry.attackType);
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
}
