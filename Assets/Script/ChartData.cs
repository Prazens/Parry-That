using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoteData
{
    public float time; // 발사 시간
    public float arriveTime; // 도착 시간

    public int type;
}

[CreateAssetMenu(fileName = "NewChart", menuName = "Chart Data")]
public class ChartData : ScriptableObject
{
    public List<NoteData> notes; // 채보 데이터 리스트
}
