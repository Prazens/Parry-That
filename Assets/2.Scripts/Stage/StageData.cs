using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageCategory
{
    Tutorial = 0,
    Normal = 1,
    Boss = 2,
}

public enum DialogueAudioPolicy
{
    KeepPlaying = 0,      // 음악 계속 재생, currentTime도 그대로
    PauseAndResume = 1,   // 음악 일시정지, 대사 끝나면 재개
    ResetAndReplay = 2,   // 음악 정지 후 처음부터 다시, currentTime도 리셋
}

[Serializable]
public class BossSpawnEntry
{
    [Tooltip("Spawn할 프리팹")]
    public GameObject prefab;

    [Tooltip("bossSpawnRoot 아래에서 특정 자식 Transform 이름으로 붙이고 싶으면 입력(없으면 bossSpawnRoot 그대로 사용)")]
    public string attachPointName;

    [Tooltip("부모 기준 로컬 위치 오프셋")]
    public Vector3 localPosition = Vector3.zero;

    [Tooltip("부모 기준 로컬 회전(오일러)")]
    public Vector3 localEulerAngles = Vector3.zero;

    [Tooltip("부모 기준 로컬 스케일")]
    public Vector3 localScale = Vector3.one;
}

[Serializable]
public class StagePhase
{
    [Header("Phase Chart")]
    [Tooltip("이 페이즈에서 재생할 채보. 없으면 채보 없이 진행 가능")]
    [SerializeField] private TextAsset chartJson;

    [Header("Dialogue After Chart")]
    [Tooltip("채보 종료 후 출력할 대사. 채보가 없어도 단독 출력 가능")]
    [SerializeField] private DialogueData dialogue;

    [Header("Dialogue Audio Policy")]
    [Tooltip("대사 시작 시 음악/시간 처리 방식")]
    [SerializeField] private DialogueAudioPolicy dialogueAudioPolicy = DialogueAudioPolicy.KeepPlaying;

    public TextAsset ChartJson => chartJson;
    public DialogueData Dialogue => dialogue;
    public DialogueAudioPolicy DialogueAudioPolicy => dialogueAudioPolicy;

    public bool HasChart => chartJson != null;
    public bool HasDialogue => dialogue != null;
    public bool IsEmpty => chartJson == null && dialogue == null;
}

[CreateAssetMenu(menuName = "Stage/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private int stageId = 0;
    [SerializeField] private Difficulty difficulty = Difficulty.Easy;
    [SerializeField] private StageCategory category = StageCategory.Normal;

    [Header("Phases")]
    [SerializeField] private List<StagePhase> phases = new List<StagePhase>();

    [Header("Audio")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private bool overrideBgmOffset = false;
    [SerializeField] private float bgmOffset = 0f;

    [Header("CutIn UI")]
    [SerializeField] private GameObject cutInUpPrefab;
    [SerializeField] private GameObject cutInDownPrefab;

    [Header("Boss Spawns (Boss stage only)")]
    [SerializeField] private List<BossSpawnEntry> bossSpawnPrefabs = new List<BossSpawnEntry>();

    public string SaveKey => $"{stageId}_{difficulty}";

    public int StageId => stageId;
    public Difficulty Difficulty => difficulty;
    public StageCategory Category => category;

    public IReadOnlyList<StagePhase> Phases => phases;

    public AudioClip Bgm => bgm;
    public bool OverrideBgmOffset => overrideBgmOffset;
    public float BgmOffset => bgmOffset;

    public GameObject CutInUpPrefab => cutInUpPrefab;
    public GameObject CutInDownPrefab => cutInDownPrefab;

    public IReadOnlyList<BossSpawnEntry> BossSpawnPrefabs => bossSpawnPrefabs;
}