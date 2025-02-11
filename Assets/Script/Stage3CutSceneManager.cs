using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage3CutSceneManager : MonoBehaviour
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

    string[] TextSet = new string[9];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private void Start()
    {
        // �ؽ�Ʈ ����
        TextSet[0] = "���̰�.. ���� �� ��ġ����?";
        TextSet[1] = "��. ���� ���� ����? �� ���� �ָ� ����� ������ �� �ž�.";
        TextSet[2] = "���մ��� �� �ƹ��� ã�� �� ���� ������ �� ���̴�! �������� ���ѷ��� �� ���̾�! �ּҴ�...";
        TextSet[3] = "����. ���� ����!\n                \n����!";
        TextSet[4] = "�� ������.. ���� ��ȭ�� �� �ƽ��ϴ�. �������� ���� ��� ���� �������?";
        TextSet[5] = "������ �׷��� ������ ��ġ�µ�..\n�̹� �������� �������� �÷����� �� ���� ������ �𸥴ٰ�.";    // 4,5 ����
        TextSet[6] = "����! ��׵��� ���� ��ȥ�� �ַʱ��� �� �� ������ �����Դϴ�! ��Ź�帳�ϴ�!!";
        TextSet[7] = "��.. ��¿ �� ����.";
        TextSet[8] = "���� ������! ���հ��� ������ ���� ������ ����� �ּ���!!";

        // �� ��ġ ����
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
        TypingSound = GetComponent<AudioSource>();

        for (int i = 0; i < cutscenePanels.Length; i++)
        {
            cutscenePanels[i].SetActive(false);
        }

        // ó�� ���̵���
        currentCutsceneIndex = 0;
        cutscenePanels[currentCutsceneIndex].SetActive(true);

        StartCoroutine(FadeInPanelImage(cutscenePanels[0]));

        // ù �ؽ�Ʈ
        StartCoroutine(ShowPrologueText(TextSet[0]));
    }

    private void Update()
    {
        // ���콺 Ŭ��(��ġ) ����
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            // ���� ���� ����
            if (isTransitioning) return;
            if (isTyping) return;

            Debug.Log($"{currentCutsceneIndex}");
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
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[6]));
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
                yield return new WaitForSeconds(0.1f);
                // �ƾ� ��
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
        // �ʱ� ���İ��� 0���� ���� ����
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

        isTyping = false;
    }
    public void EndScene()
    {
        // Debug.LogError("���� �Լ�");
        SceneManager.LoadScene("Beat Master");
    }
}
