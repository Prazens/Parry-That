using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoDisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject[] Stars;
    private TextMeshProUGUI txtStageName;
    private TextMeshProUGUI txtStageScore;

    public void InitUI(int[] stageIndex)
    {
        txtStageName = transform.Find("StageName").GetComponent<TextMeshProUGUI>();
        txtStageScore = transform.Find("StageScore").GetComponent<TextMeshProUGUI>();

        // 위치 재정렬

        // 정보 초기화
        DisplayInfo(stageIndex);
    }

    public void DisplayInfo(int[] stageIndex)
    {
        // 상세 정보 표시
        txtStageName.text = StageDBManager.Instance.StageName[stageIndex[0]];
        txtStageScore.text = string.Format("{0:#,##0}", StageDBManager.Instance.highScores[stageIndex[0], stageIndex[1]]);
        for (int i = 0; i < 4; i++)
        {
            if (i != StageDBManager.Instance.starRatings[stageIndex[0], stageIndex[1]])
            {
                Stars[i].SetActive(false);
            }
            else
            {
                Stars[i].SetActive(true);
            }
        }
        if (StageDBManager.Instance.starRatings[stageIndex[0], stageIndex[1]] >= 4)
        {
            Stars[3].SetActive(true);
        }
    }
}
