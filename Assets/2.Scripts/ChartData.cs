using System;
using UnityEngine;

[System.Serializable]
public class StrikerData
{
    public int strikerType;
    public int direction;
    public float appearTime;     // 박자 단위
    public float disappearTime;  // 박자 단위
}

[System.Serializable]
public class NoteData
{
    public int strikerIndex;

    public float noticeBeat; // 예고 시간 (박자 단위)
    public float arriveBeat; // 도착 시간 (박자 단위)

    public int type;
}

[Serializable]
public class ChartData
{
    public float bpm;
    public StrikerData[] strikers;
    public NoteData[] notes;
}

public class JsonReader
{
    public static T ReadJson<T>(TextAsset jsonFile)
    {
        string json = jsonFile.text;
        return JsonUtility.FromJson<T>(json);
    }
}