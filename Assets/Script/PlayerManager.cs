using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public ScoreManager scoreManager;
    public int hp;
    public Direction currentDirection = Direction.Up;  // 쉴드 방향
    public StageManager stageManager; // StageManager 참조

    public Animator playerAnimator;
    public Animator bladeAnimator;

    private Transform direcrionDisplayer;

    public float musicOffset;
    // public float visualOffset;

    private AudioSource audioSource;
    [SerializeField] private AudioClip blocked;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip SwingStrong;
    [SerializeField] private AudioClip SwingWeak;
    [SerializeField] private AudioSource SmallaudioSource;
    [SerializeField] private AudioSource SwingaudioSource;

    // public string[] triggers = new string[2] { "playerParryUp", "playerParryDown" };

    // Start is called before the first frame update
    void Start()
    {
        musicOffset = PlayerPrefs.GetFloat("musicOffset", 2);
        // musicOffset = 2f;  // 판정 오프셋: -일수록 빨리 쳐야 함
        // visualOffset = 0.22f;  // 판정선: -일수록 플레이어에 가까움

        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            scoreManager = gameController.GetComponent<ScoreManager>();
            if (scoreManager != null)
            {
                Debug.Log("PlayerManager successfully linked with ScoreManager.");
            }
            else
            {
                Debug.LogError("ScoreManager script is not attached to GameController!");
            }
        }
        else
        {
            Debug.LogError("GameController not found!");
        }

        direcrionDisplayer = transform.GetChild(0);

        audioSource = GetComponent<AudioSource>();
    }

    public void Operate(Direction direction, AttackType type)
    {
        switch (direction)
        {
            case Direction.Up:
                direcrionDisplayer.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case Direction.Down:
                direcrionDisplayer.rotation = Quaternion.Euler(0, 0, 180);
                break;

            case Direction.Left:
                direcrionDisplayer.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case Direction.Right:
                direcrionDisplayer.rotation = Quaternion.Euler(0, 0, 270);
                break;
        }

        if (type == AttackType.HoldStart)
        {
            transform.GetChild(2).transform.position = DirTool.TranstoVec(direction) * 0.8f;
        }

        // 홀드 끝 모션
        else if (type == AttackType.HoldFinishStrong || type == AttackType.HoldStop)
        {
            Debug.Log($"Animation : {direction}, {type}");

            bladeAnimator.SetTrigger("bladeHoldFinish");
            playerAnimator.SetTrigger("playerHoldFinish");
            
            transform.GetChild(2).transform.position = Vector3.zero;
            return;
        }

        int randomNum;

        // 모션이 위아래는 3개, 좌우는 2개임
        if (direction == Direction.Up || direction == Direction.Down)
        {
            randomNum = UnityEngine.Random.Range(0, 3);
        }
        else
        {
            randomNum = UnityEngine.Random.Range(0, 2);
        }

        Debug.Log($"Animation : {direction}, {type}, {randomNum}");

        playerAnimator.SetInteger("attackType", (int)type);
        playerAnimator.SetInteger("parryDirection", (int)direction);

        playerAnimator.SetInteger("randomSelecter", randomNum);
        playerAnimator.SetTrigger("playerParryPlay");

        bladeAnimator.SetInteger("attackType", (int)type);
        bladeAnimator.SetInteger("bladeDirection", (int)direction);

        bladeAnimator.SetInteger("randomSelecter", randomNum);
        bladeAnimator.SetTrigger("bladePlay");

        return;
    }
    
    public void GameOver()
    {
        if (stageManager != null)
        {
            stageManager.GameOver(); // StageManager에 GameOver 호출
        }
    }

    public void PlayerParrySound(AttackType attackType)  // 헛스윙 사운드
    {
        switch(attackType)
        {
            case AttackType.Normal:
                audioSource.PlayOneShot(SwingWeak);
                break;
            case AttackType.Strong:
                SwingaudioSource.PlayOneShot(SwingStrong);
                break;
            default:
                break;
            // 홀드 스타트 실패 시 헛스윙 사운드? -> 일단 구현 X
        }
    }
    public void PlayerBlockedSound()
    {
        audioSource.PlayOneShot(blocked);
    }
    public void PlayerHitSound()
    {
        SmallaudioSource.PlayOneShot(hit);
    }
}
