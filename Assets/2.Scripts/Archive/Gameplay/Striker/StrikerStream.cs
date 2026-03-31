/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기존 StrikerController의 연타 관련 로직 보관
/// </summary>
public class StrikerStream : MonoBehaviour
{
    private void PrepareForAttack()
    {
        // 연타 로직
        //if (!isMelee && noteType == AttackType.StreamStart)
        //{
        //    var j = new Judgeable(AttackType.StreamStart, arriveTime, location, this, null, this.ActStreamStart);
        //    j.SetStreamCount(CalcStreamCountForThisSegment(currentNoteIndex));
        //    judgeSystem.EnqueueJudgeable(j);
        //    judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.StreamFinish, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActStreamFinish));
        //}
    }

    public void ActStreamStart()
    {
        sound.PlayHoldStart();

        isRenta = true;
        FireRenTusache();
    }

    private void ActStreamFinish()
    {
        // 연타 종료 시의 처리, Judgeable의 onDestroy에 저장 후 호출
        // (streamstart 일 때만, streamend는 그냥 끝 시간 알림용, 별도 판정 처리 없음)
        sound.PlayHoldFinish();

        isRenta = false;

        holdExclamation?.GetComponent<holdExclamation>()?.ForceStop();
        while (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]);
            prepareExclamation.RemoveAt(0);
        }
        if (prepareQueue.Count != 0) prepareQueue.Dequeue();
    }

    private IEnumerator StreamHoldAnim()
    {
        // 연타 중의 애니메이션? 표시
        yield return null;
    }

    private int CalcStreamCountForThisSegment(int startIdx)
    {
        float startBeat = chartData.notes[startIdx].arriveTime;
        int finishIdx = startIdx + 1;
        float finishBeat = (finishIdx < chartData.notes.Length)
            ? chartData.notes[finishIdx].arriveTime
            : startBeat;

        float beats = Mathf.Max(0f, finishBeat - startBeat);
        int hitsPerBeat = 4; // 기획값으로 설정
        return Mathf.Max(1, Mathf.RoundToInt(beats * hitsPerBeat));
    }

    // 연타 투사체 발사
    private void FireRenTusache()
    {
        GameObject selectedProjectile = projectilePrefabs[2];
        GameObject projectile = Instantiate(selectedProjectile, transform.position, Quaternion.identity);
        renProjectile projScript = projectile.GetComponent<renProjectile>();

        if (playerManager == null) playerManager = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();

        projScript.target = playerManager.transform; // 플레이어를 타겟으로 설정
        projScript.owner = this;   // 소유자로 현재 스트라이커 설정
        projScript.type = 5;

        projScript.moveTimeMultiplier = 60f / bpm;
        projScript.genTimeMultiplier = 60f / bpm;
    }
}
*/
