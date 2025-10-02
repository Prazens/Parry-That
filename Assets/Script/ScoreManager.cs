using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ScoreManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    [SerializeField] public ParriedProjectileManager parriedProjectileManager;
    public ScoreUI scoreUI;
    public int combo = 0;
    public int score = 0;
    public float musicOffset;

    public double lastNonMissJudge = 0;
    public bool isHolding = false;
    public bool isOnStream = false;  // 연타 중인지 확인
    public int streamCount = -1;  // 연타한 횟수
    public Judgeable streamJudgeable = null; // 연타 판정용 Judgeable 저장

    public Queue<JudgeFormat> judgeQueue = new Queue<JudgeFormat>();

    public int bpm;
    public List<int[]> judgeDetails = new List<int[]>();  // 스트라이커별 판정 정보, index 0은 전체 판정 합
    // { 총 노트수, 늦은 MISS, 늦은 GUARD, 늦은 BOUNCE, 완벽한 PARFECT, 빠른 BOUNCE, 빠른 GUARD } 순서

    // 디버깅용
    private string[] judgeStrings = new string[7]
                                    { "공노트", "늦은 MISS", "늦은 GUARD", "늦은 BOUNCE",
                                      "PERFECT", "빠른 BOUNCE", "빠른 GUARD" };

    private StrikerController tempStrikerController = null;
    private Judgeable tempJudgeable;

    private void Start()
    {
        musicOffset = PlayerPrefs.GetFloat("musicOffset", 2);
    }

    void Update()
    {
        if(!StageManager.isActive) return;
        // 각 선두 노트에 대해 늦은 MISS가 발생 가능한지 확인
        // 그리고 stream의 시작/끝 확인
        foreach (GameObject striker in strikerManager.strikerList)
        {
            if (striker != null)
            {
                tempStrikerController = striker.GetComponent<StrikerController>();

                if (tempStrikerController.judgeableQueue.Count != 0)
                {
                    tempJudgeable = tempStrikerController.judgeableQueue.Peek();

                    float tempTimeDiff = StageManager.Instance.currentTime - tempJudgeable.arriveBeat * 60f / tempStrikerController.bpm - musicOffset;

                    // 연타 모드 시작
                    if (tempJudgeable.attackType == AttackType.StreamStart && !isOnStream && tempTimeDiff > -0.01d)
                    {
                        isOnStream = true;
                        streamCount = 0;
                        streamJudgeable = tempStrikerController.judgeableQueue.Dequeue();
                        break;
                    }

                    // 연타 모드 종료
                    else if (tempJudgeable.attackType == AttackType.StreamFinish)
                    {
                        // 연타 시간이 끝났을 경우
                        if (tempTimeDiff > -0.01d)
                        {
                            // 연타 종료, 연타 수에 따른 판정 시행
                            if (streamCount < streamJudgeable.streamCount * 1 / 2)
                            {
                                // Debug.Log("연타 횟수 부족으로 MISS 처리");
                                JudgeManage(streamJudgeable, 0);
                            }
                            else if (streamCount < streamJudgeable.streamCount * 3 / 4)
                            {
                                JudgeManage(streamJudgeable, 1);
                            }
                            else if (streamCount < streamJudgeable.streamCount)
                            {
                                JudgeManage(streamJudgeable, 2);
                            }
                            else
                            {
                                // 연타 횟수가 정해진 횟수 이상
                                JudgeManage(streamJudgeable, 3);
                            }

                            isOnStream = false;
                            streamCount = -1;
                            streamJudgeable = null;
                            tempStrikerController.judgeableQueue.Dequeue();
                            break;
                        }
                    }

                    if (tempTimeDiff > 0.12d)
                    {
                        if (tempJudgeable.attackType == AttackType.HoldStart)
                        {
                            // Debug.Log($"무조작 판정 : Direction.{tempJudgeable.noteDirection}, AttackType.{tempJudgeable.attackType}, {tempTimeDiff:F3} -> \"{judgeStrings[1]}\"");
                            JudgeManage(tempJudgeable, 0, true);
                            tempJudgeable = tempStrikerController.judgeableQueue.Peek();
                            tempTimeDiff = StageManager.Instance.currentTime - tempJudgeable.arriveBeat * 60f / tempStrikerController.bpm - musicOffset;
                        }
                        else if (tempJudgeable.attackType == AttackType.HoldFinishStrong)
                        {
                            isHolding = false;
                        }
                        // Debug.Log($"무조작 판정 : Direction.{tempJudgeable.noteDirection}, AttackType.{tempJudgeable.attackType}, {tempTimeDiff:F3} -> \"{judgeStrings[1]}\"");
                        JudgeManage(tempJudgeable, 0, true);
                    }
                }
            }
        }

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
        isHolding = false;
        judgeDetails = new List<int[]>();

        // Debug.Log($"strikerManager.charts 의 길이:{strikerManager.charts.Count}");
        for (int i = 0; i < strikerManager.charts.Count + 1; i++)
        {
            if (i == 0)
            {
                judgeDetails.Add(new int[7] { 0, 0, 0, 0, 0, 0, 0 });
                foreach (ChartData chart in strikerManager.charts)
                {
                    judgeDetails[0][0] += chart.notes.Length;
                }
            }
            else
            {
                judgeDetails.Add(new int[7] { strikerManager.charts[i - 1].notes.Length, 0, 0, 0, 0, 0, 0 });
            }
        }

        return;
    }

    // 판정 - 입력이 들어왔을 때에 실행
    // StrikerController의 judgeableQueue에 있는 노트 데이터를 각 StrikerController마다
    // 맨 앞 거를 꺼내서 비교 후 처리.
    public void Judge(Direction direction, double touchTimeSec, AttackType type)
    {

        if (isOnStream)
        {
            if (streamCount != -1)
            {
                streamCount++;
                return;
            }
            else
            {
                Debug.LogError("stream 중이 아닌 isOnStream");
                return;
            }
        }

        // 홀드 중이 아닐 때의 의미없는 홀드정지
        if (type == AttackType.HoldStop && !isHolding)
        {
            // Debug.Log("홀드정지 판정 무시됨");
            return;
        }

        bool findHoldFinish = false;
        print(isHolding);
        // 홀드 중인데 다른 판정 입력
        // if (type != AttackType.HoldFinish && isHolding)
        if (isHolding)
        {
            if (!(type == AttackType.HoldStop))
            {
                // Debug.Log("홀드 중에서 스와이프 포함한 다른 판정 무시됨");
                return;
            }
            // 홀드 중일 때의 스와이프는 홀드 종료로 판정하지 않음
            else if (type == AttackType.HoldStop)
            {
                findHoldFinish = true;
            }
        }

        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;
        Judgeable _judgeable = null;
        float arriveSec;
        double timeDiff;
        timeDiff = touchTimeSec - lastNonMissJudge;

        // 간접 미스 방지
        if (type == AttackType.Strong && timeDiff < 0.01d)
        {
            // Debug.Log("겹치는 강패링 판정 무시됨");
            return;
        }

        int tempJudge = -1;

        playerManager.currentDirection = touchDirection;
        // Debug.LogWarning(playerManager.currentDirection);
        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerManager.strikerList)
        {
            if (striker != null)
            {
                tempStrikerController = striker.GetComponent<StrikerController>();

                // 터치 방향과 맞는 방향에서 공격하는 스트라이커라면
                if (tempStrikerController.location == touchDirection
                    && tempStrikerController.judgeableQueue.Count != 0)
                {
                    _judgeable = tempStrikerController.judgeableQueue.Peek();

                    // 홀드 틀렸을 경우
                    // if (findHoldFinish && _judgeable.attackType == AttackType.HoldFinishStrong
                    //     && (tempStrikerController.location != touchDirection || type == AttackType.HoldStop))
                    // {
                    //     // Debug.Log("홀드 틀림");
                    //     isHolding = false;
                    //     tempJudge = 0;
                    //     break;
                    // }

                    arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;

                    // 시간에 따라 판정
                    timeDiff = touchTimeSec - arriveSec - musicOffset;
                    //timeDiff = touchTimeSec - projectileNoteData.arriveTime * (60f / strikerController.bpm) - musicOffset;
                    
                    // 강공격을 약패링으로 처리한 경우
                    if (type == AttackType.Normal && _judgeable.attackType == AttackType.Strong)
                    {
                        tempJudge = -1;
                    }

                    // 판정 나누기
                    // 기획서의 판정 표와 반대 순서임
                    else if (timeDiff > 0.12d)
                    {
                        tempJudge = 0;
                    }
                    else if (timeDiff > 0.12d)
                    {
                        tempJudge = 1;
                    }
                    else if (timeDiff > 0.07d)
                    {
                        tempJudge = 2;
                    }
                    else if (timeDiff >= -0.07d)
                    {
                        tempJudge = 3;
                    }
                    else if (timeDiff >= -0.12d)
                    {
                        tempJudge = 4;
                    }
                    else if (timeDiff >= -0.15d)
                    {
                        tempJudge = 5;
                    }
                    else if (timeDiff >= -0.18d)
                    {
                        tempJudge = 0;
                    }

                    else  // 공노트? 공POOR?
                    {
                        tempJudge = -1;
                    }

                    // 홀드 시작
                    if (!isHolding && _judgeable.attackType == AttackType.HoldStart)
                    {
                        if (tempJudge >= 1)
                        {
                            // Debug.Log("홀드 시작");
                            isHolding = true;
                            type = AttackType.HoldStart;
                        }

                        if (tempJudge == 0)
                        {
                            // Debug.Log($"판정 수행 : Direction.{direction}, AttackType.{type}, {timeDiff:F3} -> \"{judgeStrings[tempJudge + 1]}\"");
                            JudgeManage(_judgeable, tempJudge, false, touchDirection, type);
                            _judgeable = tempStrikerController.judgeableQueue.Peek();
                            arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;
                            timeDiff = touchTimeSec - arriveSec - musicOffset;
                        }
                    }

                    // 홀드 끝
                    else if (isHolding && tempJudge != -1)
                    {
                        // Debug.Log("홀드 종료");
                        isHolding = false;

                        // 홀드 끝판정 보정 (너무빡셈)
                        if (tempJudge != 0)
                        {
                            if (tempJudge < 3)
                            {
                                tempJudge++;
                            }
                            else if (tempJudge > 3)
                            {
                                tempJudge--;
                            }
                        }
                    }

                    lastNonMissJudge = touchTimeSec;
                    // // Debug.Log("노트를 갖고 있고 같은 방향의 Striker를 찾았습니다");
                    break;
                }
                else if (tempStrikerController.judgeableQueue.Count != 0)
                {
                    _judgeable = tempStrikerController.judgeableQueue.Peek();

                    if (_judgeable.attackType != AttackType.HoldStart)
                    {
                        tempStrikerController = null;
                        continue;
                    }

                    arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;

                    // 시간에 따라 판정
                    timeDiff = touchTimeSec - arriveSec - musicOffset;
                    //timeDiff = touchTimeSec - projectileNoteData.arriveTime * (60f / strikerController.bpm) - musicOffset;
                    // 판정 나누기
                    // 기획서의 판정 표와 반대 순서임
                    if (timeDiff > 0.12d)
                    {
                        tempJudge = 0;
                    }
                    else if (timeDiff > 0.12d)
                    {
                        tempJudge = 1;
                    }
                    else if (timeDiff > 0.07d)
                    {
                        tempJudge = 2;
                    }
                    else if (timeDiff >= -0.07d)
                    {
                        tempJudge = 3;
                    }
                    else if (timeDiff >= -0.12d)
                    {
                        tempJudge = 4;
                    }
                    else if (timeDiff >= -0.15d)
                    {
                        tempJudge = 5;
                    }
                    else if (timeDiff >= -0.18d)
                    {
                        tempJudge = 0;
                    }

                    else  // 공노트? 공POOR?
                    {
                        tempJudge = -1;
                    }

                    // 홀드 시작
                    if (!isHolding && _judgeable.attackType == AttackType.HoldStart)
                    {
                        if (tempJudge >= 1)
                        {
                            // Debug.Log("홀드 시작");
                            isHolding = true;
                            type = AttackType.HoldStart;
                            playerManager.currentDirection = tempStrikerController.location;
                            touchDirection = tempStrikerController.location;
                            //Debug.Log(playerManager.currentDirection);
                            //print("방향전환");
                        }
                        // print(touchDirection);
                        if (tempJudge == 0)
                        {
                            // Debug.Log($"판정 수행 : Direction.{direction}, AttackType.{type}, {timeDiff:F3} -> \"{judgeStrings[tempJudge + 1]}\"");
                            JudgeManage(_judgeable, tempJudge, false, touchDirection, type);
                            _judgeable = tempStrikerController.judgeableQueue.Peek();
                            arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;
                            timeDiff = touchTimeSec - arriveSec - musicOffset;
                        }
                    }
                    // 홀드 끝
                    else if (isHolding && tempJudge != -1)
                    {
                        // Debug.Log("홀드 종료");
                        isHolding = false;
                        
                        // 홀드 끝판정 보정 (너무빡셈)
                        if (tempJudge != 0)
                        {
                            if (tempJudge < 3)
                            {
                                tempJudge++;
                            }
                            else if (tempJudge > 3)
                            {
                                tempJudge--;
                            }
                        }
                    }
                    lastNonMissJudge = touchTimeSec;
                    break;
                }

                tempStrikerController = null;
            }
      
        }

        // 판정 전송
        // Debug.Log($"판정 수행 : Direction.{direction}, AttackType.{type}, {timeDiff:F3} -> \"{judgeStrings[tempJudge + 1]}\"");
        JudgeManage(_judgeable, tempJudge, false, touchDirection, type);

        return;
    }

    // 판정 결과를 이용해 결과에 맞는 행동 수행 : 스코어, SFX, ...
    // public void JudgeManage(Judgeable judgeObject, int judgement, bool isPassing = false)
    // public void JudgeManage(Direction direction, int judgement, AttackType type,
    //                         StrikerController strikerController, bool isPassing = false)

    // 혹시 모를 성능 때문에 judgeObject가 null일 때를 대비한 tpD, tpT 매개변수를 만들어뒀는데,
    // GPT는 깡통 (noteDirection과 attackType만 들어있고, 나머지는 null) Judgeable 객체를
    // 자주 생성하는 것이 성능에 큰 영향을 끼치지는 않는다고 함.
    public void JudgeManage(Judgeable judgeObject, int judgement, bool isPassing = false, Direction tpD = Direction.None, AttackType tpT = AttackType.Normal)
    {
        // Debug.Log($"JudgeManage0 {judgement}");
        
        // 노트가 처리되지 않은 경우
        if (judgeObject == null || judgement == -1)
        {
            lastNonMissJudge = 0;
            if (tpT == AttackType.HoldFinishStrong)
            {
                return;
            }
            playerManager.Operate(tpD, tpT);
            playerManager.PlayerParrySound(tpT);
            return;
        }

        // 플레이어가 조작하지 않거나 조작이 무시되는 경우, 홀드 늦게떼기 제외
        if (!isPassing || judgeObject.attackType == AttackType.HoldFinishStrong)
        {
            playerManager.Operate(judgeObject.noteDirection, judgeObject.attackType);
        }

        // index로 한번에 처리
        judgeDetails[0][judgement + 1] += 1;

        // // Debug.Log($"JudgeManage {judgeDetails} {(int)judgeObject.noteDirection} {judgement + 1}");

        // Debug.Log($"JudgeManage1 {judgeObject.noteDirection} {judgement}");
        // Debug.Log($"JudgeManage2 {judgeDetails[0][1]}");

        if (!TutorialManager.isTutorial) judgeDetails[(int)judgeObject.noteDirection][judgement + 1] += 1;
        

        // 특정 Striker 찾기
        StrikerController targetStriker = judgeObject.strikerController;

        // GameObject targetProjectile = targetStriker.projectileQueue.Peek();

        CameraMoving CameraEffect = GameObject.Find("Main Camera").GetComponent<CameraMoving>();
        // judgement = 3; // 게임 플레이 구경용 코드 
        // 따로 처리
        switch (judgement)
        {
            case 0:  // 늦은 BAD (MISS)
                score += 0;
                combo = 0;

                // 피격당한 후 죽었을 때
                if (--playerManager.hp == 0 && !TutorialManager.isTutorial)
                {
                    scoreUI.HideAll();
                    playerManager.PlayerHitSound();
                    playerManager.GameOver();
                    // Debug.LogError("abc");
                }
                // 피격당한 후 죽지 않았을 때
                else
                {
                    // Debug.Log(playerManager.hp);
                    UIManager.Instance.ShowDamageOverlayEffect();
                    CameraEffect.CameraShake();
                    playerManager.PlayerHitSound();
                }

                break;

            case 1:  // 늦은 BLOCKED
                score += 300;
                combo = 0;
                playerManager.PlayerBlockedSound();
                break;

            case 2:  // 늦은 PARRIED
                score += 9000;
                combo += 1;
                targetStriker?.TakeDamage(1, judgeObject.attackType);
                UIManager.Instance.ShowParticle(judgeObject.noteDirection, false);
                break;

            case 3:  // 완벽한 PERFECT
                score += 30000;
                combo += 1;
                targetStriker?.TakeDamage(1, judgeObject.attackType);
                UIManager.Instance.ShowParticle(judgeObject.noteDirection, true);
                break;

            case 4:  // 빠른 PARRIED
                score += 9000;
                combo += 1;
                targetStriker?.TakeDamage(1, judgeObject.attackType);
                UIManager.Instance.ShowParticle(judgeObject.noteDirection, false);
                break;

            case 5:  // 빠른 BLOCKED
                score += 300;
                combo = 0;
                playerManager.PlayerBlockedSound();
                break;
        }

        if (judgement != 0)
        {
            scoreUI.DisplayScore(score);
            
            if (judgement != 1 && judgement != 5)
            {
                // parriedProjectileManager.CreateParriedProjectile(targetProjectile.transform.position, direction);
                if (parriedProjectileManager != null && !targetStriker.isMelee && judgeObject.attackType != AttackType.StreamStart)
                {
                    if (targetStriker.boss != null)
                        parriedProjectileManager.ParryTusache(Direction.Up, (int)judgeObject.attackType);
                    else
                        parriedProjectileManager.ParryTusache(judgeObject.noteDirection, (int)judgeObject.attackType);
                }
                else
                {
                    // Debug.Log("패링투사체 못찾음");
                }
            }
        }
        else
        {
            scoreUI.DisplayHP(playerManager.hp);
        }

        scoreUI.DisplayJudge(judgement, judgeObject.noteDirection);

        // 대상 노트 제거
        judgeObject.FinishJudge();
        // Destroy(targetProjectile);

        return;
    }
}
