using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private StrikerManager strikerManager;
    // Start is called before the first frame update
    void Start()
    {
        strikerManager.SpawnStriker(0,10,120); // 위쪽에 체력10, bpm120인 striker 소환
        strikerManager.SpawnStriker(1,15,120); // 아래쪽에 체력 15, bpm120인 striker 소환환
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
