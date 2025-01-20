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

    public string[] triggers = new string[2] { "playerParryUp", "playerParryDown" };

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Operate(Direction direction, int type)
    {
        // Debug.Log($"{(int)direction} {triggers[(int)direction - 1]} {type}");

        playerAnimator.SetTrigger(triggers[(int)direction - 1]);

        bladeAnimator.SetInteger("attackType", type);
        bladeAnimator.SetTrigger("bladePlay");
        bladeAnimator.SetInteger("bladeDirection", (int)direction);
    }
    public void GameOver()
    {
        if (stageManager != null)
        {
            stageManager.GameOver(); // StageManager에 GameOver 호출
        }
    }
}
