using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public List<GameObject> strikerList_;
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    public ParriedProjectileManager parriedProjectileManager;
    // public SoundManager soundManager;
    public ScoreUI scoreUI;
    public int combo = 0;
    public int score = 0;

    public int bpm;
    public int[][] judgeDetails = new int[4][];  // 방향별 판정 정보, index 0은 전체 판정 합
    // { 늦은 MISS, 늦은 GUARD, 늦은 BOUNCE, 완벽한 PARFECT, 빠른 BOUNCE, 빠른 GUARD } 순서
    // 나중에 방향별 판정 정보가 아니라 스트라이커별 판정 정보로 바꾸어야 함

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 초기화
    public void Initialize()
    {
        combo = 0;
        score = 0;
        strikerList_ = strikerManager.strikerList;
        for (int i = 0; i < 4; i++)
        {
            judgeDetails[i] = new int[6] { 0, 0, 0, 0, 0, 0 };
        }
    }

    // 판정
    public void Judge(Direction direction, double touchTimeSec, int type)
    {
        StrikerController strikerController;
        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;
        
        NoteData projectileNoteData;
        double timeDiff;

        int tempJudge;

        playerManager.currentDirection = touchDirection;

        // Vector3 projectileLocation;
        // double distance;

        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerList_)
        {
            strikerController = striker.GetComponent<StrikerController>();

            // 터치 방향과 맞는 방향에서 공격하는 스트라이커라면
            if (strikerController.location == touchDirection && strikerController.projectileQueue.Count != 0)
            {
                projectileNoteData = strikerController.projectileQueue.Peek().GetComponent<projectile>().noteData;

                // 시간에 따라 판정
                timeDiff = touchTimeSec - projectileNoteData.arriveTime - 2d;
                
                // 강공격인데 스와이프로 처리하지 못한 경우
                if (projectileNoteData.type == 1 && type == 0)
                {
                    tempJudge = 0;
                }

                // 기획서의 판정 표와 반대 순서임
                else if (timeDiff > 0.12d)
                {
                    tempJudge = 0;
                }
                else if (timeDiff > 0.09d)
                {
                    tempJudge = 1;
                }
                else if (timeDiff > 0.05d)
                {
                    tempJudge = 2;
                }
                else if (timeDiff >= -0.05d)
                {
                    tempJudge = 3;
                }
                else if (timeDiff >= -0.09d)
                {
                    tempJudge = 4;
                }
                else if (timeDiff >= -0.12d)
                {
                    tempJudge = 5;
                }
                else  // 공노트? 공POOR?
                {
                    return;
                }

                JudgeManage(direction, tempJudge, type, strikerController);

                Debug.Log("판정 수행됨");
                // Debug.Log($"{touchTimeSec} - {projectileNoteData.arriveTime * (60d / strikerController.bpm) - 2d} = {touchTimeSec - projectileNoteData.arriveTime * (60d / strikerController.bpm) - 2d}");
                Destroy(strikerController.projectileQueue.Dequeue());
            }
        }
    }

    // 판정 결과를 이용해 결과에 맞는 행동 수행 : 스코어, SFX, ...
    public void JudgeManage(Direction direction, int judgement, int type, StrikerController strikerController)
    {
        // index로 한번에 처리하는 것들
        judgeDetails[0][judgement] += 1;
        judgeDetails[(int)direction][judgement] += 1;

        // 특정 Striker 찾기
        StrikerController targetStriker = strikerController;

        GameObject targetProjectile = targetStriker.projectileQueue.Peek();

        // 따로 처리하는 것들
        switch (judgement)
        {
            case 0:  // 늦은 MISS
                score += 0;
                combo = 0;
                Debug.Log("HIT (MISS)");

                // 피격당한 후 죽었을 때
                if (--playerManager.hp == 0)
                {
                    playerManager.GameOver();
                }
                // 피격당한 후 죽지 않았을 때
                else
                {
                    Debug.Log(playerManager.hp);
                    UIManager.Instance.ShowDamageOverlayEffect();
                }

                break;

            case 1:  // 늦은 GUARD
                score += 300;
                combo = 0;
                Debug.Log("GUARD (LATE)");
                break;

            case 2:  // 늦은 BOUNCE
                score += 9000;
                combo += 1;
                Debug.Log("BOUNCE! (LATE)");
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction);
                break;

            case 3:  // 완벽한 PARFECT
                score += 30000;
                combo += 1;
                Debug.Log("PARFECT!!");
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction);
                break;

            case 4:  // 빠른 BOUNCE
                score += 9000;
                combo += 1;
                Debug.Log("BOUNCE! (FAST)");
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction);
                break;

            case 5:  // 빠른 GUARD
                score += 300;
                combo = 0;
                Debug.Log("GUARD (FAST)");
                break;
        }

        if (judgement != 0)
        {
            scoreUI.DisplayScore(score);
            
            if (judgement != 1 && judgement != 5)
            {
                parriedProjectileManager.CreateParriedProjectile(targetProjectile.transform.position, direction);
            }
        }
        else
        {
            scoreUI.DisplayHP(playerManager.hp);
        }

        scoreUI.DisplayJudge(judgement, direction);

        // Destroy(targetProjectile);
    }
}
