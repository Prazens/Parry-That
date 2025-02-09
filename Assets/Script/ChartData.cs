using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct NoteData
{
    public float time; // 발사 시간 (박자 단위)
    public float arriveTime; // 도착 시간 (박자 단위)

    public int type;
}

[CreateAssetMenu(fileName = "NewChart", menuName = "Chart Data")]
[Serializable]
public class ChartData
{
    public float bpm;
    public int strikerType;
    public int direction;
    public float appearTime; // 등장 시작 시간 (박자 단위)
    public float disappearTime; // 퇴장 시작 시간 (박자 단위)
    public NoteData[] notes; // 채보 데이터 리스트
}

public class JsonReader
{
    static public T ReadJson<T>(TextAsset jsonFile)
    {
        // JSON 파일을 읽어오기
        string json = jsonFile.text;

        // JSON 데이터를 객체로 변환
        return JsonUtility.FromJson<T>(json);
    }
}
