using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public ScoreManager scoreManager;
    public int hp;
    public Direction currentDirection = Direction.Up;  // 플레이어 방향
    public GameObject shield;
    public bool isShieldMoving = false;

    private Vector3[] directionMove = { Vector3.zero, Vector3.up, Vector3.down, Vector3.left, Vector3.right };

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

    

}
