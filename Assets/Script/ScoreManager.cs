using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public List<GameObject> strikerList_;
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    [SerializeField] public ParriedProjectileManager parriedProjectileManager;
    public ScoreUI scoreUI;
    public int combo = 0;
    public int score = 0;
    public float musicOffset;  // 원본은 PlayerManager에 있음

    public double lastNonMissJudge = 0;
    public bool isHolding = false;

    public Queue<JudgeFormat> judgeQueue = new Queue<JudgeFormat>();

    public int bpm;
    public int[][] judgeDetails = new int[4][];  // 방향별 판정 정보, index 0은 전체 판정 합
    // { 총 노트수(미구현), 늦은 MISS, 늦은 GUARD, 늦은 BOUNCE, 완벽한 PARFECT, 빠른 BOUNCE, 빠른 GUARD } 순서
    // 나중에 방향별 판정 정보가 아니라 스트라이커별 판정 정보로 바꾸어야 함
    // 리스트로 바꾸기

    // 디버깅용
    private string[] judgeStrings = new string[7]
                                    { "공노트", "늦은 MISS", "늦은 GUARD", "늦은 BOUNCE",
                                      "PERFECT", "빠른 BOUNCE", "빠른 GUARD" };

    void Update()
    {
        foreach (JudgeFormat judgeObject in judgeQueue)
        {
            Judge(judgeObject.direction, judgeObject.timing, judgeObject.type);
        }

        if (judgeQueue.Count != 0)
        {
            judgeQueue.Clear();
        }
    }

    // 초기화
    public void Initialize()
    {
        combo = 0;
        score = 0;
        strikerList_ = strikerManager.strikerList;
        isHolding = false;
        for (int i = 0; i < 4; i++)
        {
            judgeDetails[i] = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
        }

        return;
    }

    // 판정 - 입력이 들어왔을 때에 실행
    public void Judge(Direction direction, double touchTimeSec, AttackType type)
    {
        float arriveTime;
        int projectile_type;
        // 홀드 중이 아닐 때의 의미없는 홀드종료
        if (type == AttackType.HoldFinish && !isHolding)
        {
            Debug.Log("홀드종료 판정 무시됨");
            return;
        }

        // 홀드 중인데 다른 판정 입력
        if (type != AttackType.HoldFinish && isHolding)
        {
            Debug.Log("홀드 중의 다른 판정 무시됨");
            return;
        }

        double timeDiff;
        timeDiff = touchTimeSec - lastNonMissJudge;

        // 간접 미스 방지
        if (type == AttackType.Strong && timeDiff < 0.01d)
        {
            Debug.Log("겹치는 강패링 판정 무시됨");
            return;
        }

        StrikerController strikerController = null;
        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;
        
        int tempJudge = -1;

        playerManager.currentDirection = touchDirection;
       
        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerList_)
        {
            strikerController = striker.GetComponent<StrikerController>();

            // 터치 방향과 맞는 방향에서 공격하는 스트라이커라면
            if (strikerController.location == touchDirection && strikerController.projectileQueue.Count != 0)
            {
                arriveTime = strikerController.projectileQueue.Peek().GetComponent<projectile>().arriveTime * 60f / strikerController.bpm;
                projectile_type = strikerController.projectileQueue.Peek().GetComponent<projectile>().type;

                // 시간에 따라 판정
                timeDiff = touchTimeSec - arriveTime - musicOffset;
                //timeDiff = touchTimeSec - projectileNoteData.arriveTime * (60f / strikerController.bpm) - musicOffset;
                
                // 강공격을 약패링으로 처리한 경우
                if (type == AttackType.Normal && (AttackType)projectile_type == AttackType.Strong)
                {
                    tempJudge = -1;
                }

                // 판정 나누기
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
                    tempJudge = -1;
                }

                // 홀드 시작
                if (!isHolding && (AttackType)projectile_type == AttackType.HoldStart && tempJudge >= 1)
                {
                    isHolding = true;
                }

                // 홀드 끝
                else if (isHolding && type == AttackType.HoldFinish)
                {
                    isHolding = false;

                    // 홀드 아직 남았는데 입력 종료시
                    if (tempJudge == -1)
                    {
                        tempJudge = 0;
                    }
                }
                // 홀드 시간이 지났는데도 놓지 않는 경우는 미구현
                // 홀드 관련 테스트 안해봄 - 버그가 있을 수 있음

                lastNonMissJudge = touchTimeSec;
                // Debug.Log("노트를 갖고 있고 같은 방향의 Striker를 찾았습니다");
                break;
            }
            else
            {
                strikerController = null;
            }
        }

        // 판정 전송
        Debug.Log($"판정 수행 : Direction.{direction}, AttackType.{type}, {timeDiff:F3} -> \"{judgeStrings[tempJudge + 1]}\"");
        JudgeManage(touchDirection, tempJudge, type, strikerController);

        return;
    }

    // 판정 결과를 이용해 결과에 맞는 행동 수행 : 스코어, SFX, ...
    public void JudgeManage(Direction direction, int judgement, AttackType type,
                            StrikerController strikerController, bool isPassing = false)
    {
        // 플레이어가 조작하지 않거나 조작이 무시되는 경우
        if (!isPassing)
        {
            playerManager.Operate(direction, type);
        }

        // 노트가 처리되지 않은 경우
        if (judgement == -1)
        {
            lastNonMissJudge = 0;
            return;
        }

        // index로 한번에 처리
        judgeDetails[0][judgement + 1] += 1;
        judgeDetails[(int)direction][judgement + 1] += 1;

        // 특정 Striker 찾기
        StrikerController targetStriker = strikerController;

        // GameObject targetProjectile = targetStriker.projectileQueue.Peek();

        CameraMoving CameraEffect = GameObject.Find("Main Camera").GetComponent<CameraMoving>();

        // 따로 처리
        switch (judgement)
        {
            case 0:  // 늦은 BAD (MISS)
                score += 0;
                combo = 0;

                // 피격당한 후 죽었을 때
                if (--playerManager.hp == 0)
                {
                    scoreUI.HideAll();
                    playerManager.GameOver();
                }
                // 피격당한 후 죽지 않았을 때
                else
                {
                    Debug.Log(playerManager.hp);
                    UIManager.Instance.ShowDamageOverlayEffect();
                    CameraEffect.CameraShake();
                }

                break;

            case 1:  // 늦은 BLOCKED
                score += 300;
                combo = 0;
                break;

            case 2:  // 늦은 PARRIED
                score += 9000;
                combo += 1;
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction, false);
                break;

            case 3:  // 완벽한 PERFECT
                score += 30000;
                combo += 1;
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction, true);
                break;

            case 4:  // 빠른 PARRIED
                score += 9000;
                combo += 1;
                targetStriker?.TakeDamage(1);
                UIManager.Instance.ShowParticle(direction, false);
                break;

            case 5:  // 빠른 BLOCKED
                score += 300;
                combo = 0;
                break;
        }

        if (judgement != 0)
        {
            scoreUI.DisplayScore(score);
            
            if (judgement != 1 && judgement != 5)
            {
                // parriedProjectileManager.CreateParriedProjectile(targetProjectile.transform.position, direction);
                if (parriedProjectileManager != null)
                {
                    parriedProjectileManager.ParryTusache(direction, (int)type);
                }
                else
                {
                    Debug.Log("패링투사체 못찾음");
                }
            }
        }
        else
        {
            scoreUI.DisplayHP(playerManager.hp);
        }

        scoreUI.DisplayJudge(judgement, direction);

        // 대상 노트 제거
        Destroy(strikerController.projectileQueue.Dequeue());
        // Destroy(targetProjectile);

        return;
    }
}
