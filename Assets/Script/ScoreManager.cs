using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public List<GameObject> strikerList_;
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    public int combo = 0;
    public int score = 0;
    public int[][] judgeDetails;  // 방향별 판정 정보, index 0은 전체 판정 합
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
        for (int i = 0; i < 5; i++)
        {
            judgeDetails[i] = new int[] { 0, 0, 0, 0, 0, 0 };
        }
    }

    // 판정
    public void Judge(Direction direction, float touchTime)
    {
        StrikerController strikerController;
        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;
        
        NoteData projectileNoteData;
        float timeDiff;

        // Vector3 projectileLocation;
        // double distance;

        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerList_)
        {
            strikerController = striker.GetComponent<StrikerController>();

            // 터치 방향과 맞는 방향에서 공격하는 스트라이커라면
            if (strikerController.location == touchDirection)
            {
                projectileNoteData = strikerController.projectileQueue.Peek().GetComponent<projectile>().noteData;

                // 시간에 따라 판정
                timeDiff = touchTime - projectileNoteData.arriveTime;
                
                // 강공격인데 스와이프로 처리하지 못한 경우
                if (projectileNoteData.type == 1 && direction != Direction.None)
                {
                    JudgeManage(direction, 0);
                }

                // 기획서의 판정 표와 반대 순서임
                else if (timeDiff > 0.12d)
                {
                    JudgeManage(direction, 0);
                }
                else if (timeDiff > 0.9d)
                {
                    JudgeManage(direction, 1);
                }
                else if (timeDiff > 0.5d)
                {
                    JudgeManage(direction, 2);
                }
                else if (timeDiff >= -0.5d)
                {
                    JudgeManage(direction, 3);
                }
                else if (timeDiff >= -0.9d)
                {
                    JudgeManage(direction, 4);
                }
                else if (timeDiff >= -0.12d)
                {
                    JudgeManage(direction, 5);
                }
                else  // 공노트? 공POOR?
                {
                    return;
                }

                // 거리에 따라 판정 : 폐기
                // projectileLocation = strikerController.projectileQueue.Peek().transform.position;
                // distance = Math.Abs(projectileLocation.x + projectileLocation.y);
                // 
                // if (distance >= 1.5d)
                // {
                //     return;
                // }
                // else if (distance >= 0.9d)
                // {
                //     Debug.Log("Fast");
                // }
                // else if (distance >= 0.3d)
                // {
                //     Debug.Log("Perfect");
                // }
                // else
                // {
                //     Debug.Log("Late");
                // }

                Destroy(strikerController.projectileQueue.Dequeue());
            }
        }
    }

    // 판정 결과를 이용해 결과에 맞는 행동 수행 : 스코어, SFX, ...
    public void JudgeManage(Direction direction, int judgement)
    {
        // index로 한번에 처리하는 것들
        judgeDetails[0][judgement] += 1;
        judgeDetails[(int)direction][judgement] += 1;

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

                }
                // 피격당한 후 죽지 않았을 때
                else
                {

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
                break;
            
            case 3:  // 완벽한 PARFECT
                score += 30000;
                combo += 1;
                Debug.Log("PARFECT!!");
                break;
            
            case 4:  // 빠른 BOUNCE
                score += 9000;
                combo += 1;
                Debug.Log("BOUNCE! (FAST)");
                break;
            
            case 5:  // 빠른 GUARD
                score += 300;
                combo = 0;
                Debug.Log("GUARD (FAST)");
                break;
        }
    }
}
