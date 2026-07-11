using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaticUIManager : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform canvasRoot;
    [SerializeField] private Transform canvasPause;

    [Header("Prefabs")]
    [SerializeField] private GameObject overlayPrefab;
    [SerializeField] private GameObject clearPanelPrefab;
    [SerializeField] private GameObject gameOverPanelPrefab;
    [SerializeField] private GameObject pausePanelPrefab;

    [Header("Panel Background")]
    [SerializeField] private GameObject darkPanel;

    [Header("Pause")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private Sprite unpauseButtonSprite;
    [SerializeField] private TextMeshProUGUI continueText;

    [Header("Tutorial")]
    [SerializeField] private GameObject skipButton;
    [SerializeField] private float tutorialFadeInDuration = 0.15f;
    [SerializeField] private float tutorialFadeOutDuration = 0.3f;

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Victory")]
    [SerializeField] private GameObject victoryAnimatorObject;
    [SerializeField] private AudioSource victoryAudioSource;

    [Header("Result")]
    [SerializeField] private StageResultManager stageResultManager;

    private GameObject overlayObject;
    private GameObject clearPanel;
    private GameObject gameOverPanel;
    private GameObject pausePanel;
    private GameObject tutorialPanel;

    private Image pauseButtonImage;
    private Sprite originalPauseButtonSprite;

    private Animator victoryAnimator;

    public bool victoryPlayed = false;

    private bool initialized = false;

    private void Start()
    {
        if (initialized) return;
        initialized = true;

        CreatePanels();
        CreateOverlay();

        CachePauseButton();
        CacheVictory();
        CacheDarkPanel();

        if (overlayObject != null) overlayObject.SetActive(false);
        if (darkPanel != null) darkPanel.SetActive(false);
        if (clearPanel != null) clearPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (victoryAnimatorObject != null) victoryAnimatorObject.SetActive(false);
        if (continueText != null) continueText.gameObject.SetActive(false);
    }

    public void CreateOverlay()
    {
        if (overlayObject != null) return;
        if (overlayPrefab == null) return;
        if (canvasPause == null) return;

        overlayObject = Instantiate(overlayPrefab, canvasPause);
        overlayObject.SetActive(false);
    }

    public void ToggleOverlay(bool isOn)
    {
        if (overlayObject != null)
        {
            overlayObject.SetActive(isOn);
        }
    }

    public void CreatePanels()
    {
        if (canvasRoot == null || canvasPause == null)
        {
            Debug.LogError("[StaticUIManager] canvasRoot/canvasPause is not assigned.");
            return;
        }

        if (clearPanel == null && clearPanelPrefab != null)
        {
            clearPanel = Instantiate(clearPanelPrefab, canvasPause);
            BindPanelButtons(clearPanel, false);
            clearPanel.SetActive(false);
        }

        if (gameOverPanel == null && gameOverPanelPrefab != null)
        {
            gameOverPanel = Instantiate(gameOverPanelPrefab, canvasPause);
            BindPanelButtons(gameOverPanel, false);
            gameOverPanel.SetActive(false);
        }

        if (pausePanel == null && pausePanelPrefab != null)
        {
            pausePanel = Instantiate(pausePanelPrefab, canvasPause);
            BindPanelButtons(pausePanel, true);
            pausePanel.SetActive(false);
        }
    }

    private void CacheDarkPanel()
    {
        if (darkPanel != null || canvasPause == null) return;

        Transform[] children = canvasPause.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.gameObject.name == "DarkPanel")
            {
                darkPanel = child.gameObject;
                return;
            }
        }
    }

    private void ToggleDarkPanel(bool isOn)
    {
        CacheDarkPanel();
        if (darkPanel != null)
        {
            darkPanel.SetActive(isOn);
        }
    }

    private void BindPanelButtons(GameObject panel, bool includeContinue)
    {
        if (panel == null) return;

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "ButtonReplay":
                    button.onClick.AddListener(RestartStage);
                    break;

                case "ButtonExit":
                    button.onClick.AddListener(ExitStage);
                    break;

                case "ButtonContinue" when includeContinue:
                    button.onClick.AddListener(ResumeStage);
                    break;
            }
        }
    }

    private void RestartStage()
    {
        StageFlowManager.Instance?.RestartStage();
    }

    private void ExitStage()
    {
        StageFlowManager.Instance?.ExitStage();
    }

    private void ResumeStage()
    {
        StageFlowManager.Instance?.ResumeStage();
    }

    public void Setup_UI()
    {
        if (StageFlowManager.Instance.currentStageData.Category == StageCategory.Tutorial)
        {
            pauseButton.SetActive(false);
            skipButton.SetActive(true);
            ToggleOverlay(false);
            ToggleClearPanel(false);
            ToggleGameOverPanel(false);
            TogglePausePanel(false);
            UpdatePauseButtonSprite(false);
        }
        else
        {
            pauseButton.SetActive(true);
            skipButton.SetActive(false);
            ToggleOverlay(false);
            ToggleClearPanel(false);
            ToggleGameOverPanel(false);
            TogglePausePanel(false);
            UpdatePauseButtonSprite(false);
        }
    }

    public IEnumerator ShowTutorialPanel(GameObject tutorialPanelPrefab)
    {
        if (tutorialPanelPrefab == null) yield break;
        if (canvasRoot == null) yield break;

        tutorialPanel = Instantiate(tutorialPanelPrefab, canvasRoot);

        CanvasGroup canvasGroup = GetOrAddCanvasGroup(tutorialPanel);
        tutorialPanel.SetActive(true);

        yield return StartCoroutine(FadeTutorialPanel(canvasGroup, true));
    }

    public IEnumerator HideTutorialPanel()
    {
        if (tutorialPanel == null) yield break;

        CanvasGroup canvasGroup = GetOrAddCanvasGroup(tutorialPanel);

        yield return StartCoroutine(FadeTutorialPanel(canvasGroup, false));

        if (tutorialPanel != null)
        {
            Destroy(tutorialPanel);
            tutorialPanel = null;
        }
    }

    private IEnumerator FadeTutorialPanel(CanvasGroup canvasGroup, bool fadeIn)
    {
        if (canvasGroup == null)
            yield break;

        float duration = fadeIn ? tutorialFadeInDuration : tutorialFadeOutDuration;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = startAlpha;

        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            if (!fadeIn)
            {
                canvasGroup.gameObject.SetActive(false);
            }
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject targetObject)
    {
        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    public void ToggleClearPanel(bool isOn)
    {
        if (clearPanel == null) return;

        clearPanel.SetActive(isOn);
        ToggleDarkPanel(isOn);

        if (isOn)
        {
            UpdatePanelScores(clearPanel);
            UpdateStarDisplay();
        }
    }

    public void ToggleGameOverPanel(bool isOn)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(isOn);
        ToggleDarkPanel(isOn);

        if (isOn)
        {
            UpdatePanelScores(gameOverPanel);
        }
    }

    public void TogglePausePanel(bool isOn)
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(isOn);
        ToggleDarkPanel(isOn);

        if (isOn)
        {
            UpdatePanelScores(pausePanel);
            if (continueText != null) continueText.gameObject.SetActive(true);
        }
        else
        {
            if (continueText != null) continueText.gameObject.SetActive(false);
        }
    }

    public void UpdatePauseButtonSprite(bool isPaused)
    {
        if (pauseButtonImage == null) return;

        if (isPaused)
        {
            if (unpauseButtonSprite != null)
            {
                pauseButtonImage.sprite = unpauseButtonSprite;
            }
        }
        else
        {
            if (originalPauseButtonSprite != null)
            {
                pauseButtonImage.sprite = originalPauseButtonSprite;
            }
        }
    }

    public void UpdatePanelScores(GameObject panelObject)
    {
        if (panelObject == null) return;
        if (stageResultManager == null) return;

        int score = stageResultManager.LatestScore;
        List<int[]> judgeDetails = stageResultManager.LatestJudgeDetails;

        TextMeshProUGUI FindText(string exactName)
        {
            TextMeshProUGUI[] all = panelObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].gameObject.name == exactName)
                {
                    return all[i];
                }
            }
            return null;
        }

        TextMeshProUGUI scoreText = FindText("ScoreText");
        if (scoreText != null) scoreText.text = score.ToString();

        if (judgeDetails == null || judgeDetails.Count == 0 || judgeDetails[0] == null) return;

        int[] d = judgeDetails[0];
        if (d.Length < 8) return;

        TextMeshProUGUI perfectText = FindText("ParfectText");
        if (perfectText == null) perfectText = FindText("PerfectText");
        if (perfectText != null) perfectText.text = d[4].ToString();

        TextMeshProUGUI bounceText = FindText("BounceText");
        if (bounceText != null) bounceText.text = (d[3] + d[5]).ToString();

        TextMeshProUGUI guardText = FindText("GuardText");
        if (guardText != null) guardText.text = (d[2] + d[6]).ToString();

        TextMeshProUGUI hitText = FindText("HitText");
        if (hitText != null) hitText.text = (d[1] + d[7]).ToString();
    }

    public IEnumerator ResumeCountDown()
    {
        if (countdownText == null) yield break;

        if (continueText != null) continueText.gameObject.SetActive(false);

        countdownText.gameObject.SetActive(true);

        for (int count = 3; count >= 1; count--)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.gameObject.SetActive(false);
    }

    public void ResetVictoryAnimation()
    {
        victoryPlayed = false;
        if (victoryAnimatorObject != null) victoryAnimatorObject.SetActive(false);
    }

    public void StartVictoryAnimation()
    {
        if (victoryAnimatorObject == null || victoryAnimator == null) return;
        if (victoryPlayed) return;

        victoryPlayed = true;
        victoryAnimatorObject.SetActive(true);

        if (victoryAudioSource != null) victoryAudioSource.Play();

        victoryAnimator.SetTrigger("Play");
    }

    public bool VictoryAnimationFinished()
    {
        if (!victoryPlayed) return false;
        if (victoryAnimatorObject == null || victoryAnimator == null) return true;

        if (victoryAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            victoryAnimatorObject.SetActive(false);
            return true;
        }

        return false;
    }

    private void CachePauseButton()
    {
        if (pauseButton == null) return;

        pauseButtonImage = pauseButton.GetComponent<Image>();
        if (pauseButtonImage != null)
        {
            originalPauseButtonSprite = pauseButtonImage.sprite;
        }
    }

    private void CacheVictory()
    {
        if (victoryAnimatorObject == null) return;
        victoryAnimator = victoryAnimatorObject.GetComponent<Animator>();
    }

    private void UpdateStarDisplay()
    {
        if (clearPanel == null) return;
        if (stageResultManager == null) return;

        int stars = stageResultManager.LatestStarCount;

        Transform star1 = clearPanel.transform.Find("Star1");
        Transform star2 = clearPanel.transform.Find("Star2");
        Transform star3 = clearPanel.transform.Find("Star3");

        if (star1 != null) star1.gameObject.SetActive(stars >= 1);
        if (star2 != null) star2.gameObject.SetActive(stars >= 2);
        if (star3 != null) star3.gameObject.SetActive(stars >= 3);
    }
}
