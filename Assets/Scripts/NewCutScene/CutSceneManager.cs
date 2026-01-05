using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private CutSceneData data;
    [SerializeField] private GameObject[] cutscenePanels;

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font prologueFont;

    [SerializeField] private Animator cutsceneAnimator;
    [SerializeField] private AudioSource bgm;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typingDelay = 0.03f;

    private Text dialogText;
    private int currentIndex;
    private bool isTransitioning;
    private bool isTyping;
    private bool isAnimationPlaying = false;

    private AudioSource typingSound;
    private DatabaseManager databaseManager;

    private void Start()
    {
        typingSound = GetComponent<AudioSource>();
        databaseManager = FindObjectOfType<DatabaseManager>();

        CreateTextUI();
        SetAllPanelsActive(false);

        currentIndex = 0;

        if (data != null && data.clickSteps.Count > 0)
        {
            StartCoroutine(ExecuteClickStep(0));
        }
    }

    private void Update()
    {
        if (!IsAdvanceInputDown()) return;

        if (isTransitioning) return;
        if (isAnimationPlaying) return;
        if (isTyping) return;

        currentIndex++;

        if (data == null || currentIndex >= data.clickSteps.Count)
        {
            EndCutScene();
            return;
        }

        StartCoroutine(ExecuteClickStep(currentIndex));
    }

    private IEnumerator ExecuteClickStep(int index)
    {
        CutSceneClickStep step = data.clickSteps[index];

        for (int i = 0; i < step.actions.Count; i++)
        {
            CutSceneAction action = step.actions[i];

            switch (action.actionType)
            {
                case CutSceneActionType.ShowPanel:
                    cutscenePanels[action.panelIndex].gameObject.SetActive(true);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.HidePanel:
                    cutscenePanels[action.panelIndex].gameObject.SetActive(false);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.FadeIn:
                    StartCoroutine(FadePanel(action.panelIndex, true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.FadeOut:
                    StartCoroutine(FadePanel(action.panelIndex, false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.ShowTextReset:
                    StartCoroutine(TypeText(data.textSet[currentIndex], shouldResetText: true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.ShowTextAppend:
                    StartCoroutine(TypeText(data.textSet[currentIndex], shouldResetText: false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.TriggerAnimator:
                    if (cutsceneAnimator != null && !string.IsNullOrEmpty(action.animatorTrigger))
                    {
                        isAnimationPlaying = true;
                        cutsceneAnimator.SetTrigger(action.animatorTrigger);
                    }
                    yield return new WaitForSeconds(action.waitseconds);
                    isAnimationPlaying = false;
                    break;
            }
        }
    }

    private IEnumerator FadePanel(int panelIndex, bool fadeIn)
    {
        if (panelIndex < 0 || panelIndex >= cutscenePanels.Length) yield break;

        GameObject panel = cutscenePanels[panelIndex];
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null)
        {
            panel.SetActive(fadeIn);
            yield break;
        }

        panel.SetActive(true);
        isTransitioning = true;

        Color color = panelImage.color;
        float startAlpha = fadeIn ? 0f : color.a;
        float endAlpha = fadeIn ? 1f : 0f;

        color.a = startAlpha;
        panelImage.color = color;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            panelImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        panelImage.color = color;

        if (!fadeIn) panel.SetActive(false);

        isTransitioning = false;
    }

    private IEnumerator TypeText(string textToType, bool shouldResetText)
    {
        if (shouldResetText)
        {
            dialogText.text = "";
        }

        isTyping = true;

        int characterIndex = 0;
        int typingSoundDelay = 0;

        while (characterIndex < textToType.Length)
        {
            dialogText.text += textToType[characterIndex];
            characterIndex++;

            if (characterIndex < textToType.Length)
            {
                if (typingSoundDelay >= 3 && textToType[characterIndex] != ' ')
                {
                    if (typingSound != null) typingSound.Play();
                    typingSoundDelay = 0;
                }
            }

            typingSoundDelay++;
            yield return new WaitForSeconds(typingDelay);
        }

        if (currentIndex == 1)
        {
            string firstText = (data.textSet != null && data.textSet.Length > 0) ? data.textSet[0] : "";
            string secondText = (data.textSet != null && data.textSet.Length > 1) ? data.textSet[1] : "";
            dialogText.text = firstText + secondText;
        }
        else
        {
            dialogText.text = textToType;
        }

        isTyping = false;
    }

    private void EndCutScene()
    {
        if (data == null) return;

        switch (data.cutsceneType)
        {
            case CutSceneType.Prologue:
                SceneLinkage.StageLV = 0;
                SceneManager.LoadScene("Tutorial");
                break;

            //Todo: 나머지 스테이지 씬전환 로직 추가
        }
    }

    private bool IsAdvanceInputDown()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    private void SetAllPanelsActive(bool active)
    {
        for (int i = 0; i < cutscenePanels.Length; i++)
        {
            cutscenePanels[i]?.SetActive(active);
        }
    }

    private void CreateTextUI()
    {
        GameObject textObject = new GameObject("PrologueText");
        textObject.transform.SetParent(mainCanvas.transform, false);

        dialogText = textObject.AddComponent<Text>();
        dialogText.font = prologueFont;
        dialogText.fontSize = 60;
        dialogText.color = Color.white;
        dialogText.alignment = TextAnchor.UpperCenter;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.9f, 0.3f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        dialogText.text = "";
        textObject.SetActive(true);
    }
}
