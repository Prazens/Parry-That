using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage4CutSceneManager : MonoBehaviour
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

    string[] TextSet = new string[9];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private DatabaseManager databaseManager;

    private void Start()
    {
        // 텍스트 내용
        TextSet[0] = "용사님. 이 물범들 생각보다 너무 강한 것 같지 않나요?";
        TextSet[1] = "확실히 심상치가 않아. 밸런스 조정을 실패했나?";
        TextSet[2] = "잠깐만요, 용사님! 물범들이 누군가에게 조종당하고 있는 것 같습니다!";
        TextSet[3] = "(콰아아아아아아아아)";
        TextSet[4] = "마왕이다!!";
        TextSet[5] = "마왕이다!!!";
        TextSet[6] = "좀 조용히 놀라 줄래?";
        TextSet[7] = "왜 소리를 없앴나!!";
        TextSet[8] = "물어보길 기다리고 있었다! 설명해 주마.";
        TextSet[9] = "나는 원래 고시생으로서 열심히 시험 준비를 하고 있던 선량한 사람이었다.";
        TextSet[10] = "하지만 세상에는 불필요한 소음을 내는 사람들이 너무 많았고..";
        TextSet[11] = "그 사람들의 방해 때문에 나는 몇 번이고 탈락만을 반복했지.";
        TextSet[12] = "그때 결심했다. 세상에 소음이 더는 들리지 않게 하겠다고!";
        TextSet[13] = "간절히 바란 끝에 드디어 내가 바라는 힘을 얻";    // 4,5 동시
        TextSet[14] = "기습스매쉬!!!\n(시@밤쾅)";
        TextSet[15] = "철푸덕";
        TextSet[16] = "좋아. 틀림없이 해치웠어!";
        TextSet[17] = "(기습당한 분노로 각성)\n널 죽여버리겠다!!";
        TextSet[18] = "...이게 아닌데. \n (중얼)용사를 잘못 골랐나...";

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
                
                StartCoroutine(FadeInPanelImage(cutscenePanels[2]));
                StartCoroutine(ShowPrologueText(TextSet[2]));
                currentCutsceneIndex++;
                yield break;
            case 2:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[0]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[1]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[2]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[3]));
                StartCoroutine(ShowPrologueText(TextSet[3]));
                currentCutsceneIndex++;
                yield break;
            case 3:
                StartCoroutine(FadeInPanelImage(cutscenePanels[4]));
                StartCoroutine(ShowPrologueText(TextSet[4]));
                currentCutsceneIndex++;
                yield break;
            case 4:
                StartCoroutine(ShowPrologueText(TextSet[5]));
                currentCutsceneIndex++;
                yield break;
            case 5:
                StartCoroutine(FadeInPanelImage(cutscenePanels[5]));
                StartCoroutine(ShowPrologueText(TextSet[6]));
                currentCutsceneIndex++;
                yield break;
            case 6:
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[6]));
                StartCoroutine(ShowPrologueText(TextSet[8]));
                currentCutsceneIndex++;
                yield break;
            case 8:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
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
        databaseManager.SaveStage3Done();
        DatabaseManager.isStage3Done = true;
        SceneManager.LoadScene("Beat Master");
    }
}
