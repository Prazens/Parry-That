using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("기본 UI참조")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font DescriptionFont;
    [SerializeField] private StageFlowManager stageFlowManager;
    [SerializeField] private StageAudioManager stageAudioManager;
    [SerializeField] private JudgeSystem judgeSystem;

    [SerializeField] private GameObject VictoryAnime;

    private DatabaseManager databaseManager;
    private PlayerManager playerManager;
    private bool restartPhase = false;

    private SpriteRenderer spriteRenderer;
    private Image animeSpriteImg;
    private Animator animator;

    private void Awake()
    {
        databaseManager = FindObjectOfType<DatabaseManager>();

        spriteRenderer = VictoryAnime.GetComponent<SpriteRenderer>();
        animeSpriteImg = VictoryAnime.GetComponent<Image>();
        animator = VictoryAnime.GetComponent<Animator>();

        VictoryAnime.SetActive(false);
    }

    private void Start()
    {
        playerManager = GameObject.Find("Player(Clone)").GetComponent<PlayerManager>();

        if (stageAudioManager != null)
        {
            stageAudioManager.AudioPause();
        }

        if (judgeSystem != null)
        {
            judgeSystem.Judged += OnJudged;
        }
    }

    private void OnDestroy()
    {
        if (judgeSystem != null)
        {
            judgeSystem.Judged -= OnJudged;
        }
    }

    private void Update()
    {
        if (animeSpriteImg != null && spriteRenderer != null)
        {
            animeSpriteImg.sprite = spriteRenderer.sprite;
        }
    }

    private void OnJudged(JudgeContext context)
    {
        if (context.judgeType == JudgeType.LateMiss || context.judgeType == JudgeType.EarlyMiss)
        {
            restartPhase = true;
        }
    }

    public void OnPhaseStarted()
    {
        restartPhase = false;
    }

    public bool ShouldRestartCurrentPhase()
    {
        return restartPhase;
    }

    public void SkipOn()
    {
        DatabaseManager.isTutorialDone = true;

        if (databaseManager != null)
        {
            databaseManager.SaveTutorialDone();
        }

        SceneManager.LoadScene("testMain");
    }
}