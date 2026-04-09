using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateLogger : StateMachineBehaviour
{
    [SerializeField] private string stateName;
    // 상태에 진입할 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"<color=green>[Animator]</color> Enter State: <b>{stateName}</b> on {animator.gameObject.name}");
    }
    // 상태에서 빠져나올 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"<color=red>[Animator]</color> Exit State: <b>{stateName}</b> on {animator.gameObject.name}");
    }
}
