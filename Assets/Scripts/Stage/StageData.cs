using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageCategory
{
    Tutorial = 0,
    Normal = 1,
    Boss = 2,
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

[CreateAssetMenu(menuName = "Stage/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private int stageId = 0;
    [SerializeField] private Difficulty difficulty = Difficulty.Normal;
    [SerializeField] private StageCategory category = StageCategory.Normal;

    [Header("Display")]
    [SerializeField] private string displayName = "Stage";

    [Header("Charts (JSON)")]
    [SerializeField] private List<TextAsset> chartJsons = new List<TextAsset>();

    [Header("Audio")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private bool overrideMusicOffset = false;
    [SerializeField] private float musicOffset = 0f;

    [Header("CutIn UI")]
    [SerializeField] private GameObject cutInUpPrefab;
    [SerializeField] private GameObject cutInDownPrefab;

    public GameObject CutInUpPrefab => cutInUpPrefab;
    public GameObject CutInDownPrefab => cutInDownPrefab;


    [Header("Boss Spawns (Boss stage only)")]
    [SerializeField] private List<BossSpawnEntry> bossSpawnPrefabs = new List<BossSpawnEntry>();

    public string SaveKey => $"{stageId}_{difficulty}";

    public int StageId => stageId;
    public Difficulty Difficulty => difficulty;
    public StageCategory Category => category;
    public string DisplayName => displayName;

    public IReadOnlyList<TextAsset> ChartJsons => chartJsons;

    public AudioClip Bgm => bgm;
    public bool OverrideMusicOffset => overrideMusicOffset;
    public float MusicOffset => musicOffset;

    public IReadOnlyList<BossSpawnEntry> BossSpawnPrefabs => bossSpawnPrefabs;
}
