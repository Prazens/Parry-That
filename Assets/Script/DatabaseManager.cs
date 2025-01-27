using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public int[] score;
    public int[] star;
    public static bool isTutorialDone;
    private void Awake()
    {
        if (FindObjectsOfType<DatabaseManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        LoadData();
        LoadTutorialDone();
    }
    private void Start()
    {
        
    }
    public void SaveScoreData()
    {
        // 스테이지 개수만큼 각각 저장
        PlayerPrefs.SetInt("Score1", score[1]);
        // PlayerPrefs.SetInt("Score2", score[2]);
        // PlayerPrefs.SetInt("Score3", score[3]);
    }
    public void SaveStarData()
    {
        // 스테이지 개수만큼 각각 저장
        PlayerPrefs.SetInt("Stage1_Star", star[1]);
        // PlayerPrefs.SetInt("Stage2_Star", star[2]);
        // PlayerPrefs.SetInt("Stage3_Star", star[3]);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("Score1"))
        {
            score[1] = PlayerPrefs.GetInt("Score1");
            star[1] = PlayerPrefs.GetInt("Stage1_Star");
            Debug.Log($"{score[1]} 점 로드");
            Debug.Log($"별 {star[1]}개 로드");
            // score[2] = PlayerPrefs.GetInt("Score2");
            // star[2] = PlayerPrefs.GetInt("Stage2_Star");
            // score[3] = PlayerPrefs.GetInt("Score3");
            // star[3] = PlayerPrefs.GetInt("Stage3_Star");
        }
    }

    public void SaveTutorialDone()
    {
        PlayerPrefs.SetInt("TutorialDone", 1);
    }

    public void LoadTutorialDone()
    {
        isTutorialDone = (PlayerPrefs.GetInt("TutorialDone") == 1);
    }

}
