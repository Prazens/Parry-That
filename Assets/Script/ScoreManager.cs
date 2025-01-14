using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public List<GameObject> strikerList_;
    public PlayerManager playerManager;
    public StrikerManager strikerManager;
    public int combo = 0;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        combo = 0;
        strikerList_ = strikerManager.strikerList;
    }

    // 임시 판정
    public void Judge(Direction direction)
    {
        StrikerController strikerController;
        Vector3 projectileLocation;
        double distance;
        Direction touchDirection = (direction == Direction.None) ? playerManager.currentDirection : direction;

        // 스트라이커마다 탐지
        foreach (GameObject striker in strikerList_)
        {
            strikerController = striker.GetComponent<StrikerController>();

            // 터치 방향과 맞는 방향에서 공격하는 스트라이커라면
            if (strikerController.location == touchDirection)
            {
                projectileLocation = strikerController.projectileQueue.Peek().transform.position;
                distance = Math.Abs(projectileLocation.x + projectileLocation.y);
                
                
                // 거리에 따라 판정
                if (distance >= 1.5d)
                {
                    return;
                }
                else if (distance >= 0.9d)
                {
                    Debug.Log("Fast");
                }
                else if (distance >= 0.3d)
                {
                    Debug.Log("Perfect");
                }
                else
                {
                    Debug.Log("Late");
                }

                Destroy(strikerController.projectileQueue.Dequeue());
            }
        }
    }
}
