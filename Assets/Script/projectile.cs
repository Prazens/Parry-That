using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    // 투사체에 들어가는 script
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker
    public NoteData noteData; // 노트의 정보
    public ScoreManager scoreManager;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime); // player를 향해서 이동

        if (StageManager.Instance.currentTime - noteData.arriveTime > 0.12d)
        {
            scoreManager.JudgeManage(owner.location, 0);
            Destroy(owner.projectileQueue.Dequeue());
        }

        // 일정거리 가까워지면 destroy로 임시 구현
        // if (Vector3.Distance(transform.position, target.position) < 0.1d)
        // {
        //     Debug.Log("Missed Note!");

        //     // 저장 해제
        //     owner.projectileQueue.Dequeue();

        //     Destroy(gameObject);
        // }
    }
}
