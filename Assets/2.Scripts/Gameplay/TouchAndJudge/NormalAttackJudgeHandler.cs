using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalAttackJudgeContext : IAttackContext
{
    public NoteData note { get; }
    public Direction direction;
    public Judgeable judgeable;

    public NormalAttackJudgeContext(NoteData note, Direction direction)
    {
        this.note = note;
        this.direction = direction;
    }
}

public class NormalAttackJudgeHandler : MonoBehaviour, IAttackJudgeHandler<NormalAttackJudgeContext>
{
    [SerializeField] private JudgeSystem judgeSystem;
    private AttackType[] relevantAttacks = new AttackType[1] { AttackType.Normal };
    private AttackType[] relevantTouches = new AttackType[2] { AttackType.Normal, AttackType.Strong };

    public void OnNotice(NormalAttackJudgeContext context)
    {
        NoteData note = context.note;
        Judgeable judgeable = new Judgeable((AttackType)note.type, note.arriveBeat, context.direction);
        judgeSystem.EnqueueJudgeable(judgeable);
        context.judgeable = judgeable;
    }

    void IAttackHandler.OnNotice(IAttackContext context)
        => OnNotice((NormalAttackJudgeContext)context);

    public void OnAttackStart(NormalAttackJudgeContext context)
    {

    }

    void IAttackHandler.OnAttackStart(IAttackContext context)
        => OnAttackStart((NormalAttackJudgeContext)context);

    public void OnJudge(JudgeContext context)
    {

    }

    public Judgeable GetFirstJudgeable(Dictionary<Direction, Judgeable> judgeables, Touched touch)
    {
        AttackType touchType = touch.type;

        // 관련 없는 터치이면 무시
        if (!relevantTouches.Contains(touchType))
            return null;

        Judgeable judgeable = null;
        float arriveBeat = Mathf.Infinity;

        // 모든 방향 중 가장 빠른 약공격 탐색
        foreach (var tempJudgeable in judgeables.Values)
        {
            if (tempJudgeable != null && relevantAttacks.Contains(tempJudgeable.attackType) && tempJudgeable.arriveBeat < arriveBeat)
            {
                judgeable = tempJudgeable;
                arriveBeat = tempJudgeable.arriveBeat;
            }
        }

        return judgeable;
    }

    public JudgeType Judge(Judgeable judgeable, Touched touch)
    {
        double touchSec = touch.touchSec;
        AttackType touchType = touch.type;

        // 관련 없는 터치이면 무시
        if (!relevantTouches.Contains(touchType))
            return JudgeType.None;

        float arriveSec = StageFlowManager.Instance.BeatToSec(judgeable.arriveBeat);
        JudgeType judgeType = judgeSystem.GetJudgeType(touchSec, arriveSec);

        // EarlyMiss는 무시
        if (judgeType == JudgeType.EarlyMiss)
            judgeType = JudgeType.None;

        // 약공격에 강패링하면 Blocked 처리
        if (touchType == AttackType.Strong)
        {
            if (judgeType >= JudgeType.LateBlocked && judgeType <= JudgeType.EarlyBlocked)
                judgeType = JudgeType.EarlyBlocked;
        }

        // 플레이어가 자동으로 공격 방향을 바라봄
        if (judgeType >= JudgeType.LateBlocked && judgeType <= JudgeType.EarlyBlocked)
        {
            touch.direction = judgeable.noteDirection;
        }

        return judgeType;
    }
}
