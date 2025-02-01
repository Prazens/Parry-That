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
    public int bpm;
    private float moveLength;
    private float calcSpeed;
    private Vector3 directionVector;

    void Start()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            scoreManager = gameController.GetComponent<ScoreManager>();
            bpm = owner.bpm;
            if (scoreManager != null)
            {
                Debug.Log("projectile successfully linked with ScoreManager.");
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

        // 임시 도착시간 설정
        noteData.arriveTime = noteData.time + 0.5f * (bpm / 60f);
        moveLength = 3.4f - owner.playerManager.visualOffset;
        calcSpeed = moveLength / (noteData.arriveTime - noteData.time) * (bpm / 60f);

        switch (owner.location)
        {
            case Direction.Up:
                directionVector = Vector3.down;
                break;

            case Direction.Down:
                directionVector = Vector3.up;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = Vector3.MoveTowards(transform.position, target.position, calcSpeed * Time.deltaTime); // player를 향해서 이동
        transform.position += directionVector * calcSpeed * Time.deltaTime;

        if (StageManager.Instance.currentTime > noteData.arriveTime * (60f / bpm) + scoreManager.musicOffset + 0.12d)
        {
            Debug.Log($"미조작 판정 수행 : {StageManager.Instance.currentTime - noteData.arriveTime * (60f / bpm) - scoreManager.musicOffset:3F} -> \"늦은 MISS\"");
            
            scoreManager.JudgeManage(owner.location, 0, (AttackType)noteData.type, owner, true);
        }

        // 일정거리 가까워지면 destroy로 임시 구현
        // if (Vector3.Distance(transform.position, target.position) <= 0.6f)
        // {

        //     // 저장 해제
        //     Destroy(owner.projectileQueue.Dequeue());
        // }
    }
}
