/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeStriker : MonoBehaviour
{
    //근거리 striker용
    public PlayerManager playerManager; // Player 정보 저장
    public UIManager uiManager;
    [SerializeField] public ChartData chartData; // 채보 데이터
    [SerializeField] public int hp; // 스트라이커 HP
    private int initialHp; // 스트라이커 initialHp
    [SerializeField] public int bpm; // BPM
    public Direction location; // 위치 방향
    private int currentNoteIndex = 0; // 현재 채보 인덱스
    [SerializeField] private Animator animator;
    // 임시로 발사체 저장해놓을 공간
    private Queue<Tuple<float, int>> prepareQueue = new Queue<Tuple<float, int>>(); // (arriveTime, type) 저장
    public GameObject hpBarPrefab;
    private GameObject hpBar;
    private Transform hpControl;

    //공격 준비비 시의 !관련
    [SerializeField] private GameObject exclamationPrefab; // 공통 느낌표 프리팹
    private Transform exclamationParent; // 느낌표 표시 위치
    private List<GameObject> prepareExclamation = new List<GameObject>(); // 느낌표 오브젝트 저장

    //준비 효과음
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip prepareSoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip prepareSoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //패링 효과음
    [SerializeField] private AudioClip parrySoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip parrySoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //근접 관련
    private Vector3 originalPosition; // 초기 위치 저장
    private Vector3 targetPosition;
    private bool isMoved = false;
    public float moveTime = 0.2f;
    private float backtime = 0f;

    private void Start()
    {
        backtime = 0f;
        originalPosition = transform.position; // 초기 위치 저장
        SetupExclamationParent();// exclamationParent 자동 생성
        //움직일 targetPosition 설정정
        targetPosition = playerManager.transform.position;
        switch (location)
        {
            case Direction.Up:
                targetPosition += Vector3.down * 1.5f;
                break;
            case Direction.Down:
                targetPosition += Vector3.up * 1.5f;
                break;
            case Direction.Left:
                targetPosition += Vector3.right * 1.5f;
                break;
            case Direction.Right:
                targetPosition += Vector3.left * 1.5f;
                break;
        }
    }
    private void Update() // 현재 striker 자체에서 투사체 일정 간격으로 발사
    {
        // 투사체 발사 타이밍 계산
        if (currentNoteIndex >= chartData.notes.Length) return;

        // 현재 시간 가져오기
        float currentTime = StageFlowManager.Instance.currentTime;
        // 1️⃣ `prepareTime` 확인 → 준비 상태 활성화 & `arriveTime`과 `type` 저장
        if (currentNoteIndex < chartData.notes.Length && currentTime >= chartData.notes[currentNoteIndex].time * (60f / bpm) + playerManager.musicOffset)
        {
            PrepareForAttack();
        }

        // 공격 이전에 미리 이동
        //chardata를 바꿔서 대기시간 받아야할듯 - 대기시간 - 0.2초로 출발(movetime)
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset - 1f - moveTime && !isMoved)
        {
            float fractionOfJourney = (prepareQueue.Peek().Item1 * (60f / bpm) + playerManager.musicOffset  - 1f - currentTime) / moveTime;

            if (fractionOfJourney > 0f)
            {
                transform.position = Vector3.Lerp(targetPosition, originalPosition, fractionOfJourney);
            }
            else
            {
                fractionOfJourney = 0f;
                transform.position = Vector3.Lerp(targetPosition, originalPosition, fractionOfJourney);
                isMoved = true;
            }
        }
        // 채보 시간에 맞춰 공격
        if (prepareQueue.Count > 0 && currentTime >= (prepareQueue.Peek().Item1 * (60d / bpm)) + playerManager.musicOffset && isMoved)
        {
            // 공격
            //근접 전용의 scoreManager의 judge를 이용해야함. projectile과 구분해서 애니메이션도 다르게 되어야한다.


            //공격 애니메이션 작용


            // 공격격 시 느낌표 제거 (좌측부터)
            if (prepareExclamation.Count > 0)
            {
                Destroy(prepareExclamation[0]); // 가장 오래된 느낌표 제거
                prepareExclamation.RemoveAt(0);

                // 남은 느낌표 위치 재배치
                for (int i = 0; i < prepareExclamation.Count; i++)
                {
                    prepareExclamation[i].transform.localPosition = new Vector3(i * 0.3f, 0, 0);
                }
            }
            prepareQueue.Dequeue(); // 준비된 공격 제거
        }
        //공격 종료시 원위치로 이동
        if(prepareQueue.Count == 0 && isMoved)
        {
            if(backtime == 0f)
            {
                backtime = currentTime;
            }
            if(currentTime <= backtime + moveTime)
            {
                float fraction = (currentTime - backtime) / moveTime;
                transform.position = Vector3.Lerp(targetPosition, originalPosition,fraction);
            }
            else
            {
                transform.position = originalPosition;
                isMoved = false;
                backtime = 0f;
            }
        }

    }
    private void PrepareForAttack()
    {
        float arriveTime = chartData.notes[currentNoteIndex].arriveTime;
        int noteType = chartData.notes[currentNoteIndex].type; // 노트 타입 저장

        prepareQueue.Enqueue(new Tuple<float, int>(arriveTime, noteType)); // 도착 시간과 타입 저장
        ShowExclamation(noteType); // 느낌표 표시
        // Debug.Log("prepare!");

        // 애니메이션 실행 (느낌표 표시)
        // **애니메이션 실행 (공격 준비)**
        animator.SetTrigger("isPrepare");
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
            newParent.transform.localPosition = new Vector3(1.8f, 0.0f, 0.0f); // Striker의 오른쪽에 배치
            exclamationParent = newParent.transform;
        }
    }
    private void ShowExclamation(int type)
    {
        GameObject newExclamation = Instantiate(exclamationPrefab, exclamationParent);
        newExclamation.transform.localPosition = new Vector3(prepareExclamation.Count * 0.3f, 0, 0); // 왼쪽부터 배치

        // 색상 변경
        SpriteRenderer exclamationSprite = newExclamation.GetComponent<SpriteRenderer>();
        if (exclamationSprite != null)
        {
            switch (type)
            {
                case 0:  // 일반 공격
                    exclamationSprite.color = Color.yellow;
                    break;
                case 1:  // 강공격
                    exclamationSprite.color = Color.red;
                    break;
                default: // 예외 처리
                    exclamationSprite.color = Color.white; // 기본값
                    break;
            }
        }

        prepareExclamation.Add(newExclamation);
    }

    public void Initialize(int _initialHp, int initialBpm, PlayerManager targetPlayer, Direction direction, ChartData chart) //striker 정보 초기화(spawn될 때 얻어오는 정보보)
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
    }

    public void TakeDamage(int damage, AttackType type)
    {
        if (audioSource != null)
        {
            if (type == AttackType.Normal && prepareSoundNormal != null)
            {
                audioSource.PlayOneShot(parrySoundNormal);
            }
            else if (type == AttackType.Strong && prepareSoundStrong != null)
            {
                audioSource.PlayOneShot(parrySoundStrong);
            }
        }
        if (hp >= 0)
        {
            hpControl.transform.localScale = new Vector3(1 - ((float)hp / initialHp), 1, 1);

            hp -= damage;
            // Debug.Log($"{gameObject.name} took {damage} damage! Current HP: {hp}");
            animator.SetTrigger("isDamaged");
            if (hp == 0)
            {
                //animator.SetBool("isClear", true);
                //애니메이션 나중에 구현
                playerManager.hp =+ 1;
            }
        }
    }
}
*/
