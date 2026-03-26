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

    public int combo = 0;
    public int score = 0;

    public double lastNonMissJudge = 0;
    public bool isHolding = false; // 홀드 공격을 받아치는 중인지 (누르고 있는지가 아님)
    //public bool isOnStream = false;  // 연타 중인지 확인
    //public int streamCount = -1;  // 연타한 횟수
    //public Judgeable streamJudgeable = null; // 연타 판정용 Judgeable 저장

    private Dictionary<Direction, Queue<Judgeable>> judgeableQueues = new() {
        { Direction.None, new() },
        { Direction.Up, new() },
        { Direction.Down, new() },
        { Direction.Left, new() },
        { Direction.Right, new() },
    };
    public Queue<JudgeFormat> judgeQueue = new();

    public List<int[]> judgeDetails = new List<int[]>();  

    private string[] judgeStrings = new string[8]
                                    { "공노트", "늦은 MISS", "늦은 BLOCKED", "늦은 PARRIED",
                                      "PERFECT", "빠른 PARRIED", "빠른 BLOCKED", "빠른 MISS" };

    private void Awake()
    {
        if (dynamicUIManager == null)
            dynamicUIManager = FindObjectOfType<DynamicUIManager>();
    }

    void Update()
    {
        if (!StageFlowManager.isActive) return;

        // 방향별 스트라이커 공격 Queue를 순회하며 LateMiss 여부 확인하여 처리
        foreach (var judgeableQueue in judgeableQueues.Values)
        {
            if (judgeableQueue.Count <= 0) continue;

            Judgeable tempJudgeable = judgeableQueue.Peek();

            float tempSecDiff =
                StageFlowManager.Instance.currentTime
                - StageFlowManager.Instance.BeatToSec(tempJudgeable.arriveBeat);

            // 연타 시작
            //if (tempJudgeable.attackType == AttackType.StreamStart && !isOnStream && tempSecDiff > -0.01d)
            //{
            //    isOnStream = true;
            //    streamCount = 0;
            //    streamJudgeable = judgeableQueue.Dequeue();
            //    break;
            //}

            // 연타 종료
            //else if (tempJudgeable.attackType == AttackType.StreamFinish)
            //{
            //    if (tempSecDiff > -0.01d)
            //    {
            //        if (streamCount < streamJudgeable.streamCount * 1 / 2)
            //            JudgeManage(streamJudgeable, JudgeType.LateMiss);
            //        else if (streamCount < streamJudgeable.streamCount * 3 / 4)
            //            JudgeManage(streamJudgeable, JudgeType.LateBlocked);
            //        else if (streamCount < streamJudgeable.streamCount)
            //            JudgeManage(streamJudgeable, JudgeType.LateParried);
            //        else
            //            JudgeManage(streamJudgeable, JudgeType.Perfect);

            //        isOnStream = false;
            //        streamCount = -1;
            //        streamJudgeable = null;
            //        judgeableQueue.Dequeue();
            //        break;
            //    }
            //}

            // 늦은 MISS
            if (tempSecDiff > 0.2d)
            {
                if (tempJudgeable.attackType == AttackType.HoldStop)
                {
                    // Hold 공격이 끝났으므로 누르고 있는지에 무관하게 false
                    isHolding = false;
                }

                JudgeManage(tempJudgeable, JudgeType.LateMiss, true);

                if (tempJudgeable.attackType == AttackType.HoldStart)
                {
                    // HoldStop에 대한 EarlyMiss 처리, 이후 HoldStop 공격은 무시됨
                    JudgeManage(null, JudgeType.EarlyMiss, true);
                }
            }
        }

        // 터치 입력 Queue에 들어온 입력을 판정
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

    public void Judge(Direction touchDirection, double touchTimeSec, AttackType touchType)
    {
        // 연타 중
        //if (isOnStream)
        //{
        //    if (touchType == AttackType.HoldStop)
        //        return;

        //    if (streamCount != -1)
        //    {
        //        streamCount++;
        //        JudgeManage(null, JudgeType.EarlyMiss, false, Direction.None, touchType);
        //        return;
        //    }
        //    else
        //    {
        //        Debug.LogError("stream 중이 아닌 isOnStream");
        //        return;
        //    }
        //}

        // 홀드 중엔 HoldStop 외의 입력은 모두 무시
        if (isHolding && touchType != AttackType.HoldStop)
        {
            return;
        }

        double timeDiff = touchTimeSec - lastNonMissJudge;
        // 간접 미스 방지
        if (touchType == AttackType.Strong && timeDiff < 0.01d)
            return;

        // touchType에 따라 분기
        switch (touchType)
        {
            case AttackType.Normal:
                JudgeNormalTouch(touchDirection, touchTimeSec);
                return;
            case AttackType.Strong:
                JudgeStrongTouch(touchDirection, touchTimeSec);
                return;
            case AttackType.HoldStop:
                JudgeHoldStopTouch(touchDirection, touchTimeSec);
                return;
        }
    }

    private void JudgeNormalTouch(Direction touchDirection, double touchTimeSec)
    {
        AttackType touchType = AttackType.Normal;
        JudgeType judgeType = JudgeType.None;

        // 모든 방향 중 가장 빠른 공격 탐색
        Judgeable judgeable = GetClosestAttackFromAllDirections();

        // 가장 빠른 공격이 강공격이면 약패링 무시
        if (judgeable != null && judgeable.attackType != AttackType.Strong)
        {
            float arriveSec = StageFlowManager.Instance.BeatToSec(judgeable.arriveBeat);
            judgeType = GetJudgeType(touchTimeSec, arriveSec);

            // EarlyMiss는 무시
            if (judgeType == JudgeType.EarlyMiss)
            {
                judgeType = JudgeType.None;
            }
        }

        if (judgeType != JudgeType.None)
        {
            // 홀드 시작 공격 처리
            if (judgeable.attackType == AttackType.HoldStart)
            {
                isHolding = true;
                touchType = AttackType.HoldStart;
            }

            // 플레이어가 자동으로 공격 방향을 바라보며 패링함
            touchDirection = judgeable.noteDirection;
            lastNonMissJudge = touchTimeSec;
        }

        DebugJudge(touchDirection, touchTimeSec, touchType, judgeable, judgeType);
        JudgeManage(judgeable, judgeType, false, touchDirection, touchType);
    }

    private void JudgeStrongTouch(Direction touchDirection, double touchTimeSec)
    {
        JudgeType judgeType = JudgeType.None;

        Judgeable judgeable = null;
        
        // 방향에 맞는 Queue만 확인
        var judgeableQueue = judgeableQueues[touchDirection];
        if (judgeableQueue.Count > 0)
        {
            judgeable = judgeableQueue.Peek();

            float arriveSec = StageFlowManager.Instance.BeatToSec(judgeable.arriveBeat);
            judgeType = GetJudgeType(touchTimeSec, arriveSec);

            // EarlyMiss는 무시
            if (judgeType == JudgeType.EarlyMiss)
            {
                judgeType = JudgeType.None;
            }

            if (judgeType != JudgeType.None)
            {
                // 약공격에 강패링하면 Blocked 판정
                if (judgeable.attackType == AttackType.Normal)
                {
                    judgeType = JudgeType.EarlyBlocked;
                }

                lastNonMissJudge = touchTimeSec;
            }
        }

        DebugJudge(touchDirection, touchTimeSec, AttackType.Strong, judgeable, judgeType);
        JudgeManage(judgeable, judgeType, false, touchDirection, AttackType.Strong);
    }

    private void JudgeHoldStopTouch(Direction touchDirection, double touchTimeSec)
    {
        // 홀드 중이 아니면 HoldStop 입력 무시
        if (!isHolding)
            return;
        isHolding = false;

        // 홀드 중 손을 뗀 것으로 기본적으로 EarlyMiss 판정
        JudgeType judgeType = JudgeType.EarlyMiss;

        // 모든 방향 중 가장 빠른 공격 탐색
        Judgeable judgeable = GetClosestAttackFromAllDirections();

        if (judgeable != null && judgeable.attackType == AttackType.HoldStop)
        {
            float arriveSec = StageFlowManager.Instance.BeatToSec(judgeable.arriveBeat);
            judgeType = GetJudgeType(touchTimeSec, arriveSec);
        }

        if (judgeType != JudgeType.EarlyMiss)
        {
            // 플레이어가 자동으로 공격 방향을 바라보며 패링함
            touchDirection = judgeable.noteDirection;
            lastNonMissJudge = touchTimeSec;
        }

        DebugJudge(touchDirection, touchTimeSec, AttackType.HoldStop, judgeable, judgeType);
        JudgeManage(judgeable, judgeType, false, touchDirection, AttackType.HoldStop);
    }

    private Judgeable GetClosestAttackFromAllDirections()
    {
        Judgeable judgeable = null;
        float arriveBeat = Mathf.Infinity;

        // 모든 방향 중 가장 빠른 공격 탐색
        foreach (var judgeableQueue in judgeableQueues.Values)
        {
            if (judgeableQueue.Count <= 0) continue;

            Judgeable tempJudgeable = judgeableQueue.Peek();

            if (tempJudgeable.arriveBeat < arriveBeat)
            {
                judgeable = tempJudgeable;
                arriveBeat = tempJudgeable.arriveBeat;
            }
        }

        return judgeable;
    }

    private JudgeType GetJudgeType(double touchTimeSec, double attackArriveSec)
    {
        double timeDiff = touchTimeSec - attackArriveSec;
        if (timeDiff > 0.2d) return JudgeType.LateMiss;
        else if (timeDiff > 0.14d) return JudgeType.LateBlocked;
        else if (timeDiff > 0.07d) return JudgeType.LateParried;
        else if (timeDiff >= -0.07d) return JudgeType.Perfect;
        else if (timeDiff >= -0.14d) return JudgeType.EarlyParried;
        else if (timeDiff >= -0.2d) return JudgeType.EarlyBlocked;
        else return JudgeType.EarlyMiss;
    }

    private void DebugJudge(Direction touchDirection, double touchTimeSec, AttackType touchType, Judgeable judgeable, JudgeType judgeType)
    {
        Debug.Log($"Touch: Dir.{touchDirection}, Type.{touchType}");
        if (judgeable != null)
        {
            Debug.Log($"Judgeable: Dir.{judgeable.noteDirection}, Type.{judgeable.attackType}, Beat.{judgeable.arriveBeat}");
            Debug.Log($"touchTimeSec: {touchTimeSec}, arriveSec: {StageFlowManager.Instance.BeatToSec(judgeable.arriveBeat)}");
        }
        else
        {
            Debug.Log("Judgeable is null");
        }
        Debug.Log($"판정 결과: {judgeStrings[(int)judgeType]}");
    }

    public void JudgeManage(Judgeable judgeable, JudgeType judgeType, bool isPassing = false, 
                        Direction touchDirection = Direction.None, AttackType touchType = AttackType.Normal)
    {
        // 노트가 처리되지 않은 경우
        if (judgeType == JudgeType.None)
        {
            lastNonMissJudge = 0;

            if (touchType == AttackType.HoldStop)
                return;

            // 연타 중
            //if (isOnStream && judgeType == JudgeType.EarlyMiss)
            //{
            //    score += 100;
            //    combo = 1;
            //    playerManager.Operate((Direction)UnityEngine.Random.Range(1, 5), touchType);
            //    playerManager.PlayerParrySound(touchType);
            //    return;
            //}

            playerManager.Operate(touchDirection, touchType);
            playerManager.PlayerParrySound(touchType);
            return;
        }

        // 플레이어의 조작이 없는 경우 제외
        if (!isPassing || judgeable.attackType == AttackType.HoldStop)
        {
            playerManager.Operate(judgeable.noteDirection, judgeable.attackType);
        }

        // 점수 부여
        switch (judgeType)
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

            case JudgeType.EarlyMiss:
                score += 0;
                combo = 0;
                break;
        }

        // index로 한번에 처리
        judgeDetails[0][(int)judgeType] += 1;
        if (!TutorialManager.isTutorial)
        {
            judgeDetails[(int)judgeable.noteDirection][(int)judgeType] += 1;
        }
        dynamicUIManager?.DisplayJudge((int)judgeType, judgeable.noteDirection);

        // 특정 Striker 찾기
        StrikerController targetStriker = judgeable.strikerController;
        CameraMoving cameraEffect = GameObject.Find("Main Camera").GetComponent<CameraMoving>();

        // Miss일 때 피격 처리
        if (judgeType == JudgeType.LateMiss || judgeType == JudgeType.EarlyMiss)
        {
            playerManager.hp--;
            playerManager.PlayerHitSound();
            dynamicUIManager?.DisplayHP(playerManager.hp, false);

            // 피격 → 사망
            if (playerManager.hp <= 0 && !TutorialManager.isTutorial)
            {
                dynamicUIManager?.HideAll();
                StageFlowManager.Instance?.GameOver();
            }
            // 피격 → 생존
            else
            {
                dynamicUIManager?.ShowDamageOverlayEffect();
                cameraEffect?.CameraShake();
            }
        }
        // 가드 시 처리
        else if (judgeType == JudgeType.LateBlocked || judgeType == JudgeType.EarlyBlocked)
        {
            dynamicUIManager?.DisplayScore(score);
            playerManager.PlayerBlockedSound();
        }
        // 패링 성공 시 처리
        else if (judgeType >= JudgeType.LateParried && judgeType <= JudgeType.EarlyParried)
        {
            // 투사체가 있는 공격이라면 되돌려 보냄
            if (parriedProjectileManager != null && judgeable.judgeableObject != null)
                //&& judgeable.attackType != AttackType.StreamStart)
            {
                // 보스 스트라이커 - 위로 반격
                if (targetStriker.boss != null)
                {
                    int fixRandom;
                    if (judgeable.noteDirection == Direction.Up || judgeable.noteDirection == Direction.Right)
                    {
                        fixRandom = 1;
                    }
                    else
                    {
                        fixRandom = 2;
                    }
                    parriedProjectileManager.ParryTusache(Direction.Up, (int)judgeable.attackType, fixRandom);
                }
                // 일반 스트라이커 - 날아온 방향으로 반격
                else
                {
                    parriedProjectileManager.ParryTusache(judgeable.noteDirection, (int)judgeable.attackType);
                }
            }
        }

        // 대상 노트 제거
        bool isParried = (judgeType >= JudgeType.LateParried && judgeType <= JudgeType.EarlyParried);
        FinishJudge(judgeable, isParried);
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
