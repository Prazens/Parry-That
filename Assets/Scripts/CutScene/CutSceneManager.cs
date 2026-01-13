using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//summary: CutSceneData SO로부터 받은 데이터를 바탕으로 컷씬을 진행해주는 코드
public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private CutSceneData[] datas;
    [SerializeField] private CutSceneData data;

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform cutScenesRoot;
    [SerializeField] private Font prologueFont;
    [SerializeField] private AudioSource bgm;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typingDelay = 0.03f;

    private Text cutSceneText;
    private int currentIndex;
    private bool isTransitioning;
    private bool isTyping;
    private bool isAnimationPlaying;

    private AudioSource typingSound;
    private DatabaseManager databaseManager;

    //SO 기반으로 생성된 컷씬들
    private readonly List<GameObject> spawnedPanels = new();

    private void Start()
    {
        typingSound = GetComponent<AudioSource>();
        databaseManager = FindObjectOfType<DatabaseManager>();

        data = datas[SceneLinkage.StageLV];     

        CreateTextUI();
        CreatePanelsFromSO();

        currentIndex = 0;

        if (data != null && data.clickSteps.Count > 0)
        {
            StartCoroutine(ExecuteClickStep(0));
        }
    }

    private void Update()
    {
        //클릭 입력 확인
        if (!(Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))) return;
        //입력 무시 조건
        if (isAnimationPlaying || isTransitioning || isTyping) return;

        currentIndex++;

        if (data == null || currentIndex >= data.clickSteps.Count)
        {
            EndCutScene();
            return;
        }

        StartCoroutine(ExecuteClickStep(currentIndex));
    }
    
    //클릭 시 나오는 액션들 (액션+대기시간)
    private IEnumerator ExecuteClickStep(int index)
    {
        var step = data.clickSteps[index];

        foreach (var action in step.actions)
        {
            switch (action.actionType)
            {
                //패널 보이기
                case CutSceneActionType.ShowPanel:
                    isTransitioning = true;
                    SetPanelActive(action.panelIndex, true);
                    yield return new WaitForSeconds(action.waitseconds);
                    isTransitioning = false;
                    break;
                    
                //패널 숨기기
                case CutSceneActionType.HidePanel:
                    isTransitioning = true;
                    SetPanelActive(action.panelIndex, false);
                    yield return new WaitForSeconds(action.waitseconds);
                    isTransitioning = false;
                    break;

                //패널 페이드인
                case CutSceneActionType.FadeIn:
                    isTransitioning = true;
                    StartCoroutine(FadePanel(action.panelIndex, true));
                    yield return new WaitForSeconds(action.waitseconds);
                    isTransitioning = false;
                    break;

                //패널 페이드아웃
                case CutSceneActionType.FadeOut:
                    isTransitioning = true;
                    StartCoroutine(FadePanel(action.panelIndex, false));
                    yield return new WaitForSeconds(action.waitseconds);
                    isTransitioning = false;
                    break;

                //이전 대사 지운 후 다음 대사 출력
                case CutSceneActionType.ShowTextReset:
                    StartCoroutine(TypeText(data.textSet[currentIndex], true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //이전 대사에 붙여서 다음 대사 출력
                case CutSceneActionType.ShowTextAppend:
                    StartCoroutine(TypeText(data.textSet[currentIndex], false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //애니메이터 트리거(대기시간을 애니메이션 시간으로 활용 가능)
                case CutSceneActionType.TriggerAnimator:
                    isAnimationPlaying = true;
                    TriggerAnimator(action.panelIndex, action.animatorTrigger);
                    yield return new WaitForSeconds(action.waitseconds);
                    isAnimationPlaying = false;
                    break;
            }
        }
    }

    private void CreatePanelsFromSO()
    {
        if (data == null || data.panels == null) return;
        if (cutScenesRoot == null)
        {
            Debug.LogError("CutSceneManager: cutScenesRoot가 비어있음 (Canvas/CutScenes 연결 필요)");
            return;
        }

        for (int i = 0; i < data.panels.Count; i++)
        {
            var panel = data.panels[i];
            GameObject panelObject;

            if (panel.type == CutScenePanelType.Animator)
            {
                if (panel.animatorcontroller == null)
                {
                    Debug.LogWarning($"CutSceneManager: panels[{i}] AnimatorController가 null");
                    continue;
                }

                panelObject = new GameObject("CutsceneAnimator");
                panelObject.transform.SetParent(cutScenesRoot, false);

                Image image = panelObject.AddComponent<Image>();
                image.sprite = panel.sprite; // 필요 없으면 이 줄/필드도 제거 가능

                Animator animator = panelObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = panel.animatorcontroller;
            }
            else
            {
                panelObject = new GameObject("CutsceneSprite");
                panelObject.transform.SetParent(cutScenesRoot, false);

                Image image = panelObject.AddComponent<Image>();
                image.sprite = panel.sprite;
            }

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = panelObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = panel.anchorMin;
            rectTransform.anchorMax = panel.anchorMax;
            rectTransform.pivot     = panel.pivot;

            rectTransform.anchoredPosition = panel.anchoredPosition;
            rectTransform.sizeDelta        = panel.size;
            rectTransform.localScale       = panel.scale;

            panelObject.SetActive(false);
            spawnedPanels.Add(panelObject);
        }
    }

    private void SetPanelActive(int index, bool active)
    {
        if (!IsValidPanel(index)) return;
        spawnedPanels[index].SetActive(active);
    }

    private IEnumerator FadePanel(int index, bool fadeIn)
    {
        if (!IsValidPanel(index)) yield break;

        var image = spawnedPanels[index].GetComponent<Image>();
        if (image == null)
        {
            spawnedPanels[index].SetActive(fadeIn);
            yield break;
        }

        spawnedPanels[index].SetActive(true);

        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;
        float time = 0f;

        var color = image.color;
        color.a = start;
        image.color = color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(start, end, time / fadeDuration);
            image.color = color;
            yield return null;
        }

        //페이드아웃인 경우 비활성화
        if (!fadeIn) 
        {
            spawnedPanels[index].SetActive(false);
        }
    }

    private void TriggerAnimator(int index, string trigger)
    {
        if (!IsValidPanel(index)) return;

        Animator animator = spawnedPanels[index].GetComponent<Animator>();
        if (animator == null || string.IsNullOrEmpty(trigger))
        {
            print(index);
            return;
        }

        animator.SetTrigger(trigger);
    }

    private bool IsValidPanel(int index)
        => index >= 0 && index < spawnedPanels.Count;

    // ===================== 텍스트 =====================

    private IEnumerator TypeText(string text, bool reset)
    {
        if (reset) cutSceneText.text = "";

        isTyping = true;

        int typingSoundDelay = 0;

        for (int characterIndex = 0; characterIndex < text.Length; characterIndex++)
        {
            cutSceneText.text += text[characterIndex];

            // 타이핑 사운드
            if (typingSound != null)
            {
                if (typingSoundDelay >= 3 && text[characterIndex] != ' ')
                {
                    typingSound.Play();
                    typingSoundDelay = 0;
                }
            }

            typingSoundDelay++;
            yield return new WaitForSeconds(typingDelay);
        }

        isTyping = false;
    }


    // ===================== 기타 =====================

    public void EndCutScene()
    {
        switch (SceneLinkage.StageLV)
        {
            case 0: 
                SceneManager.LoadScene("Tutorial");
                break;
            case 1: 
                SceneManager.LoadScene("Stage1");
                break;
            case 2: 
                SceneManager.LoadScene("Stage2");
                break;
            case 3: 
                SceneManager.LoadScene("Beat Master");
                break;
            case 4:
                SceneManager.LoadScene("Stage4");
                break;
            case 5:
                SceneManager.LoadScene("Stage5");
                break;
            case 6:
                SceneManager.LoadScene("Main");
                break;
            case 7:
                SceneManager.LoadScene("Stage1Easy");
                break;
            case 8:
                SceneManager.LoadScene("Stage2Easy");
                break;
            case 9:
                SceneManager.LoadScene("Beat Master Easy");
                break;
            default:
                Debug.LogError("존재하지 않는 스테이지");
                break;
        }
    }

    //텍스트 UI 오브젝트 생성
    private void CreateTextUI()
    {
        GameObject textObject = new GameObject("CutSceneText");
        textObject.transform.SetParent(mainCanvas.transform, false);

        cutSceneText = textObject.AddComponent<Text>();
        cutSceneText.font = prologueFont;
        cutSceneText.fontSize = 60;
        cutSceneText.color = Color.white;
        cutSceneText.alignment = TextAnchor.UpperCenter;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.3f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
