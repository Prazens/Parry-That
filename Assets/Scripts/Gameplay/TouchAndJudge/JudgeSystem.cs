using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JudgeSystem : MonoBehaviour
{
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    [SerializeField] public ParriedProjectileManager parriedProjectileManager;

    // ScoreUI / UIManager -> DynamicUIManager
    [SerializeField] private DynamicUIManager dynamicUIManager;

    // musicOffset -> StageAudioManager
    [SerializeField] private StageAudioManager stageAudioManager;

    public int combo = 0;
    public int score = 0;

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

    private float MusicOffset
    {
        get
        {
            if (stageAudioManager == null) return 0f;
            return stageAudioManager.musicOffset;
        }
    }

    private void Awake()
    {
        if (dynamicUIManager == null)
        {
            dynamicUIManager = FindObjectOfType<DynamicUIManager>();
        }

        if (stageAudioManager == null)
        {
            stageAudioManager = FindObjectOfType<StageAudioManager>();
        }
    }

    void Update()
    {
        if (!StageFlowManager.isActive) return;

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

                    float tempTimeDiff =
                        StageFlowManager.Instance.currentTime
                        - tempJudgeable.arriveBeat * 60f / tempStrikerController.bpm
                        - MusicOffset;

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
                                // MISS 처리
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
                                JudgeManage(streamJudgeable, 3);
                            }

                            isOnStream = false;
                            streamCount = -1;
                            streamJudgeable = null;
                            tempStrikerController.judgeableQueue.Dequeue();
                            break;
                        }
                    }

                    if (tempTimeDiff > 0.2d)
                    {
                        if (tempJudgeable.attackType == AttackType.HoldStart)
                        {
                            JudgeManage(tempJudgeable, 0, true);
                            tempJudgeable = tempStrikerController.judgeableQueue.Peek();
                            tempTimeDiff =
                                StageFlowManager.Instance.currentTime
                                - tempJudgeable.arriveBeat * 60f / tempStrikerController.bpm
                                - MusicOffset;
                        }
                        else if (tempJudgeable.attackType == AttackType.HoldStop)
                        {
                            isHolding = false;
                        }
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
        isOnStream = false;
        streamCount = -1;
        streamJudgeable = null;
        lastNonMissJudge = 0;

        judgeDetails = new List<int[]>();

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
    public void Judge(Direction direction, double touchTimeSec, AttackType type)
    {
        if (isOnStream)
        {
            if (type == AttackType.HoldStop)
            {
                return;
            }
            if (streamCount != -1)
            {
                streamCount++;
                print($"{streamCount} 연타");
                JudgeManage(null, 6, false, Direction.None, type);
                return;
            }
            else
            {
                Debug.LogError("stream 중이 아닌 isOnStream");
                return;
            }
        }

        print(isHolding);

        if (isHolding)
        {
            if (!(type == AttackType.HoldStop))
            {
                return;
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
            return;
        }

        int tempJudge = -1;

        playerManager.currentDirection = touchDirection;

        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerManager.strikerList)
        {
            if (striker != null)
            {
                tempStrikerController = striker.GetComponent<StrikerController>();

                if (tempStrikerController.judgeableQueue.Count != 0)
                {
                    _judgeable = tempStrikerController.judgeableQueue.Peek();

                    // 같은 방향이거나 홀드시작노트거나 홀드종료노트가 아니면 패스
                    if (tempStrikerController.location != touchDirection
                        && _judgeable.attackType != AttackType.HoldStart
                        && _judgeable.attackType != AttackType.HoldStop)
                    {
                        continue;
                    }

                    if (type == AttackType.HoldStop)
                    {
                        if (_judgeable.attackType != AttackType.HoldStop)
                        {
                            print("홀드 중에 다른 노트 판정 무시됨");
                            break;
                        }
                        print(_judgeable.attackType);
                    }

                    arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;

                    // 시간에 따라 판정
                    timeDiff = touchTimeSec - arriveSec - MusicOffset;

                    // 강공격을 약패링으로 처리한 경우
                    if (type == AttackType.Normal && _judgeable.attackType == AttackType.Strong)
                    {
                        tempJudge = -1;
                        continue;
                    }
                    // 판정 나누기
                    else if (timeDiff > 0.2d)
                    {
                        tempJudge = 0;
                    }
                    else if (timeDiff > 0.14d)
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
                    else if (timeDiff >= -0.14d)
                    {
                        tempJudge = 4;
                    }
                    else if (timeDiff >= -0.2d)
                    {
                        tempJudge = 5;
                    }
                    else if (timeDiff >= -0.22d)
                    {
                        tempJudge = 0;
                    }
                    else if (type == AttackType.HoldStop)
                    {
                        tempJudge = 0;
                    }
                    else
                    {
                        tempJudge = -1;
                    }

                    // 홀드 시작
                    if (!isHolding && _judgeable.attackType == AttackType.HoldStart)
                    {
                        if (tempJudge >= 1)
                        {
                            isHolding = true;
                            type = AttackType.HoldStart;
                            playerManager.currentDirection = tempStrikerController.location;
                            touchDirection = tempStrikerController.location;
                        }

                        if (tempJudge == 0)
                        {
                            JudgeManage(_judgeable, tempJudge, false, touchDirection, type);
                            _judgeable = tempStrikerController.judgeableQueue.Peek();
                            arriveSec = _judgeable.arriveBeat * 60f / tempStrikerController.bpm;
                            timeDiff = touchTimeSec - arriveSec - MusicOffset;
                        }
                    }

                    // 홀드 끝
                    else if (type == AttackType.HoldStop && tempJudge != -1)
                    {
                        print("홀드 종료");
                        isHolding = false;

                        // 홀드 끝판정 보정 (너무빡셈)
                        if (tempJudge != 0)
                        {
                            if (tempJudge <= 5 && tempJudge >= 1)
                            {
                                tempJudge = 3;
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
        Debug.Log($"판정 수행 : Direction.{direction}, AttackType.{type}, {timeDiff:F3} -> \"{judgeStrings[tempJudge + 1]}\"");
        JudgeManage(_judgeable, tempJudge, false, touchDirection, type);

        return;
    }

    // 판정 결과를 이용해 결과에 맞는 행동 수행 : 스코어, SFX, ...
    public void JudgeManage(Judgeable judgeObject, int judgement, bool isPassing = false, Direction tpD = Direction.None, AttackType tpT = AttackType.Normal)
    {
        // 노트가 처리되지 않은 경우
        if (judgeObject == null || judgement == -1)
        {
            lastNonMissJudge = 0;
            if (tpT == AttackType.HoldStop)
            {
                return;
            }

            // 연타 중
            if (isOnStream && judgement == 6)
            {
                score += 100;
                combo = 1;
                playerManager.Operate((Direction)UnityEngine.Random.Range(1, 5), tpT);
                playerManager.PlayerParrySound(tpT);
                return;
            }

            playerManager.Operate(tpD, tpT);
            playerManager.PlayerParrySound(tpT);
            return;
        }

        // 플레이어가 조작하지 않거나 조작이 무시되는 경우, 홀드 늦게떼기 제외
        if (!isPassing || judgeObject.attackType == AttackType.HoldStop)
        {
            playerManager.Operate(judgeObject.noteDirection, judgeObject.attackType);
        }

        // index로 한번에 처리
        judgeDetails[0][judgement + 1] += 1;

        if (!TutorialManager.isTutorial)
        {
            // 기존 코드 그대로: Direction enum이 1~4(Up/Down/Left/Right)라고 가정
            judgeDetails[(int)judgeObject.noteDirection][judgement + 1] += 1;
        }

        // 특정 Striker 찾기
        StrikerController targetStriker = judgeObject.strikerController;

        CameraMoving cameraEffect = GameObject.Find("Main Camera").GetComponent<CameraMoving>();

        switch (judgement)
        {
            case 0:  // 늦은 BAD (MISS)
                score += 0;
                combo = 0;

                // 피격당한 후 죽었을 때
                if (--playerManager.hp == 0 && !TutorialManager.isTutorial)
                {
                    if (dynamicUIManager != null)
                    {
                        dynamicUIManager.HideAll();
                    }

                    playerManager.PlayerHitSound();

                    if (StageFlowManager.Instance != null)
                    {
                        StageFlowManager.Instance.GameOver();
                    }
                }
                // 피격당한 후 죽지 않았을 때
                else
                {
                    if (dynamicUIManager != null)
                    {
                        dynamicUIManager.ShowDamageOverlayEffect();
                    }

                    if (cameraEffect != null)
                    {
                        cameraEffect.CameraShake();
                    }

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
                if (dynamicUIManager != null) dynamicUIManager.ShowParticle(judgeObject.noteDirection, false);
                break;

            case 3:  // 완벽한 PERFECT
                score += 30000;
                combo += 1;
                targetStriker?.TakeDamage(1, judgeObject.attackType);
                if (dynamicUIManager != null) dynamicUIManager.ShowParticle(judgeObject.noteDirection, true);
                break;

            case 4:  // 빠른 PARRIED
                score += 9000;
                combo += 1;
                targetStriker?.TakeDamage(1, judgeObject.attackType);
                if (dynamicUIManager != null) dynamicUIManager.ShowParticle(judgeObject.noteDirection, false);
                break;

            case 5:  // 빠른 BLOCKED
                score += 300;
                combo = 0;
                playerManager.PlayerBlockedSound();
                break;
        }

        if (judgement != 0)
        {
            if (dynamicUIManager != null)
            {
                dynamicUIManager.DisplayScore(score);
            }

            if (judgement != 1 && judgement != 5)
            {
                // parriedProjectileManager.CreateParriedProjectile(targetProjectile.transform.position, direction);
                if (parriedProjectileManager != null && targetStriker != null && !targetStriker.isMelee && judgeObject.attackType != AttackType.StreamStart)
                {
                    if (targetStriker.boss != null)
                    {
                        int fixRandom = 0;
                        if (judgeObject.noteDirection == Direction.Up || judgeObject.noteDirection == Direction.Right)
                        {
                            fixRandom = 1;
                        }
                        else
                        {
                            fixRandom = 2;
                        }
                        parriedProjectileManager.ParryTusache(Direction.Up, (int)judgeObject.attackType, fixRandom);
                    }
                    else
                    {
                        parriedProjectileManager.ParryTusache(judgeObject.noteDirection, (int)judgeObject.attackType);
                    }
                }
            }
        }
        else
        {
            if (dynamicUIManager != null)
            {
                // 기존 ScoreUI.DisplayHP(int hp)와 매칭: 여기서는 heal=false 고정
                dynamicUIManager.DisplayHP(playerManager.hp, false);
            }
        }

        if (dynamicUIManager != null)
        {
            dynamicUIManager.DisplayJudge(judgement, judgeObject.noteDirection);
        }

        // 대상 노트 제거
        judgeObject.FinishJudge();

        return;
    }
}
