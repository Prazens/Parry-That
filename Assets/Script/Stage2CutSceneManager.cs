using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage2CutSceneManager : MonoBehaviour
{
    [SerializeField] private Image fadeImg;
    private float fadeDuration = 0.3f; // 페이드 시간
    [SerializeField] private GameObject[] cutscenePanels; // 컷씬 
    private int currentCutsceneIndex = 0;
    [SerializeField] private Animator cutsceneAnimator; // 알람 애니메이션
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font PrologueFont;

    private bool isTransitioning = false;  // 현재 페이드(검은 배경) 진행 중인지 여부
    private bool isAnimationPlaying = false; // 애니메이션이 재생 중인지 여부
    private bool isTyping = false;
    private Coroutine fadeCoroutine = null;  // 페이드 코루틴 참조

    private SpriteRenderer sourceSpriteRenderer;
    private Image sourceImage;
    private AudioSource TypingSound;

    string[] TextSet = new string[12];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private DatabaseManager databaseManager;

    private void Start()
    {
        // 텍스트 내용
        TextSet[0] = "제 기억이 맞다면 여기가 마왕의 근거지입니다!";
        TextSet[1] = "벌써 왔다고? 마왕치고 너무 허술한 거 아냐?\n          \n마왕이 프로젝트를 아주 촉박하게 진행해서 그렇습니다. 한 달이 조금 넘었나?";
        TextSet[2] = "나와라 마왕!";
        TextSet[3] = "..뭐야. 왜 너네들밖에 없어?";
        TextSet[4] = "마왕님은 우리를 버리고 피난 가셨다!!!!!";
        TextSet[5] = "안전한 곳에서 더욱 완벽한 계획을 세우고 계신다!!!!!";
        TextSet[6] = "아무래도 마왕이 얘네를 방패로 두고 도망갔나 봅니다. 어쩐지 쉽게 끝난다 했어.";
        TextSet[7] = "자자, 네모머리 여러분! 난 여러분을 해치러 온 게 아닙니다! 본래의 귀여운 모습으로 돌려 드릴게요!";
        TextSet[8] = "짠! 바로 이런 고양이가 당신들의 본래...\n........";
        TextSet[9] = "......";
        TextSet[10] = "저놈들이 우리를 해치러 왔다!!!!!!!";
        TextSet[11] = "....이런.";

        // 씬 위치 조정
        //CutScenes = GameObject.Find("CutScenes");
        //RectTransform rt = CutScenes.GetComponent<RectTransform>();
        //rt.anchorMin = new Vector2(0.5f, 1f);
        //rt.anchorMax = new Vector2(0.5f, 1f);
        //rt.offsetMin = Vector2.zero;
        //rt.offsetMax = Vector2.zero;
        //rt.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 1.103f);
        //rt.anchoredPosition = new Vector2(0, -Screen.width * (1.103f * 0.5f + 0.1f));

        //RectTransform rt_S1 = cutscenePanels[0].GetComponent<RectTransform>();
        //rt_S1.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 0.7f);
        //rt_S1.anchoredPosition = new Vector2(0, Screen.width * 0.2f);

        //RectTransform rt_S2 = cutscenePanels[1].GetComponent<RectTransform>();
        //rt_S2.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 0.686f);
        //rt_S2.anchoredPosition = new Vector2(0, -Screen.width * 0.2f);

        // 텍스트창 생성
        PrologueTextObj = new GameObject("PrologueText");
        PrologueTextObj.transform.SetParent(mainCanvas.transform, false);
        PrologueText = PrologueTextObj.AddComponent<Text>();
        PrologueText.font = PrologueFont;
        PrologueText.fontSize = 60;
        PrologueText.color = Color.white;
        PrologueText.alignment = TextAnchor.UpperCenter;
        RectTransform textRect = PrologueTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.1f);
        textRect.anchorMax = new Vector2(0.9f, 0.3f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        PrologueText.text = "";
        PrologueTextObj.SetActive(true);

        // 컴포넌트 가져오기
        TypingSound = GetComponent<AudioSource>();
        databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();

        for (int i = 0; i < cutscenePanels.Length; i++)
        {
            cutscenePanels[i].SetActive(false);
        }

        // 처음 페이드인
        currentCutsceneIndex = 0;
        cutscenePanels[currentCutsceneIndex].SetActive(true);

        StartCoroutine(FadeInPanelImage(cutscenePanels[0]));

        // 첫 텍스트
        StartCoroutine(ShowPrologueText(TextSet[0]));
    }

    private void Update()
    {
        // 마우스 클릭(터치) 감지
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            // 조작 금지 조건
            if (isTransitioning) return;
            if (isTyping) return;

            // Debug.Log($"{currentCutsceneIndex}");
            StartCoroutine(GoToNextCutscene());
        }

    }


    private IEnumerator GoToNextCutscene()
    {
        switch (currentCutsceneIndex)
        {
            case 0:
                StartCoroutine(FadeInPanelImage(cutscenePanels[1]));
                StartCoroutine(ShowPrologueText(TextSet[1]));
                currentCutsceneIndex++;
                yield break;
            case 1:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[0]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[1]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[2]));
                StartCoroutine(ShowPrologueText(TextSet[2]));
                currentCutsceneIndex++;
                yield break;
            case 2:
                StartCoroutine(FadeInPanelImage(cutscenePanels[3]));
                StartCoroutine(ShowPrologueText(TextSet[3]));
                currentCutsceneIndex++;
                yield break;
            case 3:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[2]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[4]));
                StartCoroutine(ShowPrologueText(TextSet[4]));
                currentCutsceneIndex++;
                yield break;
            case 4:
                
                StartCoroutine(FadeInPanelImage(cutscenePanels[5]));
                StartCoroutine(ShowPrologueText(TextSet[5]));
                currentCutsceneIndex++;
                yield break;
            case 5:
                StartCoroutine(FadeInPanelImage(cutscenePanels[6]));
                StartCoroutine(ShowPrologueText(TextSet[6]));
                currentCutsceneIndex++;
                yield break;
            case 6:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[7]));
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                
                StartCoroutine(FadeInPanelImage(cutscenePanels[8]));
                StartCoroutine(ShowPrologueText(TextSet[8]));
                currentCutsceneIndex++;
                yield break;
            case 8:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[7]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[8]));
                yield return new WaitForSecondsRealtime(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[9]));
                StartCoroutine(ShowPrologueText(TextSet[9]));
                currentCutsceneIndex++;
                yield break;
            case 9:
                
                StartCoroutine(FadeInPanelImage(cutscenePanels[10]));
                StartCoroutine(ShowPrologueText(TextSet[10]));
                currentCutsceneIndex++;
                yield break;
            case 10:
                StartCoroutine(FadeInPanelImage(cutscenePanels[11]));
                StartCoroutine(ShowPrologueText(TextSet[11]));
                currentCutsceneIndex++;
                yield break;
            case 11:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[9]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[10]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[11]));
                yield return new WaitForSeconds(0.1f);
                // 컷씬 끝
                EndScene();
                yield break;
        }


    }

    private IEnumerator FadeInPanelImage(GameObject panel)
    {
        panel.SetActive(true);
        isTransitioning = true;

        float timer = 0f;
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg == null)
        {
            isTransitioning = false;
            yield break;
        }
        // 초기 알파값을 0으로 강제 설정
        Color color = panelImg.color;
        color.a = 0f;
        panelImg.color = color;

        float startAlpha = color.a;
        float endAlpha = 1f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / (fadeDuration));
            panelImg.color = color;
            yield return null;
        }

        color.a = endAlpha;
        panelImg.color = color;

        isTransitioning = false;

    }

    private IEnumerator FadeOutPanelImage(GameObject panel)
    {
        isTransitioning = true;

        float timer = 0f;
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg == null)
        {
            isTransitioning = false;
            yield break;
        }

        Color color = panelImg.color;
        float startAlpha = color.a;
        float endAlpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / (fadeDuration));
            panelImg.color = color;
            yield return null;
        }

        color.a = endAlpha;
        panelImg.color = color;

        isTransitioning = false;
        panel.SetActive(false);
    }


    public IEnumerator ShowPrologueText(string Text)
    {
        yield return StartCoroutine(ShowPrologueTextCoroutine(Text));
    }

    private IEnumerator ShowPrologueTextCoroutine(string Text)
    {
        PrologueText.text = "";
        isTyping = true;

        if (currentCutsceneIndex == 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 텍스트 한 글자씩 나오게
        int charIndex = 0;
        int TypingSoundDelay = 0;
        while (charIndex < Text.Length)
        {
            PrologueText.text += Text[charIndex];
            charIndex++;
            if (charIndex < Text.Length)
            {
                if (TypingSoundDelay >= 3 && Text[charIndex] != ' ')
                {
                    TypingSound.Play();
                    TypingSoundDelay = 0;
                }
            }

            TypingSoundDelay++;

            yield return new WaitForSeconds(0.03f);
        }

        isTyping = false;
    }
    public void EndScene()
    {
        // // Debug.LogError("엔딩 함수");
        databaseManager.SaveStage2Done();
        DatabaseManager.isStage2Done = true;
        SceneManager.LoadScene("Stage2");
    }
}
