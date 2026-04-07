using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackNoticeContext : IAttackContext
{
    public NoteData note { get; }
    public List<Judgeable> judgeables = new();

    public AttackNoticeContext(NoteData note, List<Judgeable> judgeables)
    {
        this.note = note;
        this.judgeables = judgeables;
    }
}
