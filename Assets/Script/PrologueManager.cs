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
    private Coroutine fadeCoroutine = null;  // 페이드 코루틴 참조

    private SpriteRenderer sourceSpriteRenderer;
    private Image sourceImage;
    private AudioSource TypingSound;

    string[] TextSet = new string[10];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private void Start()
    {
        // 텍스트 내용
        TextSet[0] = "대학생 '소리'는 시험을 앞두고 팔자 좋게 자고 있었습니다.";
        TextSet[1] = " 심지어 꿈까지 꾸고 있네요.";
        TextSet[2] = "용사 '소리'시여, 세계를 구해주소서!!";
        TextSet[3] = "마왕이 소리의 정령들을 타락시켜 자기 부하로 바꾸고 있습니다!";
        TextSet[4] = "지금 마왕을 멈추지 못하면 세상에 소리가 없어져 버릴 겁니다!";
        TextSet[5] = "\"미안한데 꿈인 거 다 티난다.\"";
        TextSet[6] = "'소리'는 9시에 시험이 있었으므로, 꿈 꿀 시간따위 없었습니다.";
        TextSet[7] = "그러나 짜잔\n                                            \n\"아\"";
        TextSet[8] = "\"아니, 알람이 분명 울렸어야했는데...?\"\n                                            \n\n\"아?\"";
        TextSet[9] = "                          \n죽여버리겠다 마왕!!!!";

        // 씬 위치 조정
        CutScenes = GameObject.Find("CutScenes");
        RectTransform rt = CutScenes.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 1.103f);
        rt.anchoredPosition = new Vector2(0, -Screen.width * (1.103f * 0.5f + 0.1f));

        RectTransform rt_S1 = cutscenePanels[0].GetComponent<RectTransform>();
        rt_S1.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 0.7f);
        rt_S1.anchoredPosition = new Vector2(0, Screen.width * 0.2f);

        RectTransform rt_S2 = cutscenePanels[1].GetComponent<RectTransform>();
        rt_S2.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f * 0.686f);
        rt_S2.anchoredPosition = new Vector2(0, -Screen.width * 0.2f);

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

        Color color = fadeImg.color;
        color.a = 1f;
        fadeImg.color = color;

        StartCoroutine(FadeIn());

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

            Debug.Log($"{currentCutsceneIndex}");
            GoToNextCutscene();
        }

        sourceImage.sprite = sourceSpriteRenderer.sprite;
    }


    private void GoToNextCutscene()
    {
        // 마지막 컷씬 옵션
        if (currentCutsceneIndex >= cutscenePanels.Length - 2)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            StartCoroutine(prologueEnd());
        }

        if (currentCutsceneIndex == 0)
        {
            currentCutsceneIndex++;
            GameObject secondPanel = cutscenePanels[currentCutsceneIndex];
            secondPanel.SetActive(true);

            Image secondPanelImg = secondPanel.GetComponent<Image>();
            if (secondPanelImg != null)
            {
                Color panelColor = secondPanelImg.color;
                panelColor.a = 0f;
                secondPanelImg.color = panelColor;
            }

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeInPanelImage(secondPanel));
        }
        else
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(DoFadeOutIn());
        }
    }

    private IEnumerator DoFadeOutIn()
    {

        yield return StartCoroutine(FadeOut());

        PrologueText.text = "";
        if (currentCutsceneIndex == 1)
            {
                cutscenePanels[0].SetActive(false);
                cutscenePanels[1].SetActive(false);
            }
        else
            cutscenePanels[currentCutsceneIndex].SetActive(false);
        currentCutsceneIndex++;
        cutscenePanels[currentCutsceneIndex].SetActive(true);
        yield return StartCoroutine(FadeIn());
        if (currentCutsceneIndex == 1) StartCoroutine(ShowPrologueText(TextSet[currentCutsceneIndex - 1]));
        else StartCoroutine(ShowPrologueText(TextSet[currentCutsceneIndex]));

        CheckCutsceneAnimation();
    }

    private void CheckCutsceneAnimation()
    {
        if (currentCutsceneIndex == 8 && cutsceneAnimator != null)
        {
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

        /*
        Debug.Log("aaaaa");
        if (cutsceneAnimator == null)
        {
            Debug.LogError("Animator가 할당되지 않았습니다!");
        }
        foreach (var param in cutsceneAnimator.parameters)
        {
            Debug.Log("Animator Parameter: " + param.name + " (" + param.type + ")");
        }
        */
        cutsceneAnimator.SetTrigger("Play");
        // Debug.Log("트리거 'Play' 호출됨.");

        // cutsceneAnimator.Play("SceneAnimeNew", 0, 0f);
        yield return new WaitForSeconds(2.7f);

        isAnimationPlaying = false;
    }

    private IEnumerator PlayCutsceneAnimation2()
    {
        isAnimationPlaying = true;
        yield return new WaitForSeconds(1f);

        Image currentImg = cutscenePanels[9].GetComponent<Image>();
        Image nextImg = cutscenePanels[10].GetComponent<Image>();
        currentImg.sprite = nextImg.sprite;

        isAnimationPlaying = false;
    }

    private IEnumerator FadeIn()
    {
        isTransitioning = true;

        float timer = 0f;
        Color color = fadeImg.color;
        float startAlpha = color.a;  
        float endAlpha = 0f;        

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeImg.color = color;
            yield return null;
        }

        // 최종 보정
        color.a = endAlpha;
        fadeImg.color = color;

        isTransitioning = false;
    }

    private IEnumerator FadeOut()
    {
        isTransitioning = true;

        float timer = 0f;
        Color color = fadeImg.color;
        float startAlpha = color.a; 
        float endAlpha = 1f;       

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeImg.color = color;
            yield return null;
        }

        // 최종 보정
        color.a = endAlpha;
        fadeImg.color = color;

        isTransitioning = false;
    }

    // 현재는 검은 오버레이를 이용해서 화면을 페이드인 페이드아웃하는 방식을 사용하지만
    // 화면 전체를 가릴 필요가 없는 이상 바로 아래 FadeInPanelImage만 이용해서 구현 가능함.
    // 모든 컷씬 아래 FadeInPanelImage 이용해서 바꾸는데 코드적으로 예쁠듯하나 작동엔 문제 없으니 나중에 개선..
    private IEnumerator FadeInPanelImage(GameObject panel)
    {
        isTransitioning = true;
        StartCoroutine(ShowPrologueText(TextSet[1])); // 2번째 컷씬에만 사용할거라 1로 작성함

        float timer = 0f;
        Image panelImg = panel.GetComponent<Image>();
        if (panelImg == null)
        {
            isTransitioning = false;
            yield break;
        }

        Color color = panelImg.color;
        float startAlpha = color.a; 
        float endAlpha = 1f;

        while (timer < fadeDuration * 1.5f)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / (fadeDuration * 1.5f));
            panelImg.color = color;
            yield return null;
        }

        color.a = endAlpha;
        panelImg.color = color;

        isTransitioning = false;

    }

    private IEnumerator prologueEnd()
    {
        StartCoroutine(FadeIn());
        yield return 0.2f;

        SceneLinkage.StageLV = 0;   
        SceneManager.LoadScene("Tutorial"); 
    }

    public IEnumerator ShowPrologueText(string Text)
    {
        yield return StartCoroutine(ShowPrologueTextCoroutine(Text));
    }

    private IEnumerator ShowPrologueTextCoroutine(string Text)
    {
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
        if (currentCutsceneIndex == 1) PrologueText.text = TextSet[0] + TextSet[1];
        else PrologueText.text = Text;
        isTyping = false;
    }
}
