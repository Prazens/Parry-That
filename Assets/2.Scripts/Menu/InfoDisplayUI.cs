using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoDisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject[] stars;
    [SerializeField] private TextMeshProUGUI txtStageName;
    [SerializeField] private TextMeshProUGUI txtStageScore;
    [SerializeField] private GameObject fireEffect;

    public void InitUI(int[] stageIndex)
    {

        // 위치 재정렬

        // 정보 초기화
        DisplayInfo(stageIndex);
    }

    public void DisplayInfo(int[] stageIndex)
    {
        // 튜토리얼과 에필로그에는 필요한 정보만 출력
        if (stageIndex[0] == 0)
        {
            transform.GetChild(2).gameObject.SetActive(false);
            fireEffect.SetActive(false);
            txtStageName.text = StageDBManager.Instance.StageName[stageIndex[0]];
            return;
        }
        transform.GetChild(2).gameObject.SetActive(true);

        if (stageIndex[1] == 1)
        {
            fireEffect.SetActive(true);
        }
        else
        {
            fireEffect.SetActive(false);
        }

        // 상세 정보 표시
        txtStageName.text = StageDBManager.Instance.StageName[stageIndex[0]];
        txtStageScore.text = string.Format("{0:#,##0}", StageDBManager.Instance.highScores[stageIndex[0]][stageIndex[1]]);
        for (int i = 0; i < 3; i++)
        {
            if (i >= StageDBManager.Instance.starRatings[stageIndex[0]][stageIndex[1]])
            {
                stars[i].SetActive(false);
            }
            else
            {
                stars[i].SetActive(true);
            }
        }
        if (StageDBManager.Instance.starRatings[stageIndex[0]][stageIndex[1]] == 4)
        {
            Debug.Log("잘못된 별 개수");
        }
    }
}
