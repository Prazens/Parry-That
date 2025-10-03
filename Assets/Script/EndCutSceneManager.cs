using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndCutSceneManager : MonoBehaviour
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

    string[] TextSet = new string[18];
    GameObject PrologueTextObj;
    Text PrologueText;

    private GameObject CutScenes;

    private DatabaseManager databaseManager;

    private void Start()
    {
        // 텍스트 내용
        TextSet[0] = "마왕이 쓰러졌다! 용사님 굉장하십니다!";
        TextSet[1] = "또 큰소리쳐 봐라 마왕! (쿨럭) 누가 누굴 죽인다고?!";
        TextSet[2] = "나한테 왜 이래! 소리 좀 없앤다는 게 그렇게 큰일이야??";
        TextSet[3] = "너. 소리 때문에 시험을 망쳤다고 그랬지?";
        TextSet[4] = "그...그래.";
        TextSet[5] = "나는 네가 소리를 없애서 알람을 못 들었기 때문에 시험을 망쳤단 말이다!";
        TextSet[6] = "!!";
        TextSet[7] = "그렇다고 내가 세상 모든 시험을 없애겠다는 극단적인 망상을 하진 않았어!";
        TextSet[8] = "난 너 같은 정신 나간 쫌생이가 아니거든!";
        TextSet[9] = "...내가 죽을 죄를 지었어. 어떻게 속죄해야 할까?";
        TextSet[10] = "속죄라... 방법이 하나 있긴 하지.";
        TextSet[11] = "넌 소리를 없애는 죄를 지었으니, 소리로 갚아야겠지?";
        TextSet[12] = "그게 무슨 소리야?";
        TextSet[13] = "아이돌 가수를 해라!!!!!!!!!";
        TextSet[14] = "예???????????";
        TextSet[15] = "그렇게 소리는 마왕을 아이돌로 만들고 자신은 프로듀서가 되어 떼돈을 벌었습니다!";
        TextSet[16] = "역시 시험 따위 안 본다고 인생이 망하는 법은 없는 거로군요.";
        TextSet[17] = "                             ";

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
                StartCoroutine(FadeOutPanelImage(cutscenePanels[3]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[4]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[5]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[6]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[7]));
                StartCoroutine(ShowPrologueText(TextSet[7]));
                currentCutsceneIndex++;
                yield break;
            case 7:
                StartCoroutine(ShowPrologueText(TextSet[8]));
                currentCutsceneIndex++;
                yield break;
            case 8:
                StartCoroutine(FadeInPanelImage(cutscenePanels[8]));
                StartCoroutine(ShowPrologueText(TextSet[9]));
                currentCutsceneIndex++;
                yield break;
            case 9:
                StartCoroutine(FadeInPanelImage(cutscenePanels[9]));
                StartCoroutine(ShowPrologueText(TextSet[10]));
                currentCutsceneIndex++;
                yield break;
            case 10:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[7]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[8]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[9]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[10]));
                StartCoroutine(ShowPrologueText(TextSet[11]));
                currentCutsceneIndex++;
                yield break;
            case 11:
                StartCoroutine(FadeInPanelImage(cutscenePanels[11]));
                StartCoroutine(ShowPrologueText(TextSet[12]));
                currentCutsceneIndex++;
                yield break;
            case 12:
                StartCoroutine(FadeInPanelImage(cutscenePanels[12]));
                StartCoroutine(ShowPrologueText(TextSet[13]));
                currentCutsceneIndex++;
                yield break;
            case 13:
                StartCoroutine(FadeInPanelImage(cutscenePanels[13]));
                StartCoroutine(ShowPrologueText(TextSet[14]));
                currentCutsceneIndex++;
                yield break;
            case 14:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[10]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[11]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[12]));
                StartCoroutine(FadeOutPanelImage(cutscenePanels[13]));
                yield return new WaitForSeconds(fadeDuration);
                StartCoroutine(FadeInPanelImage(cutscenePanels[14]));
                StartCoroutine(ShowPrologueText(TextSet[15]));
                currentCutsceneIndex++;
                yield break;
            case 15:
                StartCoroutine(ShowPrologueText(TextSet[16]));
                currentCutsceneIndex++;
                yield break;
            case 16:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[14]));
                yield return new WaitForSeconds(fadeDuration);
                fadeDuration = 2f;
                StartCoroutine(FadeInPanelImage(cutscenePanels[15]));
                StartCoroutine(ShowPrologueText(TextSet[17]));
                currentCutsceneIndex++;
                yield break;
            case 17:
                StartCoroutine(FadeOutPanelImage(cutscenePanels[15]));
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
        SceneManager.LoadScene("Main");
    }
}
