using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renProjectile : MonoBehaviour
{
    // 연타 투사체에 들어가는 script
    public Transform target; // 플레이어 위치 (중앙)
    public float speed = 5.0f; // 노트 이동 속도
    public StrikerController owner; // 상위 striker
    public ScoreManager scoreManager;
    public float bpm;
    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 0.6f 거리 목표 위치
    public float minMoveTime = 0.4f;
    public float maxMoveTime = 0.6f;
    public float moveTimeMultiplier = 1f;
    private float startTime;
    private float moveTime;
    public float genRadius = 2.0f; // 생성 반지름
    
    [Header("복제 설정")]
    public float minTime = 0.05f; // 최소 복제 간격 (초)
    public float maxTime = 0.15f; // 최대 복제 간격 (초)
    public float genTimeMultiplier = 1f;
    
    private float nextCloneTime; // 다음 복제 시간
    private bool hasCloned = false; // 이미 복제했는지 여부

    public int type;


    void Start()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            scoreManager = gameController.GetComponent<ScoreManager>();
            bpm = owner.bpm;
        }

        // target을 중심으로 genRadius 반지름인 원의 둘레 위의 랜덤한 점에 위치
        SetRandomPositionOnCircle();

        startPosition = transform.position;
        // 목표 위치 설정 (플레이어에서 0.6f 거리)
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        targetPosition = target.position - directionToPlayer * 0.6f;

        nextCloneTime = Random.Range(minTime, maxTime) * genTimeMultiplier;
        startTime = StageManager.Instance.currentTime;
        moveTime = Random.Range(minMoveTime, maxMoveTime) * moveTimeMultiplier;
    }

    void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        
        // 복제 로직 - 한 번만 복제
        if (!hasCloned && currentTime >= nextCloneTime + startTime)
        {
            CreateClone();
            hasCloned = true; // 복제 완료 표시
        }
        
        // Lerp를 이용해 투사체 이동 (0.6f 지점까지)
        float fractionOfJourney = 1f - (moveTime + startTime - currentTime) / moveTime;
        transform.position = Vector3.Lerp(targetPosition, startPosition, fractionOfJourney);

        if (fractionOfJourney <= 0f)
        {
            Destroy(this.gameObject);
        }
    }

    /// target을 중심으로 genRadius 반지름인 원의 둘레 위의 랜덤한 점에 오브젝트를 위치
    private void SetRandomPositionOnCircle()
    {
        // 0도부터 360도까지 랜덤한 각도 생성
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // 원의 둘레 위의 점 계산 (2D 평면에서)
        float x = target.position.x + genRadius * Mathf.Cos(randomAngle);
        float z = target.position.z + genRadius * Mathf.Sin(randomAngle);
        
        // Y 좌표는 target과 동일하게 유지
        Vector3 randomPosition = new Vector3(x, target.position.y, z);
        
        // 오브젝트 위치 설정
        transform.position = randomPosition;
    }
    
    /// 현재 오브젝트를 복제하여 새로운 인스턴스를 생성
    private void CreateClone()
    {
        // 현재 오브젝트를 복제
        GameObject clone = Instantiate(this.gameObject);
        
        // 복제본의 renProjectile 컴포넌트 가져오기
        renProjectile cloneScript = clone.GetComponent<renProjectile>();
        
        if (cloneScript != null)
        {
            cloneScript.target = target; // 플레이어를 타겟으로 설정
            cloneScript.owner = owner;   // 소유자로 현재 스트라이커 설정
            cloneScript.type = 5;

            cloneScript.moveTimeMultiplier = moveTimeMultiplier;
            cloneScript.genTimeMultiplier = genTimeMultiplier;
        }
    }
}
