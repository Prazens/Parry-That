using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        KeyChecker();
    }

    // 임시 조작 확인기
    private void KeyChecker()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerManager.ShieldMove(Direction.Left);
            // scoreManager.Judge(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            playerManager.ShieldMove(Direction.Right);
            // scoreManager.Judge(Direction.Right);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playerManager.ShieldMove(Direction.Up);
            scoreManager.Judge(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playerManager.ShieldMove(Direction.Down);
            scoreManager.Judge(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.Space))  // 방향 스와이프 없이 탭만 할때의 움직임
        {
            playerManager.ShieldMove(Direction.None);
            scoreManager.Judge(Direction.None);
        }
    }

    // Touch Checker
    private void TouchChecker()
    {
        // To be developed
    }
}
