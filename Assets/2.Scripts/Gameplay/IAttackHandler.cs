using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackHandler<ContextType>
{
    public void OnNotice(ContextType context);
    public void OnAttackStart(ContextType context);
    public void OnJudge(JudgeContext context);
}
