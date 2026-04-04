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
    private int currentIndex; //컷씬이 진행된 횟수를 나타내는 인덱스

    private bool isTyping; //대사가 나오는 중인지 확인하는 변수

    private bool isStepRunning; //클릭 후 나올 모든 액션이 끝났는지 확인하는 변수


    private AudioSource typingSound;
    private DatabaseManager databaseManager;

    //SO 기반으로 생성된 컷씬들
    private readonly List<GameObject> spawnedPanels = new();

    //컴포넌트 연결 및 컷씬 데이터 설정
    private void Start()
    {
        typingSound = GetComponent<AudioSource>();
        databaseManager = FindObjectOfType<DatabaseManager>();
        
        //스테이지에 따라 컷씬 데이터 연결
        if (data == null)
        {
            int stageId = StageSelection.SelectedStageId;

            if (datas != null && stageId >= 0 && stageId < datas.Length)
            {
                data = datas[stageId];
            }
            else
            {
                data = null;
            }
        }
        
        //컷씬이 없는 스테이지면 넘어감
        if (data == null)
        {
            EndCutScene();
            return;
        }

        CreateTextUI();
        CreatePanelsFromSO();
        PlayBGM();

        currentIndex = 0;

        if (data.clickSteps.Count > 0)
        {
            StartCoroutine(ExecuteClickStep(0));
        }
    }

    private void Update()
    {
        //클릭 입력 확인
        if (!(Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))) return;
        //입력 무시 조건
        if (isStepRunning || isTyping) return;
        
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
        isStepRunning = true;

        var step = data.clickSteps[index];

        foreach (var action in step.actions)
        {
            switch (action.actionType)
            {
                //패널 보이기
                case CutSceneActionType.ShowPanel:
                    SetPanelActive(action.index, true);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;
                    
                //패널 숨기기
                case CutSceneActionType.HidePanel:
                    SetPanelActive(action.index, false);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //패널 페이드인
                case CutSceneActionType.FadeIn:
                    StartCoroutine(FadePanel(action.index, true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //패널 페이드아웃
                case CutSceneActionType.FadeOut:
                    StartCoroutine(FadePanel(action.index, false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //이전 대사 지운 후 다음 대사 출력
                case CutSceneActionType.ShowTextReset:
                    StartCoroutine(TypeText(data.textSet[action.index], true));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //이전 대사에 붙여서 다음 대사 출력
                case CutSceneActionType.ShowTextAppend:
                    StartCoroutine(TypeText(data.textSet[action.index], false));
                    yield return new WaitForSeconds(action.waitseconds);
                    break;

                //애니메이터 트리거(대기시간을 애니메이션 시간으로 활용 가능)
                case CutSceneActionType.TriggerAnimator:
                    TriggerAnimator(action.index, action.animatorTrigger);
                    yield return new WaitForSeconds(action.waitseconds);
                    break;
            }
        }

        yield return new WaitForSeconds(0.3f);
        isStepRunning = false;
    }

    //SO 데이터를 바탕으로 스프라이트 및 애니메이터를 가진 패널 오브젝트 생성
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

    private void PlayBGM()
    {
        if (data == null || data.bgm == null)
            return;

        bgm.clip = data.bgm;
        bgm.loop = data.loopBgm;
        bgm.volume = data.bgmVolume;
        bgm.Play();
    }

    //패널 활성화 및 비활성화
    private void SetPanelActive(int index, bool active)
    {
        if (!IsValidPanel(index)) return;
        spawnedPanels[index].SetActive(active);
    }

    //패널 페이드인 및 페이드아웃
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

    //애니메이터 트리거
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

    //텍스트 출력
    private IEnumerator TypeText(string text, bool reset)
    {
        isTyping = true;

        if (reset) cutSceneText.text = "";

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

    //컷씬 종료 시 Stage씬 불러옴
    public void EndCutScene()
    {
        if (StageSelection.SelectedStageId == 5) SceneManager.LoadScene("testMain");
        else SceneManager.LoadScene("Stage");
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
