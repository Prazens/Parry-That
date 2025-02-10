using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Stage1CutScene : MonoBehaviour
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

    string[] TextSet = new string[11];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private void Start()
    {
        // 텍스트 내용
        TextSet[0] = "    ";
        TextSet[1] = "    ";
        TextSet[2] = "끼야오!! 정말 잘하셨습니다!! 제가 사람 보는 눈이 참으로 좋았던 모양입니다!";
        TextSet[3] = "듣고 계신가요?";
        TextSet[4] = "와! 고양이들이다!!! 얘네 진짜 귀엽다!";
        TextSet[5] = "용사님. 고양이 놈들은 햄스터의 천적인데..";
        TextSet[6] = "내가 마왕이면 이 귀여운 애들을 그렇게 성의없는 디자인으로 바꾸진 않았을 거야. 이것 봐. 귀여워서 때릴 수가 없잖아!";
        TextSet[7] = "용사님!!! 마왕 입장이 되어 보려 하지 마십시오! 당신의 시험을 망친 놈입니다!!!";
        TextSet[8] = "시험이야 뭐, 공부도 안 했었고... 늦잠 잔 게 한두 번도 아니고...";
        TextSet[9] = "그놈이 고양이들을 영원히 네모머리로 바꿔 놓을 겁니다!!!!";
        TextSet[10] = "마왕 죽이러 가자!!!!!";


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
        PrologueText.fontSize = 60 * (Screen.width / 1080);
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

            Debug.Log($"{currentCutsceneIndex}");
            StartCoroutine(GoToNextCutscene());
        }

    }


    private IEnumerator GoToNextCutscene()
    {
        switch(currentCutsceneIndex)
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
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                yield return new WaitForSeconds(fadeDuration);
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
                StartCoroutine(FadeInPanelImage(cutscenePanels[7]));
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[7]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[8]));
                StartCoroutine(ShowPrologueText(TextSet[8]));
                currentCutsceneIndex++;
                yield break;
            case 8:
                StartCoroutine(FadeInPanelImage(cutscenePanels[9]));
                StartCoroutine(ShowPrologueText(TextSet[9]));
                currentCutsceneIndex++;
                yield break;
            case 9:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[8]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[9]));
                yield return new WaitForSecondsRealtime(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[10]));
                StartCoroutine(ShowPrologueText(TextSet[10]));
                currentCutsceneIndex++;
                yield break;
            case 10:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[10]));
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

            yield return new WaitForSeconds(0.05f);
        }
        
        isTyping = false;
    }
    public void EndScene()
    {
        Debug.LogError("엔딩 함수");
        SceneManager.LoadScene("Stage1");
    }
}
