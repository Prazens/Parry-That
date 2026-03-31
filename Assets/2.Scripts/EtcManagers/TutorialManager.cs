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

    [SerializeField] private GameObject VictoryAnime;

    private DatabaseManager databaseManager;
    private PlayerManager playerManager;

    public static bool isTutorial = false;

    private SpriteRenderer spriteRenderer;
    private Image animeSpriteImg;
    private Animator animator;

    private Text gameDescriptionText;

    private void Awake()
    {
        databaseManager = FindObjectOfType<DatabaseManager>();
        isTutorial = true;

        spriteRenderer = VictoryAnime.GetComponent<SpriteRenderer>();
        animeSpriteImg = VictoryAnime.GetComponent<Image>();
        animator = VictoryAnime.GetComponent<Animator>();

        VictoryAnime.SetActive(false);
    }

    private void Start()
    {
        GameObject gameDescription = new GameObject("GameDescription");
        gameDescription.transform.SetParent(mainCanvas.transform, false);

        gameDescriptionText = gameDescription.AddComponent<Text>();
        gameDescriptionText.font = DescriptionFont;
        gameDescriptionText.fontSize = 65;
        gameDescriptionText.color = Color.white;
        gameDescriptionText.alignment = TextAnchor.MiddleCenter;

        RectTransform gameDescriptionRect = gameDescription.GetComponent<RectTransform>();
        gameDescriptionRect.anchorMin = new Vector2(0.05f, 0.25f);
        gameDescriptionRect.anchorMax = new Vector2(0.95f, 0.35f);
        gameDescriptionRect.offsetMin = Vector2.zero;
        gameDescriptionRect.offsetMax = Vector2.zero;

        gameDescriptionText.text = "";

        gameDescription.SetActive(true);

        playerManager = GameObject.Find("Player(Clone)").GetComponent<PlayerManager>();

        StageFlowManager.isActive = false;

        if (stageAudioManager != null)
        {
            stageAudioManager.AudioPause();
        }
    }

    private void Update()
    {
        if (animeSpriteImg != null && spriteRenderer != null)
        {
            animeSpriteImg.sprite = spriteRenderer.sprite;
        }
    }

    public void SetDescriptionText(string text)
    {
        if (gameDescriptionText != null)
        {
            gameDescriptionText.text = text;
        }
    }

    public void SkipOn()
    {
        DatabaseManager.isTutorialDone = true;
        isTutorial = false;

        databaseManager.SaveTutorialDone();

        SceneManager.LoadScene("Main");
    }
}