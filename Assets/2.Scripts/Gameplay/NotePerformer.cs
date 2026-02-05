using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note를 읽고 연주하는 역할.
/// 시간에 따라 판정 생성, 예고 이펙트, 공격 명령 등을 담당.
/// </summary>
public class NotePerformer : MonoBehaviour
{
    private float bpm;

    public float BeatToSec(float beatIndex, float offset = 0)
    => beatIndex * (60f / bpm) + offset;
}
