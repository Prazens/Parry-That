using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StrikerController : MonoBehaviour
{
    [Header("Striker Components")]
    [SerializeField] private StrikerAnim anim;
    [SerializeField] private StrikerSound sound;

    public StrikerAnim Anim => anim;
    public StrikerSound Sound => sound;

    // striker 자체에 들어가는 script
    [SerializeField] private List<GameObject> projectilePrefabs; // 투사체 프리팹
    public PlayerManager playerManager; // Player 정보 저장
    public DynamicUIManager dynamicUIManager;
    [SerializeField] public ChartData chartData; // 채보 데이터
    [SerializeField] public int hp; // 스트라이커 HP
    private int initialHp; // 스트라이커 initialHp
    [SerializeField] public float bpm; // BPM
    public Direction location; // 위치 방향
    public int currentNoteIndex = 0; // 현재 채보 인덱스
    [SerializeField] private Animator animator;

    [SerializeField] private Animator holdSpriteAnimator = null;
    [SerializeField] private Animator bladeAnimator = null;
    // 임시로 발사체 저장해놓을 공간
    private float lastProjectileTime = 0f; // 마지막 투사체 발사 시간

    [SerializeField] private JudgeSystem judgeSystem;

    private struct PrepareEntry
    {
        public float arriveTime;
        public AttackType attackType;
        public PrepareEntry(float _arriveTime, AttackType _attackType)
        {
            arriveTime = _arriveTime;
            attackType = _attackType;
        }
    }
    private Queue<PrepareEntry> prepareQueue = new(); // (arriveTime, type) 저장

    public GameObject hpBarPrefab;
    public GameObject hpBar;
    public Transform hpControl;

    //투사체 발사 시의 !관련
    [SerializeField] private GameObject exclamationPrefab; // 공통 느낌표 프리팹
    // 🔹 `List<Sprite>`로 변경 (느낌표 타입별 이미지 저장)
    [SerializeField] private List<Sprite> exclamationSprites = new List<Sprite>();
    private Transform exclamationParent; // 느낌표 표시 위치
    private List<GameObject> prepareExclamation = new List<GameObject>(); // 느낌표 오브젝트 저장
    public GameObject holdExclamation; // 홀드 느낌표

    // 근접 공격 관련 변수
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoved = false;
    private bool isMoving = false;
    public float moveTime = 0.1f;
    private float backtime = 0f;
    public bool isMelee; // 근접 공격 여부 확인
    public float animeOffset = 0.02f;
    private float spacing = 0.25f;
    private float musicOffset;


    private bool isHolding = false;
    private bool isRenta = false;

    //spawn후 입장장 변수
    private float moveDuration = 1.0f; // 이동 시간
    private float spawnOffset = 3.0f; // 화면 밖에서 등장하는 거리
    private Vector3 spawnPosition;

    [SerializeField] private ParticleSystem particleSystemGreen;  // 🔹 초록색 파티클 시스템

    public BossController boss;
    public bool isBossMinion = false;


    private void Start()
    {
        backtime = 0f;
        originalPosition = transform.position;
        SetupExclamationParent();// exclamationParent 자동 생성
        if (isMelee)
        {
            SetMeleeTargetPosition();
        }

        anim.SetDirection((int)location);

        // 초기 위치를 화면 밖으로 설정
        spawnPosition = GetSpawnPosition();
        // 스트라이커를 화면 밖에서 시작 위치로 이동
        transform.position = spawnPosition;

        // 화면 밖에서 targetPosition으로 이동
        StartCoroutine(MoveToOriginalPosition());
        musicOffset = PlayerPrefs.GetFloat("musicOffset", 2);

        playerManager = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();
    }
    private void Update() // 현재 striker 자체에서 투사체 일정 간격으로 발사
    {
        if (isMelee)
        {
            HandleMeleeMovement();
        }
        else
        {
            HandleProjectileAttack();
        }

        // 투사체 발사 타이밍 계산
        if (currentNoteIndex >= chartData.notes.Length) return;

        // 현재 시간 가져오기
        float currentTime = StageFlowManager.Instance.currentTime;
        // `prepareTime` 확인 → 준비 상태 활성화 & `arriveTime`과 `type` 저장
        if (currentNoteIndex < chartData.notes.Length && currentTime >= chartData.notes[currentNoteIndex].time * (60f / bpm) + musicOffset)
        {
            PrepareForAttack();
        }
    }
    private void HandleMeleeMovement()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        //공격 이전에 출발
        if (prepareQueue.Count > 0 && currentTime >= ((prepareQueue.Peek().arriveTime * (60d / bpm)) + musicOffset - animeOffset - moveTime) && !isMoved &&
            prepareQueue.Peek().attackType != AttackType.HoldFinishStrong && !isMoving)
        {
            isMoving = true;
            StartCoroutine(MeleeGo(prepareQueue.Peek().arriveTime * (60f / bpm) + musicOffset - animeOffset));
        }

        // 채보 시간에 맞춰 공격
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().arriveTime * (60d / bpm)) + musicOffset - animeOffset)
        {
            AttackType attackType = (AttackType)prepareQueue.Peek().attackType;
            float attackTime = prepareQueue.Peek().arriveTime;

            // 공격
            //근접 전용의 scoreManager의 judge를 이용해야함. projectile과 구분해서 애니메이션도 다르게 되어야한다.
            // 투사체 저장은 PrepareForAttack에서 미리함

            anim.SetAttackType((int)attackType);

            //공격 애니메이션 작용
            if (attackType != AttackType.HoldFinishStrong)
            {
                if (attackType == AttackType.HoldStart)
                {
                    animator.SetBool("isAttacking", true);
                    transform.GetChild(0).transform.localPosition = DirTool.TranstoVec(DirTool.ReverseDir(location)) * 2f;
                }
                else
                {
                    int randomNum = UnityEngine.Random.Range(0, 2);
                    animator.SetInteger("randomSelecter", randomNum);
                    bladeAnimator.SetInteger("randomSelecter", randomNum);
                    animator.SetTrigger("Attack");
                    bladeAnimator.SetTrigger("bladePlay");
                }
            }
            exclamationRelocation();
            prepareQueue.Dequeue(); // 준비된 공격 제거
            if (attackType != AttackType.HoldFinishStrong && attackType != AttackType.HoldStart && prepareQueue.Count == 0)
            {
                StartCoroutine(WaitAndGoBack());
            }
        }
    }
    private IEnumerator WaitAndGoBack()
    {
        //애니메이션 지속 시간을 고려하여 대기 후 실행
        yield return new WaitForSeconds(0.1f); // 공격 후 0.1초 딜레이

        if (!isMoving && isMoved) // 중복 실행 방지
        {
            isMoved = false;
            isMoving = true;
            StartCoroutine(MeleeGoBack());
        }
    }
    public void ActMeleeHit()
    {
        if (prepareQueue.Count == 0 && isMoved)
        {
            isMoved = false;
            isMoving = true;
            StartCoroutine(MeleeGoBack());
        }
    }

    public void ActMeleeHoldStart()
    {
        // Debug.Log($"ActMeleeHoldStart {judgeableQueue.Peek().arriveBeat} {bpm} {StageFlowManager.Instance.currentTime}");
        bladeAnimator.SetTrigger("bladePlay");

        sound.PlayHoldStart();

        dynamicUIManager.CutInDisplay(judgeSystem.PeekJudgeable(location).arriveBeat * (60f / bpm) - StageFlowManager.Instance.currentTime + musicOffset);

        // StartCoroutine(MeleeHoldStartAnim());
        isHolding = true;
    }

    public void ActMeleeHoldFinish()
    {
        // Debug.Log("ActMeleeHoldFinish");
        animator.SetBool("isAttacking", false);
        bladeAnimator.SetTrigger("bladeHoldFinish");

        sound.PlayHoldEnd();

        transform.GetChild(0).transform.localPosition = Vector3.zero;
        isHolding = false;

        holdExclamation.GetComponent<holdExclamation>().ForceStop();

        // 미스났는데도 느낌표 남아있는 거 방지
        while (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]); // 가장 오래된 느낌표 제거
            prepareExclamation.RemoveAt(0);
        }
        if (prepareQueue.Count != 0)
        {
            prepareQueue.Dequeue(); // 준비된 공격 제거
        }
        ActMeleeHit();
        dynamicUIManager.CutInDisplay(0, true);
    }

    public void ActStreamStart()
    {
        sound.PlayHoldStart();

        isRenta = true;
        FireRenTusache();
    }

    private void ActStreamFinish()
    {
        // 연타 종료 시의 처리, Judgeable의 onDestroy에 저장 후 호출
        // (streamstart 일 때만, streamend는 그냥 끝 시간 알림용, 별도 판정 처리 없음)
        sound.PlayHoldEnd();
        
        isRenta = false;

        holdExclamation?.GetComponent<holdExclamation>()?.ForceStop();
        while (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]);
            prepareExclamation.RemoveAt(0);
        }
        if (prepareQueue.Count != 0) prepareQueue.Dequeue();
    }

    private IEnumerator StreamHoldAnim()
    {
        // 연타 중의 애니메이션? 표시
        yield return null;
    }

    private int CalcStreamCountForThisSegment(int startIdx)
    {
        float startBeat = chartData.notes[startIdx].arriveTime;
        int finishIdx = startIdx + 1;
        float finishBeat = (finishIdx < chartData.notes.Length)
            ? chartData.notes[finishIdx].arriveTime
            : startBeat;

        float beats = Mathf.Max(0f, finishBeat - startBeat);
        int hitsPerBeat = 4; // 기획값으로 설정
        return Mathf.Max(1, Mathf.RoundToInt(beats * hitsPerBeat));
    }

    private IEnumerator MeleeGo(float targetTime)
    {
        animator.SetBool("MovingGo", true);
        // Debug.Log("melee go 호출");
        // Debug.Log(isMoved);
        while (!isMoved)
        {
            float currentTime = StageFlowManager.Instance.currentTime;

            float fraction = (targetTime - currentTime) / moveTime;
            transform.position = Vector3.Lerp(targetPosition, originalPosition, Mathf.Clamp01(fraction));

            if (fraction <= 0f)
            {
                animator.SetBool("MovingGo", false);
                isMoved = true;
                // Debug.Log("isMove true in melee go");
                isMoving = false;
                transform.position = targetPosition;
                yield break;
            }
            yield return null;
        }
        yield break;
    }

    private IEnumerator MeleeGoBack()
    {
        Debug.Log("MeleeGoBack");
        if (hp == 0)
        {
            animator.SetBool("hp0", true);
            moveTime = 0.6f;
        }
        else animator.SetBool("MovingBack", true);
        while (isMoving)
        {
            float currentTime = StageFlowManager.Instance.currentTime;

            if (backtime == 0f) backtime = currentTime;
            float fraction = (currentTime - backtime) / (moveTime / 2);
            transform.position = Vector3.Lerp(targetPosition, originalPosition, Mathf.Clamp01(fraction));
            if (fraction >= 0.99f)
            {
                // Debug.Log("fraction if문 진입");
                backtime = 0f;
                transform.position = originalPosition;
                if (hp == 0)
                {
                    beCleared();
                }
                else
                {
                    animator.SetBool("MovingBack", false);
                    // Debug.Log("moveBack False");
                }
                isMoving = false;
                // Debug.Log("isMoving false");
            }
            yield return null;
        }
        yield break;
    }

    private IEnumerator MeleeHoldStartAnim()
    {
        // Debug.Log("MeleeHoldStartAnim");
        while (isMoved)
        {
            float currentTime = StageFlowManager.Instance.currentTime;

            if (backtime == 0f) backtime = currentTime;
            float fraction = (currentTime - backtime) / (moveTime / 3);
            transform.position = Vector3.Lerp(targetPosition, originalPosition, Mathf.Clamp01(fraction));

            if (fraction >= 1f)
            {
                isMoved = false;
                backtime = 0f;
                yield break;
            }
            yield return null;
        }
        yield break;
    }

    private void SetMeleeTargetPosition()
    {
        targetPosition = playerManager.transform.position;

        switch (location)
        {
            case Direction.Up:
                targetPosition += Vector3.up * 2f;
                break;
            case Direction.Down:
                targetPosition += Vector3.down * 2f;
                break;
            case Direction.Left:
                targetPosition += Vector3.left * 2f;
                break;
            case Direction.Right:
                targetPosition += Vector3.right * 2f;
                break;
        }
    }

    private void HandleProjectileAttack()
    {
        float currentTime = StageFlowManager.Instance.currentTime;

        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().arriveTime * (60d / bpm)) + musicOffset - 0.5f)
        {
            var prepare = prepareQueue.Peek();
            var (t, idx) = (prepare.arriveTime, prepare.attackType);
            if (idx == AttackType.HoldStart || idx == AttackType.HoldFinishStrong)
            {
                prepareQueue.Dequeue();     // 큐 소비
                exclamationRelocation();    // 느낌표 한 칸 제거
                lastProjectileTime = currentTime;
                return;
            }
            FireProjectile(t, (int)idx);
            prepareQueue.Dequeue();
            lastProjectileTime = currentTime;
        }
    }

    // StrikerController.cs
    private void ActRangeHoldStart()
    {
        sound.PlayHoldStart();
        if(holdSpriteAnimator != null)
        {
            holdSpriteAnimator.SetTrigger("holdStart");
        }

        dynamicUIManager.CutInDisplay(judgeSystem.PeekJudgeable(location).arriveBeat * (60f / bpm) - StageFlowManager.Instance.currentTime + musicOffset);

        isHolding = true;
    }

    private void ActRangeHoldFinish()
    {
        // 홀드 종료 연출
        sound.PlayHoldEnd();

        if (holdSpriteAnimator != null)
        {
            holdSpriteAnimator.SetTrigger("holdFinish");
        }

        isHolding = false;

        // 느낌표/큐 정리(근접과 동일 컨벤션)
        holdExclamation?.GetComponent<holdExclamation>()?.ForceStop();
        while (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]);
            prepareExclamation.RemoveAt(0);
        }
        if (prepareQueue.Count != 0) prepareQueue.Dequeue();

        // 컷인 제거
        dynamicUIManager?.CutInDisplay(0, true);
    }


    private void PrepareForAttack()
    {
        float arriveTime = chartData.notes[currentNoteIndex].arriveTime;
        AttackType noteType = (AttackType)chartData.notes[currentNoteIndex].type; // 노트 타입 저장

        if ((noteType != AttackType.HoldFinishStrong && noteType != AttackType.StreamStart) || isHolding || isRenta)
        {
            prepareQueue.Enqueue(new PrepareEntry(arriveTime, noteType)); // 도착 시간과 타입 저장
            ShowExclamation(noteType); // 느낌표 표시
        }

        if (isBossMinion && boss != null)
        {
            boss.OnMinionPrepare(location, (int)noteType, arriveTime);
        }

        if (isMelee)
        {
            if (noteType == AttackType.HoldStart)
            {
                judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.HoldStart, arriveTime, location, this, null, this.ActMeleeHoldStart));
                judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.HoldStop, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActMeleeHoldFinish));
            }
            else if (noteType != AttackType.HoldFinishStrong)
            {
                judgeSystem.EnqueueJudgeable(new Judgeable(noteType, arriveTime, location, this, null, this.ActMeleeHit));
            }
        }
        else
        {
            if (noteType == AttackType.HoldStart)
            {
                judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.HoldStart, arriveTime, location, this, null, this.ActRangeHoldStart));
                judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.HoldStop, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActRangeHoldFinish));
            }
            else if (noteType == AttackType.StreamStart)
            {
                var j = new Judgeable(AttackType.StreamStart, arriveTime, location, this, null, this.ActStreamStart);
                j.SetStreamCount(CalcStreamCountForThisSegment(currentNoteIndex));
                judgeSystem.EnqueueJudgeable(j);
                judgeSystem.EnqueueJudgeable(new Judgeable(AttackType.StreamFinish, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActStreamFinish));
            }
        }

        // 효과음 재생
        PlayPrepareSound(noteType);

        currentNoteIndex++; // 다음 노트로 이동
    }

    // 느낌표 생성 관련 함수
    private void SetupExclamationParent()
    {
        // `exclamationParent`가 없으면 자동 생성
        if (exclamationParent == null)
        {
            GameObject newParent = new GameObject("ExclamationHolder");
            newParent.transform.SetParent(this.transform);
            // **🔹 느낌표 기본 위치 설정**
            switch (location)
            {
                case Direction.Up:
                    newParent.transform.localPosition = new Vector3(1.5f, 0, 0);
                    break;
                case Direction.Down:
                    newParent.transform.localPosition = new Vector3(1.5f, 0, 0);
                    break;
                case Direction.Left:
                case Direction.Right:
                    newParent.transform.localPosition = new Vector3(0, -1.5f, 0);
                    break;
            }
            exclamationParent = newParent.transform;
        }
    }

    private void ShowExclamation(AttackType type)
    {
        //** 기존 느낌표 지우고 다시 생성**
        foreach (GameObject ex in prepareExclamation)
        {
            Destroy(ex);
        }
        int count = prepareQueue.Count; // 현재 준비된 공격 개수
        prepareExclamation.Clear();

        if (type == AttackType.HoldStart || type == AttackType.StreamStart)
        {
            if (SceneManager.GetActiveScene().name == "Stage4" ||
                SceneManager.GetActiveScene().name == "Stage5" ||
                SceneManager.GetActiveScene().name == "BossHoldTest")
                holdExclamation.GetComponent<holdExclamation>().Appear(bpm, 2);
            else
                holdExclamation.GetComponent<holdExclamation>().Appear(bpm, 1);
        }
        else if (type == AttackType.HoldFinishStrong || type == AttackType.StreamFinish)
        {
            if (SceneManager.GetActiveScene().name == "Stage4" ||
                SceneManager.GetActiveScene().name == "Stage5" ||
                SceneManager.GetActiveScene().name == "BossHoldTest")
                holdExclamation.GetComponent<holdExclamation>().Disappear(bpm, 2);
            else
                holdExclamation.GetComponent<holdExclamation>().Disappear(bpm, 1);

        }

        List<PrepareEntry> tempList = new List<PrepareEntry>(prepareQueue); // 현재 큐를 리스트로 변환 (순서 유지)


        for (int i = 0; i < count; i++)
        {
            Vector3 exclamationPosition = new Vector3((i + 1) * spacing, 0, 0); //(i - (count - 1) / 2f) 
            GameObject newExclamation = Instantiate(exclamationPrefab, exclamationParent);
            newExclamation.transform.localPosition = exclamationPosition; // **🔹 `exclamationParent` 기준 정렬**


            // **🔹 색상 변경 (공격 유형에 따라)**
            SpriteRenderer exclamationSprite = newExclamation.GetComponent<SpriteRenderer>();
            if (exclamationSprite != null)
            {
                int noteColor = (int)tempList[i].attackType;
                // 🔹 `type`이 `exclamationSprites` 범위 내에 있는지 확인
                if (noteColor >= 0 && noteColor < exclamationSprites.Count)
                {
                    exclamationSprite.sprite = exclamationSprites[noteColor]; // 리스트에서 해당 타입에 맞는 스프라이트 적용
                }
                else
                {
                    // Debug.LogWarning($"Unknown attack type {noteColor}! Defaulting to first sprite.");
                    exclamationSprite.sprite = exclamationSprites[0]; // 기본값
                }
            }

            prepareExclamation.Add(newExclamation);
        }
    }

    // 투사체 발사
    private void FireProjectile(float time, int index)
    {
        // Debug.Log("FireProjectile");
        if (index < 0 || index >= projectilePrefabs.Count) return;
        if (index != 0 && index != 1) return;
        GameObject selectedProjectile = projectilePrefabs[index];

        Vector3 projectilePos = transform.position;
        switch(location)
        {
            case Direction.Left:
                projectilePos += new Vector3(-2f, 0, 0);
                break;
            case Direction.Right:
                projectilePos += new Vector3(2f, 0, 0);
                break;
        }
        // 투사체 생성
        GameObject projectile = Instantiate(selectedProjectile, projectilePos, Quaternion.identity);
        switch (location)
        {
            case Direction.Up:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Left:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction.Right:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            default:
                break;
        }

        // 투사체 저장
        judgeSystem.EnqueueJudgeable(new Judgeable((AttackType)index, time, location, this, projectile));
        // // Debug.Log($"judgeableQueue의 길이:{judgeableQueue.Count}");

        // 투사체에 타겟 설정
        projectile projScript = projectile.GetComponent<projectile>();
        if (projScript != null)
        {
            if (playerManager == null) playerManager = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();
            projScript.target = playerManager.transform; // 플레이어를 타겟으로 설정
            projScript.owner = this;   // 소유자로 현재 스트라이커 설정
            projScript.arriveTime = time;
            projScript.type = index;
        }
        animator.SetTrigger("Attack");
        // ⭐ 발사 시 느낌표 제거 (좌측부터)
        exclamationRelocation();
    }

    // 연타 투사체 발사
    private void FireRenTusache()
    {
        GameObject selectedProjectile = projectilePrefabs[2];
        GameObject projectile = Instantiate(selectedProjectile, transform.position, Quaternion.identity);
        renProjectile projScript = projectile.GetComponent<renProjectile>();

        if (playerManager == null) playerManager = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();

        projScript.target = playerManager.transform; // 플레이어를 타겟으로 설정
        projScript.owner = this;   // 소유자로 현재 스트라이커 설정
        projScript.type = 5;

        projScript.moveTimeMultiplier = 60f / bpm;
        projScript.genTimeMultiplier = 60f / bpm;
    }

    private void exclamationRelocation()
    {
        if (prepareExclamation.Count > 0)
        {
            Destroy(prepareExclamation[0]); // 가장 오래된 느낌표 제거
            prepareExclamation.RemoveAt(0);
            int count = prepareExclamation.Count;
            // 남은 느낌표 위치 재배치
            for (int i = 0; i < count; i++)
            {
                prepareExclamation[i].transform.localPosition = new Vector3((i + 1) * spacing, 0, 0);
            }
        }
    }

    public void Initialize(int _initialHp, float initialBpm, PlayerManager targetPlayer, Direction direction, ChartData chart, int prepabindex) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
    {
        hp = _initialHp;
        initialHp = _initialHp;
        bpm = initialBpm;
        playerManager = targetPlayer;
        // Debug.Log($"{gameObject.name} spawned with HP: {hp}, BPM: {bpm}");
        location = direction;
        chartData = chart; // 채보 데이터 설정

        hpBar = Instantiate(hpBarPrefab, transform);
        hpBar.transform.localPosition = Vector3.down * 2f;
        hpControl = hpBar.transform.GetChild(0);
        hpControl.transform.localScale = new Vector3(0, 1, 1);
        if (isBossMinion) hpBar.SetActive(false);
        if (prepabindex == 0)
        {
            isMelee = false;
        }
        else isMelee = true;
    }

    private Vector3 GetSpawnPosition()
    {
        // 기본적으로 targetPosition을 유지
        Vector3 spawnPosition = originalPosition;

        // 화면 밖에서 등장하는 위치 설정
        switch (location)
        {
            case Direction.Up:
                spawnPosition += Vector3.up * spawnOffset;
                break;
            case Direction.Down:
                spawnPosition += Vector3.down * spawnOffset;
                break;
            case Direction.Left:
                spawnPosition += Vector3.left * spawnOffset;
                break;
            case Direction.Right:
                spawnPosition += Vector3.right * spawnOffset;
                break;
        }

        return spawnPosition;
    }

    private IEnumerator MoveToOriginalPosition()
    {
        float elapsedTime = 0;

        // 부드러운 이동을 위한 Lerp 적용
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 고정
        transform.position = originalPosition;
    }

    public void ClearProjectiles()
    {
        while (judgeSystem.CountJudgeable(location) > 0)
        {
            GameObject projectile = judgeSystem.DequeueJudgeable(location).judgeableObject;
            if (projectile != null)
            {
                Destroy(projectile); // Projectile 삭제
            }
        }
    }

    public void TakeDamage(int damage, AttackType type)
    {
        PlayParrySound(type);

        if (isBossMinion && boss != null)
        {
            // 자기 HP는 깎지 않고 보스에게 전달
            boss.TakeDamage(damage, type, location);
            Debug.Log("boss에게 데미지 전달");
            return;
        }
        if (hp >= 0)
        {
            hp -= damage;
            hpControl.transform.localScale = new Vector3(1 - ((float)hp / initialHp), 1, 1);

            // Debug.Log($"{gameObject.name} took {damage} damage! Current HP: {hp}");
            if (!isMelee) animator.SetTrigger("isDamaged");
            if (hp <= 0)
            {
                if (!isMelee) beCleared();
                if (playerManager.hp < 10)
                {
                    playerManager.hp += 1;
                }

                //기타몬 전용 굴러가기 퇴장
                //original position 도착후 isClear 세팅
                //이후 투명해지는 animation 진행
            }
        }
    }

    public void strikerExit()
    {
        if (hp != 0)
        {
            StartCoroutine(ExitToSpawnPosition());
        }
    }

    private IEnumerator ExitToSpawnPosition()
    {
        float elapsedTime = 0;

        // 부드러운 이동을 위한 Lerp 적용
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(transform.position, spawnPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 고정
        transform.position = spawnPosition;
        gameObject.SetActive(false);
    }

    public void beCleared()
    {
        animator.SetBool("isClear", true);
        if (isMelee) animator.SetTrigger("Cleared");
        PlayParticleEffect();
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // // 애니메이션 길이 가져오기
        // float exitAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;

        // // 애니메이션 실행 시간만큼 대기
        // yield return new WaitForSeconds(exitAnimationTime);
        //기타몬 애니메이션 길이 기준으로 그냥 2.5초 지정해버렸습니다.
        if (isMelee) yield return new WaitForSeconds(2.5f);
        else yield return new WaitForSeconds(1f);

        // 오브젝트 삭제
        // Destroy(gameObject);
        gameObject.SetActive(false);
    }

    // 🔹 초록색 파티클 실행 함수
    private void PlayParticleEffect()
    {
        if (particleSystemGreen != null)
        {
            particleSystemGreen.Play();
        }
    }

    private void PlayPrepareSound(AttackType type)
    {
        if (type == AttackType.Normal)
        {
            sound.PlayPrepareNormal();
        }
        else if (type == AttackType.Strong)
        {
            sound.PlayPrepareStrong();
        }
    }

    private void PlayParrySound(AttackType type)
    {
        if (type == AttackType.Normal)
        {
            sound.PlayParryNormal();
        }
        else if (type == AttackType.Strong)
        {
            sound.PlayParryStrong();
        }
    }
}
