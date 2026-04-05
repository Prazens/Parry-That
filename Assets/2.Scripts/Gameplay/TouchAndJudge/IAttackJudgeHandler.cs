using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackJudgeHandler<T> : IAttackHandler<T>
{
    public Judgeable GetFirstJudgeable(Dictionary<Direction, Judgeable> judgeables, Touched touch);
    public JudgeType Judge(Judgeable judgeable, Touched touch);
}
