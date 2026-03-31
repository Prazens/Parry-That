using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "스테이지 종료 시점" 결과 처리 전담.
/// - 별 개수 계산(클리어 기준)
/// - 최고 점수 갱신 판단
/// - DatabaseManager 저장 호출(SaveScoreData, SaveStarData)
///
/// StageFlowManager가 호출하는 함수명은 지금 코드 기준으로:
/// - ProcessClearResult()
/// - ProcessGameOverResult()
/// </summary>
public class StageResultManager : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private JudgeSystem judgeSystem; // ScoreManager 삭제 예정이므로 JudgeSystem에서 읽는 전제

    public int LatestStarCount { get; private set; } = 0;
    public int LatestScore { get; private set; } = 0;
    public List<int[]> LatestJudgeDetails { get; private set; } = null;

    public void ProcessClearResult()
    {
        PullLatestFromJudgeSystem();

        LatestStarCount = CalculateStarCount();

        SaveBestScoreIfNeeded();
        SaveStarsIfNeeded();
    }

    public void ProcessPauseResult()
    {
        PullLatestFromJudgeSystem();
    }

    public void ProcessGameOverResult()
    {
        PullLatestFromJudgeSystem();

        // GameOver에서는 별 표시를 끄는 방식(기존 UpdateStar_Over)이라 별 계산/저장 안 함.
        // 다만, "최고 점수 갱신"은 GameOver에서 하던 로직이므로 저장은 진행.
        SaveBestScoreIfNeeded();

        LatestStarCount = 0;
    }

    // -------------------------
    // 내부
    // -------------------------

    private void PullLatestFromJudgeSystem()
    {
        if (judgeSystem == null)
        {
            // 연결 안 되어 있으면 씬에서 찾아보기(기존 StageManager도 FindObjectOfType 자주 사용)
            judgeSystem = FindObjectOfType<JudgeSystem>();
        }

        if (judgeSystem != null)
        {
            LatestScore = judgeSystem.score;

            if (judgeSystem.judgeDetails != null)
            {
                // List<int[]> 참조 그대로 들고 있으면 외부에서 바뀔 수 있으니, 최소한 리스트만 새로 잡아둠
                LatestJudgeDetails = new List<int[]>(judgeSystem.judgeDetails);
            }
            else
            {
                LatestJudgeDetails = null;
            }
        }
        else
        {
            LatestScore = 0;
            LatestJudgeDetails = null;
        }
    }

    private int CalculateStarCount()
    {
        // 기존 StageManager.UpdateStar_Clear 의 조건을 그대로 유지하되,
        // "클리어면 1개는 기본"으로 잡아야 UI/저장 모두 자연스러움.
        int currentStars = 1;

        if (LatestJudgeDetails == null || LatestJudgeDetails.Count == 0) return currentStars;

        int[] details = LatestJudgeDetails[0];
        if (details == null || details.Length < 8) return currentStars;

        // 2개 조건: score >= judgeDetails[0][0] * 30000 * (2/3)
        // 기존 코드에선 (2/3)이 int 나눗셈이라 0이 되는 위험이 있었는데,
        // 의도는 2/3로 보이므로 float로 계산.
        float thresholdScore = details[0] * 30000f * (2f / 3f);
        if (LatestScore >= thresholdScore)
        {
            currentStars = 2;
        }

        // 3개 조건: judgeDetails[0][1] == 0 (기존 코드 그대로)
        if (details[1] == 0)
        {
            currentStars = 3;
        }

        return currentStars;
    }

    private void SaveBestScoreIfNeeded()
    {
        DatabaseManager theDatabase = FindObjectOfType<DatabaseManager>();
        if (theDatabase == null) return;

        int stageIndex = SceneLinkage.StageLV;

        if (LatestScore > theDatabase.score[stageIndex])
        {
            theDatabase.score[stageIndex] = LatestScore;
            theDatabase.SaveScoreData();
        }
    }

    private void SaveStarsIfNeeded()
    {
        DatabaseManager theDatabase = FindObjectOfType<DatabaseManager>();
        if (theDatabase == null) return;

        int stageIndex = SceneLinkage.StageLV;

        if (LatestStarCount > theDatabase.star[stageIndex])
        {
            theDatabase.star[stageIndex] = LatestStarCount;
            theDatabase.SaveStarData();
        }
    }
}
