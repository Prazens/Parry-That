using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // ScoreManager -> JudgeSystem
    public JudgeSystem judgeSystem;

    public int hp;
    public Direction currentDirection = Direction.Up;  // 쉴드 방향

    // StageManager -> StageFlowManager
    public StageFlowManager stageFlowManager;

    public Animator playerAnimator;
    public Animator bladeAnimator;

    private Transform direcrionDisplayer;

    private AudioSource audioSource;
    [SerializeField] private AudioClip blocked;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip SwingStrong;
    [SerializeField] private AudioClip SwingWeak;
    [SerializeField] private AudioSource SmallaudioSource;
    [SerializeField] private AudioSource SwingaudioSource;

    void Start()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            judgeSystem = gameController.GetComponent<JudgeSystem>();
        }

        if (stageFlowManager == null)
        {
            stageFlowManager = FindObjectOfType<StageFlowManager>();
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
        else if (type == AttackType.HoldStop || type == AttackType.HoldStop)
        {
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
        if (stageFlowManager != null)
        {
            stageFlowManager.GameOver();
        }
    }

    public void PlayerParrySound(AttackType attackType)  // 헛스윙 사운드
    {
        switch (attackType)
        {
            case AttackType.Normal:
                audioSource.PlayOneShot(SwingWeak, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
                break;
            case AttackType.Strong:
                SwingaudioSource.PlayOneShot(SwingStrong, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
                break;
            default:
                break;
        }
    }

    public void PlayerBlockedSound()
    {
        audioSource.PlayOneShot(blocked, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
    }

    public void PlayerHitSound()
    {
        SmallaudioSource.PlayOneShot(hit, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
    }
}
