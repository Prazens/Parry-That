using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrologueManager : MonoBehaviour
{
    [SerializeField] private Image fadeImg;
    private float fadeDuration = 0.3f; // ���̵� �ð�
    [SerializeField] private GameObject[] cutscenePanels; // �ƾ� 
    private int currentCutsceneIndex = 0;
    [SerializeField] private Animator cutsceneAnimator; // �˶� �ִϸ��̼�
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font PrologueFont;

    private bool isTransitioning = false;  // ���� ���̵�(���� ���) ���� ������ ����
    private bool isAnimationPlaying = false; // �ִϸ��̼��� ��� ������ ����
    private bool isTyping = false;
    private Coroutine fadeCoroutine = null;  // ���̵� �ڷ�ƾ ����

    private SpriteRenderer sourceSpriteRenderer;
    private Image sourceImage;
    private AudioSource TypingSound;

    string[] TextSet = new string[10];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private void Start()
    {
        // �ؽ�Ʈ ����
        TextSet[0] = "���л� '�Ҹ�'�� ������ �յΰ� ���� ���� �ڰ� �־����ϴ�.";
        TextSet[1] = " ������ �ޱ��� �ٰ� �ֳ׿�.";
        TextSet[2] = "��� '�Ҹ�'�ÿ�, ���踦 �����ּҼ�!!";
        TextSet[3] = "������ �Ҹ��� ���ɵ��� Ÿ������ �ڱ� ���Ϸ� �ٲٰ� �ֽ��ϴ�!";
        TextSet[4] = "���� ������ ������ ���ϸ� ���� �Ҹ��� ������ ���� �̴ϴ�!";
        TextSet[5] = "\"�̾��ѵ� ���� �� �� Ƽ����.\"";
        TextSet[6] = "'�Ҹ�'�� 9�ÿ� ������ �־����Ƿ�, �� �� �ð����� �������ϴ�.";
        TextSet[7] = "�׷��� ¥��\n                                            \n\"��\"";
        TextSet[8] = "\"�ƴ�, �˶��� �и� ��Ⱦ���ߴµ�...?\"\n                                            \n\n\"��?\"";
        TextSet[9] = "                          \n�׿������ڴ� ����!!!!";

        // �� ��ġ ����
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

        // �ؽ�Ʈâ ����
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

        // ������Ʈ ��������
        sourceSpriteRenderer = cutscenePanels[8].GetComponent<SpriteRenderer>();
        sourceImage = cutscenePanels[8].GetComponent<Image>();
        TypingSound = GetComponent<AudioSource>();

        for (int i = 0; i < cutscenePanels.Length; i++)
        {
            cutscenePanels[i].SetActive(false);
        }

        // ó�� ���̵���
        currentCutsceneIndex = 0;
        cutscenePanels[currentCutsceneIndex].SetActive(true);

        Color color = fadeImg.color;
        color.a = 1f;
        fadeImg.color = color;

        StartCoroutine(FadeIn());

        // ù �ؽ�Ʈ
        StartCoroutine(ShowPrologueText(TextSet[0]));
    }

    private void Update()
    {
        // ���콺 Ŭ��(��ġ) ����
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            // ���� ���� ����
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
        // ������ �ƾ� �ɼ�
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
            Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�!");
        }
        foreach (var param in cutsceneAnimator.parameters)
        {
            Debug.Log("Animator Parameter: " + param.name + " (" + param.type + ")");
        }
        */
        cutsceneAnimator.SetTrigger("Play");
        // Debug.Log("Ʈ���� 'Play' ȣ���.");

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

        // ���� ����
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

        // ���� ����
        color.a = endAlpha;
        fadeImg.color = color;

        isTransitioning = false;
    }

    // ����� ���� �������̸� �̿��ؼ� ȭ���� ���̵��� ���̵�ƿ��ϴ� ����� ���������
    // ȭ�� ��ü�� ���� �ʿ䰡 ���� �̻� �ٷ� �Ʒ� FadeInPanelImage�� �̿��ؼ� ���� ������.
    // ��� �ƾ� �Ʒ� FadeInPanelImage �̿��ؼ� �ٲٴµ� �ڵ������� ���ܵ��ϳ� �۵��� ���� ������ ���߿� ����..
    private IEnumerator FadeInPanelImage(GameObject panel)
    {
        isTransitioning = true;
        StartCoroutine(ShowPrologueText(TextSet[1])); // 2��° �ƾ����� ����ҰŶ� 1�� �ۼ���

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

        // �ؽ�Ʈ �� ���ھ� ������
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
