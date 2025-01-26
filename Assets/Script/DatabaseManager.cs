using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public int[] score;

    private void Awake()
    {
        if (FindObjectsOfType<DatabaseManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LoadScore();
    }
    public void SaveScore()
    {
        // 스테이지 개수만큼 각각 저장
        PlayerPrefs.SetInt("Score1", score[1]);
        // PlayerPrefs.SetInt("Score2", score[2]);
        // PlayerPrefs.SetInt("Score3", score[3]);
    }

    public void LoadScore()
    {
        if (PlayerPrefs.HasKey("Score1"))
        {
            score[1] = PlayerPrefs.GetInt("Score1");
            Debug.Log($"{score[1]} 점 로드");
            // score[2] = PlayerPrefs.GetInt("Score2");
            // score[3] = PlayerPrefs.GetInt("Score3");
        }
    }

}
