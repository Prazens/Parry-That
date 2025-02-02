using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public Canvas mainCanvas;
    public Font NameFont;
    public Font DialogueFont;
    public Sprite panelImageSource;
    public GameObject triangleObj;

    private float letterDelay = 0.05f;
    private float fastLetterDelay = 0.01f;

    bool daehwaON = false;
    bool accelerate = false;

    private void Update()
    {
        if (daehwaON)
        {
            if (Input.GetMouseButtonDown(0))
            {
                accelerate = true;
                Debug.Log("클릭됨");
            }
        }
    }
    public IEnumerator ShowDialogue(Sprite characterSprite, string nameText, string dialogueText, bool isLeft)
    {
        yield return StartCoroutine(ShowDialogueCoroutine(characterSprite, nameText, dialogueText, isLeft));
    }

    IEnumerator ShowDialogueCoroutine(Sprite characterSprite, string nameText, string dialogueText, bool isLeft)
    {
        // 게임 화면 검은 필터
        GameObject overlay = new GameObject("DialogueOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // 캐릭터 생성
        GameObject Character = new GameObject("CharacterImage");
        Character.transform.SetParent(mainCanvas.transform, false);
        Image characterImage = Character.AddComponent<Image>();
        characterImage.sprite = characterSprite;
        RectTransform characterRect = Character.GetComponent<RectTransform>();
        characterRect.sizeDelta = new Vector2(300f, 300f);
        if (isLeft)
        {
            characterRect.anchorMin = new Vector2(0f, 0.33f);
            characterRect.anchorMax = new Vector2(0f, 0.33f);
            characterRect.pivot = new Vector2(0f, 0f);
            characterRect.anchoredPosition = new Vector2(-460f, -430f);
            characterRect.sizeDelta = new Vector2(1536f, 960f);
        }
        else
        {
            characterRect.anchorMin = new Vector2(1f, 0.33f);
            characterRect.anchorMax = new Vector2(1f, 0.33f);
            characterRect.pivot = new Vector2(1f, 0f);
            characterRect.anchoredPosition = new Vector2(460f, -430f);
            characterRect.sizeDelta = new Vector2(1536f, 960f);
        }

        // 대화창 패널
        GameObject dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(mainCanvas.transform, false);
        Image panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.sprite = panelImageSource;
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0.33f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 이름 텍스트 UI 생성
        GameObject NameText = new GameObject("NameText");
        NameText.transform.SetParent(dialoguePanel.transform, false);
        Text nameUIText = NameText.AddComponent<Text>();
        nameUIText.font = NameFont;
        nameUIText.fontSize = 40;
        nameUIText.color = Color.yellow;
        nameUIText.alignment = TextAnchor.MiddleLeft;
        RectTransform NameTextRect = NameText.GetComponent<RectTransform>();
        NameTextRect.anchorMin = new Vector2(0.05f, 0.8f);
        NameTextRect.anchorMax = new Vector2(0.95f, 0.9f);
        NameTextRect.offsetMin = Vector2.zero;
        NameTextRect.offsetMax = Vector2.zero;
        nameUIText.text = nameText;

        // 대화 텍스트 UI 생성
        GameObject DialogueText = new GameObject("DialogueText");
        DialogueText.transform.SetParent(dialoguePanel.transform, false);
        Text dialogueUIText = DialogueText.AddComponent<Text>();
        dialogueUIText.font = DialogueFont;
        dialogueUIText.fontSize = 36;
        dialogueUIText.color = Color.white;
        dialogueUIText.alignment = TextAnchor.UpperLeft;
        RectTransform textRect = DialogueText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.2f);
        textRect.anchorMax = new Vector2(0.95f, 0.7f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        dialogueUIText.text = "";

        // 텍스트 한 글자씩 출력 (터치 시 빠른 속도로 출력)
        daehwaON = true;
        int charIndex = 0;
        while (charIndex < dialogueText.Length)
        {
            dialogueUIText.text += dialogueText[charIndex];
            charIndex++;

            float delay = accelerate ? fastLetterDelay : letterDelay;
            yield return new WaitForSeconds(delay);
        }
        dialogueUIText.text = dialogueText;

        // 깜빡이는 삼각형 이미지 생성
        GameObject triangle = new GameObject("BlinkingTriangle");
        triangle.transform.SetParent(dialoguePanel.transform, false);
        Image triangleImage = triangle.AddComponent<Image>();
        Sprite triangleSprite = triangleObj.GetComponent<SpriteRenderer>().sprite;
        if (triangleSprite != null)
        {
            triangleImage.sprite = triangleSprite;
        }
        else
        {
            triangleImage.color = Color.black;
        }
        RectTransform triangleRect = triangle.GetComponent<RectTransform>();
        triangleRect.localRotation = Quaternion.Euler(0, 0, 180);
        triangleRect.anchorMin = new Vector2(0.9f, 0.1f);
        triangleRect.anchorMax = new Vector2(0.95f, 0.15f);
        triangleRect.offsetMin = Vector2.zero;
        triangleRect.offsetMax = Vector2.zero;
        bool keepBlinking = true;
        Coroutine blinkCoroutine = StartCoroutine(BlinkTriangle(triangleImage, () => keepBlinking));

        // 텍스트 출력 완료 후 터치할 때까지 대기
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        keepBlinking = false;
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        daehwaON = false;
        accelerate = false;
        Destroy(overlay);
        Destroy(dialoguePanel);
        Destroy(Character);

        yield break;
    }

    IEnumerator BlinkTriangle(Image triangleImage, System.Func<bool> condition)
    {
        while (condition())
        {
            triangleImage.enabled = !triangleImage.enabled;
            yield return new WaitForSeconds(0.8f);
        }
        triangleImage.enabled = false;
    }
}
