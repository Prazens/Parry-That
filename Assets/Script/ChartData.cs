using System.IO;
using System;
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
[Serializable]
public class ChartData
{
    public NoteData[] notes; // 채보 데이터 리스트
}

public class JsonReader
{
    static public ChartData ReadJson(TextAsset jsonFile)
    {
        // JSON 파일을 읽어오기
        string json = jsonFile.text;

        // JSON 데이터를 MyDataList 객체로 변환
        return JsonUtility.FromJson<ChartData>(json);
    }
}
