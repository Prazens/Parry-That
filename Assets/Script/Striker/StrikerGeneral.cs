using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class StrikerGeneral : MonoBehaviour
{
    // 스트라이커 부모 클래스 (추상)

    // 체력 관련
    private int initialHp;  // 현재와 초기 체력
    private int? _hp = null;
    public int? hp
    {
        get { return _hp; }
        set { if (!_hp.HasValue) { _hp = value; } }
        // hp에 직접 접근하게 하는 것보다 체력1깎기 같은 함수를 따로 만드는 것이 안전할 듯
    }

    public void StrikerHit()
    {
        _hp--;
        // hp가 0이 될 때?
    }


    // 체력바 관련 
    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private GameObject hpBar;
    [SerializeField] private Transform hpControl;

    private void DisplayHpBar()
    {

    }


    // 채보 관련
    private ChartData _chartData = null;
    public ChartData chartData
    {
        get { return _chartData; }
        set { if (_chartData == null) { _chartData = value; } }
    }

    private int _currentPrepareNoteIndex = 0; // 현재 준비 채보 인덱스
    public int currentPrepareNoteIndex
    {
        get { return currentPrepareNoteIndex; }
        // set { _currentPrepareNoteIndex = value; }
        // 진행 중인 채보를 특정한 만큼만 뒤로 돌릴 일이 있을까?
    }

    private int _currentNoteIndex = 0; // 현재 채보 인덱스
    public int currentNoteIndex
    {
        get { return _currentNoteIndex; }
        // set { _currentNoteIndex = value; }
    }


    // 스트라이커에 소속된 오브젝트 큐
    private Queue<GameObject> exclamationQueue = new Queue<GameObject>();
    private Queue<GameObject> projectiveQueue = null;  // 존재하지 않을 수 있음


    // 개별 스트라이커의 고유값 관련
    private Direction _location = Direction.None;
    public Direction location
    {
        get { return _location; }
        set { if (_location == Direction.None)
              { _location = value; } }
    }

    private StrikerType _strikerType;
    public StrikerType strikerType
    {
        get { return _strikerType; }
    }

    private Vector3 spawnPosition;

    [SerializeField] private Dictionary<string, float?> _displayPreset = new Dictionary<string, float?>
    {
        { "moveTime", null },
        { "backtime", null },
        { "animeOffset", null },
        { "spacing", null },
        { "moveDuration", null },
        { "spawnOffset", null }
    };
    public Dictionary<string, float?> displayPreset
    {
        get { return _displayPreset; }
    }

    [SerializeField] private Dictionary<string, AudioClip> soundSet = new Dictionary<string, AudioClip?>
    {
        { "NormalprepareSound", null },
        { "StrongprepareSound", null },
        { "NormalparrySound", null },
        { "StrongparrySound", null },
        { "holdingSound", null },
        { "holdingEnd", null }
    };

    private bool _isPause = false;
    public bool isPause
    {
        get { return _isPause; }
        set { _isPause = value; }
    }

    private IAttack attackStrategy = null;


    // 초기값 대입
    private void Start()
    {
        attackStrategy.InitStrategy();
        InitStriker();
    }

    public abstract void InitStriker();

    private void Update()  // 매 프레임마다 실행
    {
        if (!isPause)
        {
            CheckAttackMovement();
        }
    }


    private void CheckAttackMovement()
    {
        float currentTime = StageManager.Instance.currentTime;

        // 발사 준비, 느낌표 표시
        if (_currentPrepareNoteIndex < _chartData.notes.Length &&
            currentTime >= chartData.notes[_currentPrepareNoteIndex].time *
            (60f / StageManager.Instance.bpm) + StageManager.Instance.musicOffset)
        {
            switch ((AttackType)chartData.notes[_currentPrepareNoteIndex].type)
            {
                case AttackType.Normal:
                    attackStrategy.NormalReady(AttackType.Normal);
                    break;

                case AttackType.Strong:
                    attackStrategy.NormalReady(AttackType.Strong);
                    break;
                
                case AttackType.HoldStart:
                    attackStrategy.HoldReady();
                    break;
                
                case AttackType.HoldFinishStrong:
                    attackStrategy.HoldEndReady();
                    break;
                
                default:
                    break;
            }

            _currentPrepareNoteIndex++;
        }

        // 발사, 느낌표 삭제
        if (_currentNoteIndex < _chartData.notes.Length &&
            currentTime >= chartData.notes[_currentNoteIndex].time *
            (60f / StageManager.Instance.bpm) + StageManager.Instance.musicOffset)
        {
            switch ((AttackType)chartData.notes[_currentPrepareNoteIndex].type)
            {
                case AttackType.Normal:
                    attackStrategy.NormalAttack(AttackType.Normal);
                    break;

                case AttackType.Strong:
                    attackStrategy.NormalAttack(AttackType.Strong);
                    break;
                
                case AttackType.HoldStart:
                    attackStrategy.HoldAttack();
                    break;
                
                case AttackType.HoldFinishStrong:
                    attackStrategy.HoldEndAttack();
                    break;
                
                default:
                    break;
            }

            _currentNoteIndex++;
        }
    }

}
