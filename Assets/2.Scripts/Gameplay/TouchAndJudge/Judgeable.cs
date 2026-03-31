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

    public Direction noteDirection;
    public AttackType attackType;
    public float arriveBeat;
    public float nextArriveBeat; // 다음 노트의 도착 박자 (없으면 -1)
    public int streamCount = -1; // 몇 번 입력해야 하는지 (stream 판정용)
    // public NoteData noteData;

    public Judgeable(AttackType _attackType, float _arriveBeat, float _nextArriveBeat, Direction _noteDirection, StrikerController _strikerController, GameObject _judgeableObject = null)
    {
        attackType = _attackType;
        arriveBeat = _arriveBeat;
        nextArriveBeat = _nextArriveBeat;
        noteDirection = _noteDirection;
        strikerController = _strikerController;
        judgeableObject = _judgeableObject;
    }

    public void SetStreamCount(int _count)
    {
        streamCount = _count;
    }
}
