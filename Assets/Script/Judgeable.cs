using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Judgeable
{
    // Judgeable을 생성한 스트라이커
    public StrikerController strikerController;

    // 투사체 등 판정과 연동되는 GameObject
    public GameObject judgeableObject;

    // 판정 끝날 시 실행할 메소드(매개변수와 반환값 모두 없음)
    private Action onDestroy;

    public Direction noteDirection;
    public AttackType attackType;
    public float arriveBeat;
    // public NoteData noteData;

    public Judgeable(AttackType _attackType, float _arriveBeat, Direction _noteDirection, StrikerController _strikerController, GameObject _judgeableObject = null, Action _onDestroy = null)
    {
        noteDirection = _noteDirection;
        judgeableObject = _judgeableObject;
        attackType = _attackType;
        arriveBeat = _arriveBeat;
        strikerController = _strikerController;
        onDestroy = _onDestroy;
    }

    public void FinishJudge()
    {
        if (strikerController.judgeableQueue.Peek() == this)
        {
            strikerController.judgeableQueue.Dequeue();
            if (judgeableObject != null)
            {
                GameObject.Destroy(judgeableObject);
            }

            if (onDestroy != null)
            {
                onDestroy();
            }
        }
    }
}
