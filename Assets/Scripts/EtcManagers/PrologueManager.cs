using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrologueManager : MonoBehaviour
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
    private bool isMusicPlaying = false;
    private Coroutine fadeCoroutine = null;  // 페이드 코루틴 참조

    private SpriteRenderer sourceSpriteRenderer;
    private Image sourceImage;
    private AudioSource TypingSound;
    [SerializeField] private AudioSource bgm;

    string[] TextSet = new string[10];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private void Start()
    {
        Application.targetFrameRate = 120;

        // 텍스트 내용
        TextSet[0] = "대학생 '소리'는 시험을 앞두고 팔자 좋게 자고 있었습니다.";
        TextSet[1] = " 심지어 꿈까지 꾸고 있네요.";
        TextSet[2] = "용사 '소리'시여, 세계를 구해주소서!!";
        TextSet[3] = "마왕이 소리의 정령들을 타락시켜 자기 부하로 바꾸고 있습니다!";
        TextSet[4] = "지금 마왕을 멈추지 못하면 세상에 소리가 없어져 버릴 겁니다!";
        TextSet[5] = "\"미안한데 꿈인 거 다 티난다.\"";
        TextSet[6] = "'소리'는 9시에 시험이 있었으므로, 꿈 꿀 시간따위 없었습니다.";
        TextSet[7] = "그러나 짜잔\n                                     \n\"아\"";
        TextSet[8] = "\"아니, 알람이 분명 울렸어야했는데...?\"\n                                     \n\n\"아?\"";
        TextSet[9] = "죽여버리겠다 마왕!!!!";

        //// 씬 위치 조정
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
        sourceSpriteRenderer = cutscenePanels[8].GetComponent<SpriteRenderer>();
        sourceImage = cutscenePanels[8].GetComponent<Image>();
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
            if (isAnimationPlaying) return;
            if (isTransitioning) return;
            if (isTyping) return;

            // Debug.Log($"{currentCutsceneIndex}");
            StartCoroutine(GoToNextCutscene());
        }

        sourceImage.sprite = sourceSpriteRenderer.sprite;


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
                StartCoroutine(FadeOutPanelImage(cutscenePanels[2]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[3]));
                StartCoroutine(ShowPrologueText(TextSet[3]));
                currentCutsceneIndex++;
                yield break;
            case 3:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[4]));
                StartCoroutine(ShowPrologueText(TextSet[4]));
                currentCutsceneIndex++;
                yield break;
            case 4:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[5]));
                StartCoroutine(ShowPrologueText(TextSet[5]));
                currentCutsceneIndex++;
                yield break;
            case 5:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[6]));
                StartCoroutine(ShowPrologueText(TextSet[6]));
                currentCutsceneIndex++;
                yield break;
            case 6:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[7]));
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[7]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[8]));
                StartCoroutine(ShowPrologueText(TextSet[8]));
                StartCoroutine(PlayCutsceneAnimation());
                currentCutsceneIndex++;
                yield break;
            case 8:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[8]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[9]));
                StartCoroutine(PlayCutsceneAnimation2());
                yield return new WaitForSeconds(1.4f);
                currentCutsceneIndex++;
                yield break;
            case 9:
                // 컷씬 끝
                prologueEnd();
                yield break;

        }
    }


    private void CheckCutsceneAnimation()
    {
        if (currentCutsceneIndex == 8 && cutsceneAnimator != null)
        {
            // bgm.Stop();
            StartCoroutine(PlayCutsceneAnimation());
        }

        if (currentCutsceneIndex == 9)
        {
            StartCoroutine(PlayCutsceneAnimation2());
        }
    }

    private IEnumerator PlayCutsceneAnimation()
    {
        isAnimationPlaying = true;
        yield return new WaitForSeconds(0.3f);

        cutsceneAnimator.SetTrigger("Play");

        // cutsceneAnimator.Play("SceneAnimeNew", 0, 0f);
        yield return new WaitForSeconds(2f);

        isAnimationPlaying = false;
    }

    private IEnumerator PlayCutsceneAnimation2()
    {
        isAnimationPlaying = true;
        yield return new WaitForSeconds(0.7f);

        Image currentImg = cutscenePanels[9].GetComponent<Image>();
        Image nextImg = cutscenePanels[10].GetComponent<Image>();
        currentImg.sprite = nextImg.sprite;

        StartCoroutine(ShowPrologueText(TextSet[9]));

        isAnimationPlaying = false;
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

    private void prologueEnd()
    {
        SceneLinkage.StageLV = 0;   
        SceneManager.LoadScene("Tutorial"); 
    }

    public IEnumerator ShowPrologueText(string Text)
    {
        yield return StartCoroutine(ShowPrologueTextCoroutine(Text));
    }

    private IEnumerator ShowPrologueTextCoroutine(string Text)
    {
        if (currentCutsceneIndex != 0) PrologueText.text = "";
        isTyping = true;

        if (currentCutsceneIndex == 0 && !isMusicPlaying)
        {
            yield return new WaitForSeconds(0.1f);
            bgm.Play();
            isMusicPlaying = true;
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
        if (currentCutsceneIndex == 1) PrologueText.text = TextSet[0] + TextSet[1];
        else PrologueText.text = Text;
        isTyping = false;
    }
}
