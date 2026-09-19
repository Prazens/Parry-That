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

    [Header("Input Safety")]
    [Tooltip("컷씬 씬 최초 진입 직후 입력을 무시하는 시간")]
    [SerializeField] private float initialInputLockDuration = 0.3f;

    [Tooltip("새로운 Step이 시작된 직후 입력을 무시하는 시간")]
    [SerializeField] private float stepInputLockDuration = 0.2f;

    [Tooltip("타이핑 스킵 직후 연속 입력을 방지하는 시간")]
    [SerializeField] private float textSkipInputLockDuration = 0.15f;

    private float fadeDuration;

    private Text cutSceneText;

    private int currentIndex;

    // 현재 텍스트가 한 글자씩 출력 중인지
    private bool isTyping;

    // 현재 CutSceneClickStep의 액션들이 실행 중인지
    private bool isStepRunning;

    // 타이핑 스킵 요청
    private bool skipTypingRequested;

    // 현재 타이핑이 완료됐을 때 보여야 할 전체 문자열
    private string typingTargetText = string.Empty;

    // 이 시간 이전의 클릭/터치는 무시
    private float inputLockedUntil;

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

        // 이전 씬에서 버튼을 누른 입력이
        // 컷씬 첫 문장을 바로 스킵하는 것을 방지
        LockInput(initialInputLockDuration);

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

        // 입력 잠금 시간 중이면 클릭 무시
        if (IsInputLocked())
        {
            return;
        }

        // ==========================================================
        // 1. 텍스트가 출력 중이면:
        //    다음 컷으로 넘어가지 않고 현재 텍스트만 즉시 완성
        // ==========================================================
        if (isTyping)
        {
            SkipCurrentTyping();
            return;
        }

        // ==========================================================
        // 2. Fade / Animator / Wait 등
        //    현재 Step 내부 액션이 아직 실행 중이면 클릭 무시
        // ==========================================================
        if (isStepRunning)
        {
            return;
        }

        // ==========================================================
        // 3. Step이 완전히 끝난 상태에서 클릭했을 때만
        //    다음 Step으로 진행
        // ==========================================================
        currentIndex++;

        if (data == null ||
            data.clickSteps == null ||
            currentIndex >= data.clickSteps.Count)
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
        // PC
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        // 모바일
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        return false;
    }

    // ==========================================================
    // Input Safety
    // ==========================================================

    private void LockInput(float duration)
    {
        inputLockedUntil = Mathf.Max(
            inputLockedUntil,
            Time.unscaledTime + duration
        );
    }

    private bool IsInputLocked()
    {
        return Time.unscaledTime < inputLockedUntil;
    }

    private void SkipCurrentTyping()
    {
        if (!isTyping)
        {
            return;
        }

        skipTypingRequested = true;

        // 사용자가 누른 순간 바로 전문 표시
        if (cutSceneText != null)
        {
            cutSceneText.text = typingTargetText;
        }

        // 타자 효과음이 재생 중이라면 정지
        if (typingSound != null)
        {
            typingSound.Stop();
        }

        // 연속 클릭 / 더블 클릭 / 터치가
        // 다음 동작까지 이어지는 것을 막음
        LockInput(textSkipInputLockDuration);
    }

    // ==========================================================
    // CutScene Step
    // ==========================================================

    private IEnumerator ExecuteClickStep(int index)
    {
        if (data == null ||
            data.clickSteps == null ||
            index < 0 ||
            index >= data.clickSteps.Count)
        {
            yield break;
        }

        isStepRunning = true;

        // 이전 Step을 넘기기 위해 눌렀던 입력이
        // 새 Step의 텍스트 스킵에 영향을 주지 않도록 잠금
        LockInput(stepInputLockDuration);

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

            // ======================================================
            // 연속된 FadeIn 액션은 동시에 실행
            // ======================================================
            if (action.actionType == CutSceneActionType.FadeIn)
            {
                while (actionIndex < step.actions.Count)
                {
                    CutSceneAction fadeAction =
                        step.actions[actionIndex];

                    if (fadeAction == null)
                    {
                        actionIndex++;
                        continue;
                    }

                    if (fadeAction.actionType !=
                        CutSceneActionType.FadeIn)
                    {
                        break;
                    }

                    StartCoroutine(
                        FadePanel(fadeAction.index, true)
                    );

                    actionIndex++;
                }

                yield return new WaitForSeconds(fadeInDuration);
                yield return new WaitForSeconds(action.waitseconds);

                continue;
            }

            // ======================================================
            // 연속된 FadeOut 액션은 동시에 실행
            // ======================================================
            if (action.actionType == CutSceneActionType.FadeOut)
            {
                while (actionIndex < step.actions.Count)
                {
                    CutSceneAction fadeAction =
                        step.actions[actionIndex];

                    if (fadeAction == null)
                    {
                        actionIndex++;
                        continue;
                    }

                    if (fadeAction.actionType !=
                        CutSceneActionType.FadeOut)
                    {
                        break;
                    }

                    StartCoroutine(
                        FadePanel(fadeAction.index, false)
                    );

                    actionIndex++;
                }

                yield return new WaitForSeconds(fadeOutDuration);
                yield return new WaitForSeconds(action.waitseconds);

                continue;
            }

            // ======================================================
            // 일반 액션
            // ======================================================
            switch (action.actionType)
            {
                case CutSceneActionType.ShowPanel:
                    SetPanelActive(action.index, true);

                    yield return new WaitForSeconds(
                        action.waitseconds
                    );
                    break;

                case CutSceneActionType.HidePanel:
                    SetPanelActive(action.index, false);

                    yield return new WaitForSeconds(
                        action.waitseconds
                    );
                    break;

                case CutSceneActionType.ShowTextReset:
                    yield return StartCoroutine(
                        ShowTextByIndex(
                            action.index,
                            true
                        )
                    );

                    yield return new WaitForSeconds(
                        action.waitseconds
                    );
                    break;

                case CutSceneActionType.ShowTextAppend:
                    yield return StartCoroutine(
                        ShowTextByIndex(
                            action.index,
                            false
                        )
                    );

                    yield return new WaitForSeconds(
                        action.waitseconds
                    );
                    break;

                case CutSceneActionType.TriggerAnimator:
                    TriggerAnimator(
                        action.index,
                        action.animatorTrigger
                    );

                    yield return new WaitForSeconds(
                        action.waitseconds
                    );
                    break;
            }

            actionIndex++;
        }

        // Step 종료 직후 너무 빠르게 넘어가지 않도록
        // 기존 지연 유지
        yield return new WaitForSeconds(stepEndDelay);

        isStepRunning = false;
    }

    // ==========================================================
    // Text
    // ==========================================================

    private IEnumerator ShowTextByIndex(
        int textIndex,
        bool reset
    )
    {
        if (data == null || data.textSet == null)
        {
            yield break;
        }

        if (textIndex < 0 ||
            textIndex >= data.textSet.Length)
        {
            Debug.LogWarning(
                $"CutSceneManager: textSet 인덱스 범위 초과 ({textIndex})"
            );

            yield break;
        }

        yield return StartCoroutine(
            TypeText(
                data.textSet[textIndex],
                reset
            )
        );
    }

    private IEnumerator TypeText(
        string text,
        bool reset
    )
    {
        if (cutSceneText == null)
        {
            yield break;
        }

        isTyping = true;
        skipTypingRequested = false;

        // Reset이면 기존 문장 삭제.
        // Append이면 현재 출력된 문장 뒤에 이어 붙임.
        string baseText = reset
            ? string.Empty
            : cutSceneText.text;

        if (string.IsNullOrEmpty(text))
        {
            cutSceneText.text = baseText;

            isTyping = false;
            skipTypingRequested = false;

            yield break;
        }

        // 현재 타이핑이 정상적으로 끝났을 때
        // 최종적으로 보여야 할 전문
        typingTargetText = baseText + text;

        int typingSoundDelayCounter = 0;

        if (typingSound != null)
        {
            typingSound.volume =
                PlayerPrefs.GetFloat(
                    "masterVolume",
                    1f
                );
        }

        for (
            int characterIndex = 0;
            characterIndex < text.Length;
            characterIndex++
        )
        {
            // 클릭으로 전문 출력 요청이 들어온 경우
            if (skipTypingRequested)
            {
                cutSceneText.text = typingTargetText;
                break;
            }

            // += 방식 대신 항상 현재 위치까지 다시 구성.
            // 스킵 발생 시 중복 문자가 생기는 것을 방지.
            cutSceneText.text =
                baseText +
                text.Substring(
                    0,
                    characterIndex + 1
                );

            // 타자음
            if (typingSound != null)
            {
                if (
                    typingSoundDelayCounter >= 3 &&
                    text[characterIndex] != ' '
                )
                {
                    typingSound.Play();
                    typingSoundDelayCounter = 0;
                }
            }

            typingSoundDelayCounter++;

            yield return new WaitForSeconds(
                typingDelay
            );
        }

        // 어떤 상황이든 Coroutine 종료 시
        // 반드시 전문이 표시되도록 보장
        cutSceneText.text = typingTargetText;

        if (typingSound != null &&
            skipTypingRequested)
        {
            typingSound.Stop();
        }

        skipTypingRequested = false;
        isTyping = false;
    }

    // ==========================================================
    // Panels
    // ==========================================================

    private void CreatePanelsFromSO()
    {
        if (data == null || data.panels == null)
        {
            return;
        }

        if (cutScenesRoot == null)
        {
            Debug.LogError(
                "CutSceneManager: cutScenesRoot가 비어있음"
            );

            return;
        }

        for (
            int panelIndex = 0;
            panelIndex < data.panels.Count;
            panelIndex++
        )
        {
            CutScenePanels panel =
                data.panels[panelIndex];

            if (panel == null)
            {
                spawnedPanels.Add(null);
                continue;
            }

            GameObject panelObject =
                CreatePanelObject(
                    panelIndex,
                    panel
                );

            spawnedPanels.Add(panelObject);
        }
    }

    private GameObject CreatePanelObject(
        int panelIndex,
        CutScenePanels panel
    )
    {
        GameObject panelObject;

        if (panel.type == CutScenePanelType.Animator)
        {
            if (panel.animatorcontroller == null)
            {
                Debug.LogWarning(
                    $"CutSceneManager: panels[{panelIndex}] AnimatorController가 null"
                );

                return null;
            }

            panelObject =
                new GameObject(
                    $"CutsceneAnimator_{panelIndex}"
                );

            panelObject.transform.SetParent(
                cutScenesRoot,
                false
            );

            RectTransform rectTransform =
                panelObject.AddComponent<RectTransform>();

            ApplyPanelTransform(
                rectTransform,
                panel
            );

            Image image =
                panelObject.AddComponent<Image>();

            image.sprite = panel.sprite;

            Animator animator =
                panelObject.AddComponent<Animator>();

            animator.runtimeAnimatorController =
                panel.animatorcontroller;
        }
        else
        {
            panelObject =
                new GameObject(
                    $"CutsceneSprite_{panelIndex}"
                );

            panelObject.transform.SetParent(
                cutScenesRoot,
                false
            );

            RectTransform rectTransform =
                panelObject.AddComponent<RectTransform>();

            ApplyPanelTransform(
                rectTransform,
                panel
            );

            Image image =
                panelObject.AddComponent<Image>();

            image.sprite = panel.sprite;
        }

        panelObject.SetActive(false);

        return panelObject;
    }

    private void ApplyPanelTransform(
        RectTransform rectTransform,
        CutScenePanels panel
    )
    {
        rectTransform.anchorMin =
            panel.anchorMin;

        rectTransform.anchorMax =
            panel.anchorMax;

        rectTransform.pivot =
            panel.pivot;

        rectTransform.anchoredPosition =
            panel.anchoredPosition;

        rectTransform.sizeDelta =
            panel.size;

        rectTransform.localScale =
            panel.scale;
    }

    private void SetPanelActive(
        int index,
        bool active
    )
    {
        if (!IsValidPanel(index))
        {
            return;
        }

        GameObject panelObject =
            spawnedPanels[index];

        if (panelObject == null)
        {
            return;
        }

        panelObject.SetActive(active);
    }

    private IEnumerator FadePanel(
        int index,
        bool fadeIn
    )
    {
        if (!IsValidPanel(index))
        {
            yield break;
        }

        GameObject panelObject =
            spawnedPanels[index];

        if (panelObject == null)
        {
            yield break;
        }

        Image image =
            panelObject.GetComponent<Image>();

        if (image == null)
        {
            panelObject.SetActive(fadeIn);
            yield break;
        }

        panelObject.SetActive(true);

        float startAlpha =
            fadeIn ? 0f : 1f;

        float endAlpha =
            fadeIn ? 1f : 0f;

        float elapsedTime = 0f;

        fadeDuration =
            fadeIn
                ? fadeInDuration
                : fadeOutDuration;

        Color color = image.color;

        color.a = startAlpha;
        image.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            color.a = Mathf.Lerp(
                startAlpha,
                endAlpha,
                elapsedTime / fadeDuration
            );

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

    private void TriggerAnimator(
        int index,
        string trigger
    )
    {
        if (!IsValidPanel(index))
        {
            return;
        }

        GameObject panelObject =
            spawnedPanels[index];

        if (panelObject == null)
        {
            return;
        }

        Animator animator =
            panelObject.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning(
                $"CutSceneManager: panels[{index}] Animator가 없음"
            );

            return;
        }

        if (string.IsNullOrEmpty(trigger))
        {
            Debug.LogWarning(
                $"CutSceneManager: panels[{index}] trigger가 비어있음"
            );

            return;
        }

        animator.SetTrigger(trigger);
    }

    private bool IsValidPanel(int index)
    {
        return
            index >= 0 &&
            index < spawnedPanels.Count;
    }

    // ==========================================================
    // Audio
    // ==========================================================

    private void PlayBGM()
    {
        if (
            data == null ||
            data.bgm == null ||
            bgm == null
        )
        {
            return;
        }

        bgm.clip = data.bgm;
        bgm.loop = data.loopBgm;
        bgm.volume = data.bgmVolume;

        bgm.Play();
    }

    private void PlaySound(
        AudioClip clip,
        float volume = 1.0f
    )
    {
        if (
            clip != null &&
            typingSound != null
        )
        {
            typingSound.PlayOneShot(
                clip,
                volume
            );
        }
    }

    // ==========================================================
    // End
    // ==========================================================

    public void EndCutScene()
    {
        if (
            bgm != null &&
            bgm.isPlaying
        )
        {
            bgm.Stop();
        }

        // 에필로그면 로비로
        if (
            CutSceneSelection.SelectedCategory ==
            CutSceneCategory.Epilogue
        )
        {
            SceneManager.LoadScene("testMain");
        }

        // 프롤로그면 스테이지로
        else
        {
            SceneManager.LoadScene("Stage");
        }
    }

    // ==========================================================
    // Text UI
    // ==========================================================

    private void CreateTextUI()
    {
        if (mainCanvas == null)
        {
            Debug.LogError(
                "CutSceneManager: mainCanvas가 비어있음"
            );

            return;
        }

        GameObject textObject =
            new GameObject("CutSceneText");

        textObject.transform.SetParent(
            mainCanvas.transform,
            false
        );

        RectTransform rectTransform =
            textObject.AddComponent<RectTransform>();

        rectTransform.anchorMin =
            new Vector2(0.1f, 0.1f);

        rectTransform.anchorMax =
            new Vector2(0.9f, 0.3f);

        rectTransform.offsetMin =
            Vector2.zero;

        rectTransform.offsetMax =
            Vector2.zero;

        cutSceneText =
            textObject.AddComponent<Text>();

        cutSceneText.font =
            prologueFont;

        cutSceneText.fontSize = 60;

        cutSceneText.color =
            Color.white;

        cutSceneText.alignment =
            TextAnchor.UpperCenter;

        cutSceneText.text =
            string.Empty;
    }
}