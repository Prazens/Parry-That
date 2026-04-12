using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// summary: CutSceneData SO로부터 받은 데이터를 바탕으로 컷씬을 진행해주는 코드
public class CutSceneManager : MonoBehaviour
{
    [Header("Optional Direct Override")]
    [SerializeField] private CutSceneData data;

    [Header("References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform cutScenesRoot;
    [SerializeField] private Font prologueFont;
    [SerializeField] private AudioSource bgm;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float typingDelay = 0.03f;
    [SerializeField] private float stepEndDelay = 0.3f;

    private float fadeDuration;

    private Text cutSceneText;
    private int currentIndex;

    private bool isTyping;
    private bool isStepRunning;

    private AudioSource typingSound;
    private CutSceneLevelManager cutSceneLevelManager;

    private readonly List<GameObject> spawnedPanels = new List<GameObject>();

    private void Start()
    {
        typingSound = GetComponent<AudioSource>();
        cutSceneLevelManager = FindObjectOfType<CutSceneLevelManager>();

        ResolveCutSceneData();

        if (data == null)
        {
            EndCutScene();
            return;
        }

        CreateTextUI();
        CreatePanelsFromSO();
        PlayBGM();

        currentIndex = 0;

        if (data.clickSteps != null && data.clickSteps.Count > 0)
        {
            StartCoroutine(ExecuteClickStep(0));
        }
    }

    private void Update()
    {
        if (!IsClickInputDown())
        {
            return;
        }

        if (isStepRunning || isTyping)
        {
            return;
        }

        currentIndex++;

        if (data == null || data.clickSteps == null || currentIndex >= data.clickSteps.Count)
        {
            EndCutScene();
            return;
        }

        StartCoroutine(ExecuteClickStep(currentIndex));
    }

    private void ResolveCutSceneData()
    {
        if (data != null)
        {
            return;
        }

        if (!CutSceneSelection.HasValidSelection())
        {
            data = null;
            return;
        }

        if (cutSceneLevelManager == null)
        {
            Debug.LogError("CutSceneManager: CutSceneLevelManager를 찾지 못함");
            data = null;
            return;
        }

        data = cutSceneLevelManager.GetCutSceneData(
            CutSceneSelection.SelectedStageId,
            CutSceneSelection.SelectedCategory
        );
    }

    private bool IsClickInputDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        return false;
    }

    private IEnumerator ExecuteClickStep(int index)
    {
        if (data == null || data.clickSteps == null || index < 0 || index >= data.clickSteps.Count)
        {
            yield break;
        }

        isStepRunning = true;

        CutSceneClickStep step = data.clickSteps[index];
        if (step == null || step.actions == null)
        {
            isStepRunning = false;
            yield break;
        }

        int actionIndex = 0;

        while (actionIndex < step.actions.Count)
        {
            CutSceneAction action = step.actions[actionIndex];
            if (action == null)
            {
                actionIndex++;
                continue;
            }

            // 연속된 FadeIn/FadeOut 액션을 동시에 실행
            if (action.actionType == CutSceneActionType.FadeIn)
            {
                while (actionIndex < step.actions.Count)
                {
                    CutSceneAction fadeAction = step.actions[actionIndex];
                    if (fadeAction == null)
                    {
                        actionIndex++;
                        continue;
                    }

                    if (fadeAction.actionType != CutSceneActionType.FadeIn)
                    {
                        break;
                    }

                    StartCoroutine(FadePanel(fadeAction.index, true));
                    actionIndex++;
                }

                yield return new WaitForSeconds(fadeDuration);
                yield return new WaitForSeconds(action.waitseconds);
                continue;
            }

            if (action.actionType == CutSceneActionType.FadeOut)
            {
                while (actionIndex < step.actions.Count)
                {
                    CutSceneAction fadeAction = step.actions[actionIndex];
                    if (fadeAction == null)
                    {
                        actionIndex++;
                        continue;
                    }

                    if (fadeAction.actionType != CutSceneActionType.FadeOut)
                    {
                        break;
                    }

                    StartCoroutine(FadePanel(fadeAction.index, false));
                    actionIndex++;
                }

                yield return new WaitForSeconds(fadeDuration);
                yield return new WaitForSeconds(action.waitseconds);
                continue;
            }

            switch (action.actionType)
            {
                case CutSceneActionType.ShowPanel:
                    SetPanelActive(action.index, true);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.HidePanel:
                    SetPanelActive(action.index, false);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.ShowTextReset:
                    yield return StartCoroutine(ShowTextByIndex(action.index, true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.ShowTextAppend:
                    yield return StartCoroutine(ShowTextByIndex(action.index, false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                case CutSceneActionType.TriggerAnimator:
                    TriggerAnimator(action.index, action.animatorTrigger);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;
            }

            actionIndex++;
        }

        yield return new WaitForSeconds(stepEndDelay);
        isStepRunning = false;
    }

    private IEnumerator ShowTextByIndex(int textIndex, bool reset)
    {
        if (data == null || data.textSet == null)
        {
            yield break;
        }

        if (textIndex < 0 || textIndex >= data.textSet.Length)
        {
            Debug.LogWarning($"CutSceneManager: textSet 인덱스 범위 초과 ({textIndex})");
            yield break;
        }

        yield return StartCoroutine(TypeText(data.textSet[textIndex], reset));
    }

    private void CreatePanelsFromSO()
    {
        if (data == null || data.panels == null)
        {
            return;
        }

        if (cutScenesRoot == null)
        {
            Debug.LogError("CutSceneManager: cutScenesRoot가 비어있음");
            return;
        }

        for (int panelIndex = 0; panelIndex < data.panels.Count; panelIndex++)
        {
            CutScenePanels panel = data.panels[panelIndex];
            if (panel == null)
            {
                spawnedPanels.Add(null);
                continue;
            }

            GameObject panelObject = CreatePanelObject(panelIndex, panel);
            spawnedPanels.Add(panelObject);
        }
    }

    private GameObject CreatePanelObject(int panelIndex, CutScenePanels panel)
    {
        GameObject panelObject;

        if (panel.type == CutScenePanelType.Animator)
        {
            if (panel.animatorcontroller == null)
            {
                Debug.LogWarning($"CutSceneManager: panels[{panelIndex}] AnimatorController가 null");
                return null;
            }

            panelObject = new GameObject($"CutsceneAnimator_{panelIndex}");
            panelObject.transform.SetParent(cutScenesRoot, false);

            RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
            ApplyPanelTransform(rectTransform, panel);

            Image image = panelObject.AddComponent<Image>();
            image.sprite = panel.sprite;

            Animator animator = panelObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = panel.animatorcontroller;
        }
        else
        {
            panelObject = new GameObject($"CutsceneSprite_{panelIndex}");
            panelObject.transform.SetParent(cutScenesRoot, false);

            RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
            ApplyPanelTransform(rectTransform, panel);

            Image image = panelObject.AddComponent<Image>();
            image.sprite = panel.sprite;
        }

        panelObject.SetActive(false);
        return panelObject;
    }

    private void ApplyPanelTransform(RectTransform rectTransform, CutScenePanels panel)
    {
        rectTransform.anchorMin = panel.anchorMin;
        rectTransform.anchorMax = panel.anchorMax;
        rectTransform.pivot = panel.pivot;
        rectTransform.anchoredPosition = panel.anchoredPosition;
        rectTransform.sizeDelta = panel.size;
        rectTransform.localScale = panel.scale;
    }

    private void PlayBGM()
    {
        if (data == null || data.bgm == null || bgm == null)
        {
            return;
        }

        bgm.clip = data.bgm;
        bgm.loop = data.loopBgm;
        bgm.volume = data.bgmVolume;
        bgm.Play();
    }

    private void SetPanelActive(int index, bool active)
    {
        if (!IsValidPanel(index))
        {
            return;
        }

        GameObject panelObject = spawnedPanels[index];
        if (panelObject == null)
        {
            return;
        }

        panelObject.SetActive(active);
    }

    private IEnumerator FadePanel(int index, bool fadeIn)
    {
        if (!IsValidPanel(index))
        {
            yield break;
        }

        GameObject panelObject = spawnedPanels[index];
        if (panelObject == null)
        {
            yield break;
        }

        Image image = panelObject.GetComponent<Image>();
        if (image == null)
        {
            panelObject.SetActive(fadeIn);
            yield break;
        }

        panelObject.SetActive(true);

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        fadeDuration = fadeIn ? fadeInDuration : fadeOutDuration;

        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;

        if (!fadeIn)
        {
            panelObject.SetActive(false);
        }
    }

    private void TriggerAnimator(int index, string trigger)
    {
        if (!IsValidPanel(index))
        {
            return;
        }

        GameObject panelObject = spawnedPanels[index];
        if (panelObject == null)
        {
            return;
        }

        Animator animator = panelObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"CutSceneManager: panels[{index}] Animator가 없음");
            return;
        }

        if (string.IsNullOrEmpty(trigger))
        {
            Debug.LogWarning($"CutSceneManager: panels[{index}] trigger가 비어있음");
            return;
        }

        animator.SetTrigger(trigger);
    }

    private bool IsValidPanel(int index)
    {
        return index >= 0 && index < spawnedPanels.Count;
    }

    private IEnumerator TypeText(string text, bool reset)
    {
        isTyping = true;

        if (cutSceneText == null)
        {
            isTyping = false;
            yield break;
        }

        if (reset)
        {
            cutSceneText.text = string.Empty;
        }

        if (string.IsNullOrEmpty(text))
        {
            isTyping = false;
            yield break;
        }

        int typingSoundDelayCounter = 0;

        for (int characterIndex = 0; characterIndex < text.Length; characterIndex++)
        {
            cutSceneText.text += text[characterIndex];

            if (typingSound != null)
            {
                if (typingSoundDelayCounter >= 3 && text[characterIndex] != ' ')
                {
                    typingSound.Play();
                    typingSoundDelayCounter = 0;
                }
            }

            typingSoundDelayCounter++;
            yield return new WaitForSeconds(typingDelay);
        }

        isTyping = false;
    }

    public void EndCutScene()
    {
        if (bgm != null && bgm.isPlaying)
        {
            bgm.Stop();
        }

        //에필로그면 로비로
        if (CutSceneSelection.SelectedCategory == CutSceneCategory.Epilogue)
        {
            SceneManager.LoadScene("testMain");
        }
        //프롤로그면 스테이지로
        else
        {
            SceneManager.LoadScene("Stage");
        }
    }

    private void CreateTextUI()
    {
        if (mainCanvas == null)
        {
            Debug.LogError("CutSceneManager: mainCanvas가 비어있음");
            return;
        }

        GameObject textObject = new GameObject("CutSceneText");
        textObject.transform.SetParent(mainCanvas.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.3f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        cutSceneText = textObject.AddComponent<Text>();
        cutSceneText.font = prologueFont;
        cutSceneText.fontSize = 60;
        cutSceneText.color = Color.white;
        cutSceneText.alignment = TextAnchor.UpperCenter;
        cutSceneText.text = string.Empty;
    }
}