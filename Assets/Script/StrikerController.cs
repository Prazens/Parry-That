using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class StrikerController : MonoBehaviour
{
    // striker 자체에 들어가는 script
    [SerializeField] private List<GameObject> projectilePrefabs; // 투사체 프리팹
    public PlayerManager playerManager; // Player 정보 저장
    public UIManager uiManager;
    [SerializeField] public ChartData chartData; // 채보 데이터
    [SerializeField] public int hp; // 스트라이커 HP
    private int initialHp; // 스트라이커 initialHp
    [SerializeField] public float bpm; // BPM
    public Direction location; // 위치 방향
    public int currentNoteIndex = 0; // 현재 채보 인덱스
    [SerializeField] private Animator animator;
    [SerializeField] private Animator bladeAnimator = null;
    // 임시로 발사체 저장해놓을 공간
    private float lastProjectileTime = 0f; // 마지막 투사체 발사 시간

    [SerializeField] public Queue<Judgeable> judgeableQueue = new Queue<Judgeable>{};
    private Queue<Tuple<float, int>> prepareQueue = new Queue<Tuple<float, int>>(); // (arriveTime, type) 저장

    public GameObject hpBarPrefab;
    private GameObject hpBar;
    private Transform hpControl;

    //투사체 발사 시의 !관련
    [SerializeField] private GameObject exclamationPrefab; // 공통 느낌표 프리팹
    // 🔹 `List<Sprite>`로 변경 (느낌표 타입별 이미지 저장)
    [SerializeField] private List<Sprite> exclamationSprites = new List<Sprite>(); 
    private Transform exclamationParent; // 느낌표 표시 위치
    private List<GameObject> prepareExclamation = new List<GameObject>(); // 느낌표 오브젝트 저장
    public GameObject holdExclamation; // 홀드 느낌표

    //준비 효과음
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip prepareSoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip prepareSoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //패링 효과음
    [SerializeField] private AudioClip parrySoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip parrySoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //패링 효과음
    [SerializeField] private AudioClip holdingSound;  // 홀드 중
    [SerializeField] private AudioClip holdingEnd;  // 홀드 끝

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


    private bool isHolding = false;

    //spawn후 입장장 변수
    private float moveDuration = 1.0f; // 이동 시간
    private float spawnOffset = 3.0f; // 화면 밖에서 등장하는 거리
    private Vector3 spawnPosition;

    private void Start()
    {
        backtime = 0f;
        originalPosition = transform.position;
        SetupExclamationParent();// exclamationParent 자동 생성
        if (isMelee)
        {
            SetMeleeTargetPosition();
        }
        
        animator.SetInteger("direction", (int)location);
        if (bladeAnimator != null)
        {
            bladeAnimator.SetInteger("bladeDirection", (int)location);
        }
        // 초기 위치를 화면 밖으로 설정
        spawnPosition = GetSpawnPosition();
        // 스트라이커를 화면 밖에서 시작 위치로 이동
        transform.position = spawnPosition;

        // 화면 밖에서 targetPosition으로 이동
        StartCoroutine(MoveToOriginalPosition());
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
        float currentTime = StageManager.Instance.currentTime;
        // 1️⃣ `prepareTime` 확인 → 준비 상태 활성화 & `arriveTime`과 `type` 저장
        if (currentNoteIndex < chartData.notes.Length && currentTime >= chartData.notes[currentNoteIndex].time * (60f / bpm) + playerManager.musicOffset)
        {
            PrepareForAttack();
        }
    }
    private void HandleMeleeMovement()
    {
        float currentTime = StageManager.Instance.currentTime;

        //공격 이전에 출발
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset - animeOffset - moveTime && !isMoved && prepareQueue.Peek().Item2 != 3 && !isMoving)
        {
            isMoving = true;
            StartCoroutine(MeleeGo(prepareQueue.Peek().Item1 * (60f / bpm) + playerManager.musicOffset - animeOffset));
        }

        // 채보 시간에 맞춰 공격
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset - animeOffset)
        {
            int attackType = prepareQueue.Peek().Item2;
            float attackTime = prepareQueue.Peek().Item1;

            // 공격
            //근접 전용의 scoreManager의 judge를 이용해야함. projectile과 구분해서 애니메이션도 다르게 되어야한다.
            // 투사체 저장은 PrepareForAttack에서 미리함

            animator.SetInteger("attackType", attackType);
            bladeAnimator.SetInteger("attackType", attackType);

            //공격 애니메이션 작용
            if (attackType != 3)
            {
                if (attackType == 2)
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
        }
    }

    public void ActMeleeHit()
    {
        if(prepareQueue.Count == 0) 
        {
            isMoved = false;
            isMoving = true;
            StartCoroutine(MeleeGoBack());
        }
    }

    public void ActMeleeHoldStart()
    {
        Debug.Log($"ActMeleeHoldStart {judgeableQueue.Peek().arriveBeat} {bpm} {StageManager.Instance.currentTime}");
        bladeAnimator.SetTrigger("bladePlay");

        audioSource.PlayOneShot(holdingSound);

        uiManager.CutInDisplay(judgeableQueue.Peek().arriveBeat * (60f / bpm) - StageManager.Instance.currentTime + playerManager.musicOffset);

        // StartCoroutine(MeleeHoldStartAnim());
        isHolding = true;
    }

    public void ActMeleeHoldFinish()
    {
        Debug.Log("ActMeleeHoldFinish");
        animator.SetBool("isAttacking", false);
        bladeAnimator.SetTrigger("bladeHoldFinish");
        
        audioSource.Stop();
        audioSource.PlayOneShot(holdingEnd);
        
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
        uiManager.CutInDisplay(0, true);
    }

    private IEnumerator MeleeGo(float targetTime)
    {
        animator.SetBool("MovingGo", true);
        Debug.Log("melee go 호출");
        Debug.Log(isMoved);
        while (!isMoved)
        {
            float currentTime = StageManager.Instance.currentTime;

            float fraction = (targetTime - currentTime) / moveTime;
            transform.position = Vector3.Lerp(targetPosition, originalPosition, Mathf.Clamp01(fraction));

            if (fraction <= 0f)
            {
                animator.SetBool("MovingGo", false);
                isMoved = true;
                Debug.Log("isMove true in melee go");
                isMoving = false;
                transform.position = targetPosition;
                yield break;
            }
            yield return null;
        }
    }
    
    private IEnumerator MeleeGoBack()
    {
        Debug.Log("MeleeGoBack");
        if(hp == 0) 
        {
            animator.SetBool("hp0", true);
            moveTime = 0.6f;
        }
        else animator.SetBool("MovingBack", true);
        Debug.Log(isMoving);
        while (isMoving)
        {
            Debug.Log("back while문 진입");
            float currentTime = StageManager.Instance.currentTime;

            if (backtime == 0f) backtime = currentTime;
            float fraction = (currentTime - backtime) / (moveTime / 3);
            transform.position = Vector3.Lerp(targetPosition, originalPosition, Mathf.Clamp01(fraction));
            if (fraction >= 0.99f)
            {
                Debug.Log("fraction if문 진입");
                backtime = 0f;
                transform.position = originalPosition;
                if(hp == 0)
                {
                    beCleared();
                }
                else
                {
                    animator.SetBool("MovingBack", false);
                    Debug.Log("moveBack False");
                }
                isMoving = false;
                Debug.Log("isMoving false");
            }
            yield return null;
        }
    }

    private IEnumerator MeleeHoldStartAnim()
    {
        Debug.Log("MeleeHoldStartAnim");
        while (isMoved)
        {
            float currentTime = StageManager.Instance.currentTime;

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
        float currentTime = StageManager.Instance.currentTime;

        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset - 0.5f)
        {
            FireProjectile(prepareQueue.Peek().Item1, prepareQueue.Peek().Item2);
            prepareQueue.Dequeue();
            lastProjectileTime = currentTime;
        }
    }
    private void PrepareForAttack()
    {
        float arriveTime = chartData.notes[currentNoteIndex].arriveTime;
        int noteType = chartData.notes[currentNoteIndex].type; // 노트 타입 저장

        if (noteType != 3 || isHolding)
        {
            prepareQueue.Enqueue(new Tuple<float, int>(arriveTime, noteType)); // 도착 시간과 타입 저장
            ShowExclamation(noteType); // 느낌표 표시
            Debug.Log("prepare!");
        }

        if (isMelee)
        {
            if (noteType == 2)
            {
                judgeableQueue.Enqueue(new Judgeable((AttackType)noteType, arriveTime, location, this, null, this.ActMeleeHoldStart));
                judgeableQueue.Enqueue(new Judgeable((AttackType)chartData.notes[currentNoteIndex + 1].type, chartData.notes[currentNoteIndex + 1].arriveTime, location, this, null, this.ActMeleeHoldFinish));
            }
            else if (noteType != 3)
            {
                judgeableQueue.Enqueue(new Judgeable((AttackType)noteType, arriveTime, location, this, null, this.ActMeleeHit));
            }
        }

        // 애니메이션 실행 (느낌표 표시)
        // **애니메이션 실행 (공격 준비)**
        //animator.SetTrigger("isPrepare");
        // **🔹 효과음 재생 (일반 / 강한 공격에 따라 다름)**
        PlayPrepareSound(noteType);

        currentNoteIndex++; // 다음 노트로 이동
    }
    private void PlayPrepareSound(int type)
    {
        if (audioSource != null)
        {
            if (type == 0 && prepareSoundNormal != null)
            {
                audioSource.PlayOneShot(prepareSoundNormal);
            }
            else if (type == 1 && prepareSoundStrong != null)
            {
                audioSource.PlayOneShot(prepareSoundStrong);
            }
        }
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
    private void ShowExclamation(int type)
    {
        //** 기존 느낌표 지우고 다시 생성**
        foreach (GameObject ex in prepareExclamation)
        {
            Destroy(ex);
        }
        int count = prepareQueue.Count; // 현재 준비된 공격 개수
        prepareExclamation.Clear();

        if (type == 2)
        {
            holdExclamation.GetComponent<holdExclamation>().Appear(bpm, 1);
            return;
        }
        else if (type == 3)
        {
            holdExclamation.GetComponent<holdExclamation>().Disappear(bpm, 1);
            return;
        }

        List<Tuple<float, int>> tempList = new List<Tuple<float, int>>(prepareQueue); // 현재 큐를 리스트로 변환 (순서 유지)


        for (int i = 0; i < count; i++)
        {
            Vector3 exclamationPosition = new Vector3( (i+1) * spacing, 0, 0); //(i - (count - 1) / 2f) 
            GameObject newExclamation = Instantiate(exclamationPrefab, exclamationParent);
            newExclamation.transform.localPosition = exclamationPosition; // **🔹 `exclamationParent` 기준 정렬**


            // **🔹 색상 변경 (공격 유형에 따라)**
            SpriteRenderer exclamationSprite = newExclamation.GetComponent<SpriteRenderer>();
            if (exclamationSprite != null)
            {
                int noteColor = tempList[i].Item2;
                // 🔹 `type`이 `exclamationSprites` 범위 내에 있는지 확인
                if (noteColor >= 0 && noteColor < exclamationSprites.Count)
                {
                    exclamationSprite.sprite = exclamationSprites[noteColor]; // 리스트에서 해당 타입에 맞는 스프라이트 적용
                }
                else
                {
                    Debug.LogWarning($"Unknown attack type {noteColor}! Defaulting to first sprite.");
                    exclamationSprite.sprite = exclamationSprites[0]; // 기본값
                }
            }

            prepareExclamation.Add(newExclamation);
        }
    }

    // 투사체 발사
    private void FireProjectile(float time, int index)
    {
        Debug.Log("FireProjectile");
        if (index < 0 || index >= projectilePrefabs.Count) return;
        GameObject selectedProjectile = projectilePrefabs[index];

        // 투사체 생성
        GameObject projectile = Instantiate(selectedProjectile, transform.position, Quaternion.identity);
        switch (location)
        {
            case Direction.Up:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.Left:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case Direction.Right:
                projectile.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            default:
                break;
        }

        // 투사체 저장
        judgeableQueue.Enqueue(new Judgeable((AttackType)index, time, location, this, projectile));
        // Debug.Log($"judgeableQueue의 길이:{judgeableQueue.Count}");

        // 투사체에 타겟 설정
        projectile projScript = projectile.GetComponent<projectile>();
        if (projScript != null)
        {
            projScript.target = playerManager.transform; // 플레이어를 타겟으로 설정
            projScript.owner = this;   // 소유자로 현재 스트라이커 설정
            projScript.arriveTime = time;
            projScript.type = index;
        }
        animator.SetTrigger("Attack");
        // ⭐ 발사 시 느낌표 제거 (좌측부터)
        exclamationRelocation();
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
                prepareExclamation[i].transform.localPosition = new Vector3((i +1) * spacing, 0, 0);
            }
        }
    }

    public void Initialize(int _initialHp, float initialBpm, PlayerManager targetPlayer, Direction direction, ChartData chart, int prepabindex) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
    {
        hp = _initialHp;
        initialHp = _initialHp;
        bpm = initialBpm;
        playerManager = targetPlayer;
        Debug.Log($"{gameObject.name} spawned with HP: {hp}, BPM: {bpm}");
        location = direction;
        chartData = chart; // 채보 데이터 설정

        hpBar = Instantiate(hpBarPrefab, transform);
        hpBar.transform.localPosition = Vector3.down * 2f;
        hpControl = hpBar.transform.GetChild(0);
        hpControl.transform.localScale = new Vector3(0, 1, 1);
        if(prepabindex == 0)
        {
            isMelee = false;
        }
        else isMelee = true;
    }
    private Vector3 GetSpawnPosition()
    {
        // 기본적으로 targetPosition을 유지
        Vector3 spawnPosition =  originalPosition;

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
        while (judgeableQueue.Count > 0)
        {
            GameObject projectile = judgeableQueue.Dequeue().judgeableObject;
            if (projectile != null)
            {
                Destroy(projectile); // Projectile 삭제
            }
        }
    }

    public void TakeDamage(int damage, AttackType type)
    {
        if (audioSource != null)
        {
            if (type == AttackType.Normal && parrySoundNormal != null)
            {
                audioSource.PlayOneShot(parrySoundNormal);
            }
            else if (type == AttackType.Strong && parrySoundStrong != null)
            {
                audioSource.PlayOneShot(parrySoundStrong);
            }
        }
        if (hp >= 0)
        {
            hpControl.transform.localScale = new Vector3(1 - ((float)hp / initialHp), 1, 1);

            hp -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage! Current HP: {hp}");
            if(!isMelee) animator.SetTrigger("isDamaged");
            if (hp <= 0)
            {
                if(!isMelee) beCleared();
                playerManager.hp += 1;
                
                //기타몬 전용 굴러가기 퇴장
                //original position 도착후 isClear 세팅
                //이후 투명해지는 animation 진행
            }
        }
    }
    public void strikerExit()
    {
        if(hp != 0)
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
        animator.SetTrigger("Cleared");
        StartCoroutine(DestroyAfterAnimation());
    }
    private IEnumerator DestroyAfterAnimation()
    {
        // // 애니메이션 길이 가져오기
        // float exitAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        
        // // 애니메이션 실행 시간만큼 대기
        // yield return new WaitForSeconds(exitAnimationTime);
        //기타몬 애니메이션 길이 기준으로 그냥 2.5초 지정해버렸습니다.
        if(isMelee) yield return new WaitForSeconds(2.5f);
        else yield return new WaitForSeconds(1f);

        // 오브젝트 삭제
        // Destroy(gameObject);
        gameObject.SetActive(false);
    }
}
