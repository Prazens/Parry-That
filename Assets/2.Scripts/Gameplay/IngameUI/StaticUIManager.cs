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

    [Header("Pause")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private Sprite unpauseButtonSprite;
    [SerializeField] private TextMeshProUGUI continueText;

    [Header("Tutorial")]
    [SerializeField] private GameObject skipButton;

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

    private Image pauseButtonImage;
    private Sprite originalPauseButtonSprite;

    private Animator victoryAnimator;

    public bool victoryPlayed = false;

    private bool initialized = false;

    private void Start()
    {
        if (initialized) return;
        initialized = true;

        if (canvasRoot == null || canvasPause == null)
        {
            Debug.LogError("[StaticUIManager] canvasRoot/canvasPause is not assigned.");
            return;
        }

        if (overlayObject == null && overlayPrefab != null)
        {
            overlayObject = Instantiate(overlayPrefab, canvasPause);
            overlayObject.SetActive(false);
        }

        if (clearPanel == null && clearPanelPrefab != null)
        {
            clearPanel = Instantiate(clearPanelPrefab, canvasRoot);
            clearPanel.SetActive(false);
        }

        if (gameOverPanel == null && gameOverPanelPrefab != null)
        {
            gameOverPanel = Instantiate(gameOverPanelPrefab, canvasRoot);
            gameOverPanel.SetActive(false);
        }

        if (pausePanel == null && pausePanelPrefab != null)
        {
            pausePanel = Instantiate(pausePanelPrefab, canvasPause);
            pausePanel.SetActive(false);
        }

        CachePauseButton();
        CacheVictory();

        // Start에서 ToggleXXX를 여러 번 호출하지 말고 직접 끔(토글 내부 로직/텍스트 갱신 방지)
        if (overlayObject != null) overlayObject.SetActive(false);
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
            clearPanel = Instantiate(clearPanelPrefab, canvasRoot);
            clearPanel.SetActive(false);
        }

        if (gameOverPanel == null && gameOverPanelPrefab != null)
        {
            gameOverPanel = Instantiate(gameOverPanelPrefab, canvasRoot);
            gameOverPanel.SetActive(false);
        }

        if (pausePanel == null && pausePanelPrefab != null)
        {
            pausePanel = Instantiate(pausePanelPrefab, canvasPause);
            pausePanel.SetActive(false);
        }
    }

    public void Setup_UI()
    {
        if (TutorialManager.isTutorial)
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

    public void ToggleClearPanel(bool isOn)
    {
        if (clearPanel == null) return;

        clearPanel.SetActive(isOn);

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

        if (isOn)
        {
            UpdatePanelScores(gameOverPanel);
        }
    }

    public void TogglePausePanel(bool isOn)
    {
        if (pausePanel == null) return;

        pausePanel.SetActive(isOn);

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
        if (perfectText != null) perfectText.text = d[4].ToString("D4");

        TextMeshProUGUI bounceText = FindText("BounceText");
        if (bounceText != null) bounceText.text = (d[3] + d[5]).ToString("D4");

        TextMeshProUGUI guardText = FindText("GuardText");
        if (guardText != null) guardText.text = (d[2] + d[6]).ToString("D4");

        TextMeshProUGUI hitText = FindText("HitText");
        if (hitText != null) hitText.text = (d[1] + d[7]).ToString("D4");
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
        if (victoryPlayed) return; // 중복 방지 (선택)

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

        // ClearPanel 인스턴스에서 직접 Star 오브젝트 찾기
        Transform star1 = clearPanel.transform.Find("Star1");
        Transform star2 = clearPanel.transform.Find("Star2");
        Transform star3 = clearPanel.transform.Find("Star3");

        if (star1 != null) star1.gameObject.SetActive(stars >= 1);
        if (star2 != null) star2.gameObject.SetActive(stars >= 2);
        if (star3 != null) star3.gameObject.SetActive(stars >= 3);
    }
}
