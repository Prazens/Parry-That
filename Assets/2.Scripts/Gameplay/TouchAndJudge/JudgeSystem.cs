using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private NotePerformer notePerformer;

    public int combo = 0;
    public int score = 0;

    public double lastNonMissJudge = 0;
    public bool isHolding = false;
    public bool isOnStream = false;  // 연타 중인지 확인
    public int streamCount = -1;  // 연타한 횟수
    public Judgeable streamJudgeable = null; // 연타 판정용 Judgeable 저장

    private Dictionary<Direction, Queue<Judgeable>> judgeableQueues = new() {
        { Direction.None, new() },
        { Direction.Up, new() },
        { Direction.Down, new() },
        { Direction.Left, new() },
        { Direction.Right, new() },
    };
    public Queue<JudgeFormat> judgeQueue = new();

    public int bpm;
    public List<int[]> judgeDetails = new List<int[]>();  

    private string[] judgeStrings = new string[7]
                                    { "공노트", "늦은 MISS", "늦은 BLOCKED", "늦은 PARRIED",
                                      "PERFECT", "빠른 PARRIED", "빠른 BLOCKED" };

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
            dynamicUIManager = FindObjectOfType<DynamicUIManager>();

        if (stageAudioManager == null)
            stageAudioManager = FindObjectOfType<StageAudioManager>();

        if (notePerformer == null)
            notePerformer = FindObjectOfType<NotePerformer>();
    }

    void Update()
    {
        if (!StageFlowManager.isActive) return;

        foreach (var judgeableQueue in judgeableQueues.Values)
        {
            if (judgeableQueue.Count <= 0) continue;

            Judgeable tempJudgeable = judgeableQueue.Peek();

            float tempSecDiff =
                StageFlowManager.Instance.currentTime
                - notePerformer.BeatToSec(tempJudgeable.arriveBeat, MusicOffset);

            // 연타 시작
            if (tempJudgeable.attackType == AttackType.StreamStart && !isOnStream && tempSecDiff > -0.01d)
            {
                isOnStream = true;
                streamCount = 0;
                streamJudgeable = judgeableQueue.Dequeue();
                break;
            }

            // 연타 종료
            else if (tempJudgeable.attackType == AttackType.StreamFinish)
            {
                if (tempSecDiff > -0.01d)
                {
                    if (streamCount < streamJudgeable.streamCount * 1 / 2)
                        JudgeManage(streamJudgeable, JudgeType.LateMiss);
                    else if (streamCount < streamJudgeable.streamCount * 3 / 4)
                        JudgeManage(streamJudgeable, JudgeType.LateBlocked);
                    else if (streamCount < streamJudgeable.streamCount)
                        JudgeManage(streamJudgeable, JudgeType.LateParried);
                    else
                        JudgeManage(streamJudgeable, JudgeType.Perfect);

                    isOnStream = false;
                    streamCount = -1;
                    streamJudgeable = null;
                    judgeableQueue.Dequeue();
                    break;
                }
            }

            // 늦은 MISS
            if (tempSecDiff > 0.2d)
            {
                if (tempJudgeable.attackType == AttackType.HoldStart)
                {
                    JudgeManage(tempJudgeable, JudgeType.LateMiss, true);
                    tempJudgeable = judgeableQueue.Peek();
                    tempSecDiff =
                        StageFlowManager.Instance.currentTime
                        - notePerformer.BeatToSec(tempJudgeable.arriveBeat, MusicOffset);
                }
                else if (tempJudgeable.attackType == AttackType.HoldStop)
                {
                    isHolding = false;
                }
                JudgeManage(tempJudgeable, JudgeType.LateMiss, true);
            }
        }

        foreach (JudgeFormat judgeObject in judgeQueue)
            Judge(judgeObject.direction, judgeObject.timing, judgeObject.type);

        if (judgeQueue.Count != 0)
            judgeQueue.Clear();
    }

    public void Initialize()
    {
        combo = 0;
        score = 0;
        isHolding = false;

        judgeDetails = new List<int[]>();

        for (int i = 0; i < strikerManager.charts.Count + 1; i++)
        {
            if (i == 0)
            {
                judgeDetails.Add(new int[7] { 0, 0, 0, 0, 0, 0, 0 });
                foreach (ChartData chart in strikerManager.charts)
                    judgeDetails[0][0] += chart.notes.Length;
            }
            else
            {
                judgeDetails.Add(new int[7] { strikerManager.charts[i - 1].notes.Length, 0, 0, 0, 0, 0, 0 });
            }
        }
    }

    public void EnqueueJudgeable(Judgeable judgeable)
    {
        judgeableQueues[judgeable.noteDirection].Enqueue(judgeable);
    }

    public Judgeable DequeueJudgeable(Direction dir)
    {
        return judgeableQueues[dir].Dequeue();
    }

    public Judgeable PeekJudgeable(Direction dir)
    {
        return judgeableQueues[dir].Peek();
    }

    public int CountJudgeable(Direction dir)
    {
        return judgeableQueues[dir].Count;
    }

    // -------------------------------------------
    // ★ 구버전 홀드 로직 1:1 복원된 Judge()
    // -------------------------------------------
    public void Judge(Direction direction, double touchTimeSec, AttackType type)
    {
        if (isOnStream)
        {
            if (type == AttackType.HoldStop)
                return;

            if (streamCount != -1)
            {
                streamCount++;
                JudgeManage(null, JudgeType.EarlyMiss, false, Direction.None, type);
                return;
            }
            else
            {
                Debug.LogError("stream 중이 아닌 isOnStream");
                return;
            }
        }

        // 구버전: 홀드 중엔 HoldStop 외의 입력은 모두 무시
        if (isHolding)
        {
            if (!(type == AttackType.HoldStop))
                return;
        }

        // 홀드 중이 아니면 HoldStop 입력 무시
        if (type == AttackType.HoldStop && !isHolding)
        {
            return;
        }

        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;
        Judgeable _judgeable = null;
        float arriveSec;
        double timeDiff = touchTimeSec - lastNonMissJudge;

        // 간접 미스 방지
        if (type == AttackType.Strong && timeDiff < 0.01d)
            return;

        JudgeType tempJudge = JudgeType.None;

        playerManager.currentDirection = touchDirection;

        // 각 스트라이커 탐색
        foreach (var judgeableQueue in judgeableQueues.Values)
        {
            if (judgeableQueue.Count <= 0) continue;

            _judgeable = judgeableQueue.Peek();

            // 방향 다르면 패스 (HoldStart/HoldStop 제외)
            if (_judgeable.noteDirection != touchDirection
                && _judgeable.attackType != AttackType.HoldStart
                && _judgeable.attackType != AttackType.HoldStop)
                continue;

            arriveSec = notePerformer.BeatToSec(_judgeable.arriveBeat, MusicOffset);
            timeDiff = touchTimeSec - arriveSec;

            // 강공격 → 약패링 무효
            if (type == AttackType.Normal && _judgeable.attackType == AttackType.Strong)
            {
                tempJudge = JudgeType.None;
                continue;
            }

            // 시간으로 판정
            if (timeDiff > 0.2d) tempJudge = JudgeType.LateMiss;
            else if (timeDiff > 0.14d) tempJudge = JudgeType.LateBlocked;
            else if (timeDiff > 0.07d) tempJudge = JudgeType.LateParried;
            else if (timeDiff >= -0.07d) tempJudge = JudgeType.Perfect;
            else if (timeDiff >= -0.14d) tempJudge = JudgeType.EarlyParried;
            else if (timeDiff >= -0.2d) tempJudge = JudgeType.EarlyBlocked;
            else if (type == AttackType.HoldStop) tempJudge = JudgeType.LateMiss;
            else tempJudge = JudgeType.None;

            // ----------------------------
            // ★ 구버전 홀드 시작 로직
            // ----------------------------
            if (!isHolding && _judgeable.attackType == AttackType.HoldStart)
            {
                if (tempJudge >= JudgeType.LateBlocked && tempJudge <= JudgeType.EarlyBlocked)
                {
                    isHolding = true;
                    type = AttackType.HoldStart;
                    playerManager.currentDirection = _judgeable.noteDirection;
                    touchDirection = _judgeable.noteDirection;
                }

                if (tempJudge == JudgeType.LateMiss)
                {
                    JudgeManage(_judgeable, tempJudge, false, touchDirection, type);

                    _judgeable = judgeableQueue.Peek();
                    arriveSec = notePerformer.BeatToSec(_judgeable.arriveBeat, MusicOffset);
                    timeDiff = touchTimeSec - arriveSec;
                }
            }

            // ----------------------------
            // ★ 구버전 홀드 종료 로직
            // ----------------------------
            else if (type == AttackType.HoldStop && tempJudge != JudgeType.None)
            {
                isHolding = false;

                if (tempJudge != JudgeType.LateMiss)
                {
                    if (tempJudge >= JudgeType.LateBlocked && tempJudge <= JudgeType.EarlyBlocked)
                        tempJudge = JudgeType.Perfect;
                }
            }

            lastNonMissJudge = touchTimeSec;
            break;
        }

        Debug.Log($"판정 수행. touch: Dir.{touchDirection}, Type.{type}, {timeDiff:F3} -> \"{judgeStrings[(int)tempJudge]}\"");
        if (_judgeable != null)
        {
            Debug.Log($"Judgeable: {_judgeable.attackType}, {_judgeable.arriveBeat}, {_judgeable.noteDirection}");
            Debug.Log($"touchTimeSec: {touchTimeSec}, arriveSec: {notePerformer.BeatToSec(_judgeable.arriveBeat, MusicOffset)}");
        }
        else
        {
            Debug.Log("Judgeable is null");
        }
        JudgeManage(_judgeable, tempJudge, false, touchDirection, type);
    }

    public void JudgeManage(Judgeable judgeObject, JudgeType judgement, bool isPassing = false, 
                        Direction tpD = Direction.None, AttackType tpT = AttackType.Normal)
    {
        // 노트가 처리되지 않은 경우
        if (judgeObject == null || judgement == JudgeType.None)
        {
            lastNonMissJudge = 0;
            if (tpT == AttackType.HoldStop)
            {
                return;
            }

            // 연타 중
            if (isOnStream && judgement == JudgeType.EarlyMiss)
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
        judgeDetails[0][(int)judgement] += 1;

        if (!TutorialManager.isTutorial)
        {
            judgeDetails[(int)judgeObject.noteDirection][(int)judgement] += 1;
        }

        // 특정 Striker 찾기
        StrikerController targetStriker = judgeObject.strikerController;

        CameraMoving cameraEffect = GameObject.Find("Main Camera").GetComponent<CameraMoving>();

        switch (judgement)
        {
            case JudgeType.LateMiss:
                score += 0;
                combo = 0;
                break;

            case JudgeType.LateBlocked:
                score += 300;
                combo = 0;
                break;

            case JudgeType.LateParried:
                score += 9000;
                combo += 1;
                break;

            case JudgeType.Perfect:
                score += 30000;
                combo += 1;
                break;

            case JudgeType.EarlyParried:
                score += 9000;
                combo += 1;
                break;

            case JudgeType.EarlyBlocked:
                score += 300;
                combo = 0;
                break;
        }

        if (judgement != JudgeType.LateMiss)
        {
            dynamicUIManager?.DisplayScore(score);

            if (judgement == JudgeType.LateBlocked || judgement == JudgeType.EarlyBlocked)
            {
                playerManager.PlayerBlockedSound();
            }

            if (judgement >= JudgeType.LateParried && judgement <= JudgeType.EarlyParried)
            {
                if (parriedProjectileManager != null &&
                    targetStriker != null &&
                    judgeObject.judgeableObject != null &&
                    judgeObject.attackType != AttackType.StreamStart)
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
            // 피격 → 사망
            if (--playerManager.hp == 0 && !TutorialManager.isTutorial)
            {
                dynamicUIManager?.HideAll();

                playerManager.PlayerHitSound();

                StageFlowManager.Instance?.GameOver();
            }
            // 피격 → 생존
            else
            {
                dynamicUIManager?.ShowDamageOverlayEffect();

                cameraEffect?.CameraShake();

                playerManager.PlayerHitSound();
            }

            dynamicUIManager?.DisplayHP(playerManager.hp, false);
        }

        dynamicUIManager?.DisplayJudge((int)judgement, judgeObject.noteDirection);

        // 대상 노트 제거
        bool isParried = (judgement >= JudgeType.LateParried && judgement <= JudgeType.EarlyParried);
        FinishJudge(judgeObject, isParried);

        return;
    }

    private void FinishJudge(Judgeable judgeable, bool isParried)
    {
        var judgeableQueue = judgeableQueues[judgeable.noteDirection];

        if (judgeableQueue.Peek() == judgeable)
        {
            judgeableQueue.Dequeue();
            if (judgeable.judgeableObject != null)
            {
                Destroy(judgeable.judgeableObject);
            }

            judgeable.strikerController.OnJudge(judgeable, isParried);
        }
    }
}
