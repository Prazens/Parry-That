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
    private AudioSource TypingSound;
    private AudioClip TypingSoundClip;

    private float letterDelay = 0.05f;
    private float fastLetterDelay = 0f;

    private bool daehwaON = false;
    private bool accelerate = false;

    [SerializeField] private Canvas inGameScreen;

    [Header("Character Sprites")]
    [SerializeField] private Sprite[] soriSprites;
    [SerializeField] private Sprite[] spiritSprites;

    private void Start()
    {
        TypingSound = GetComponent<AudioSource>();
        inGameScreen.sortingOrder = 10;
    }

    private void Update()
    {
        if (daehwaON)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                accelerate = true;
            }
        }
    }

    public IEnumerator ShowDialogue(DialogueLine line)
    {
        if (line == null)
            yield break;

        Sprite characterSprite = GetCharacterSprite(line);
        string nameText = GetSpeakerName(line);
        bool isLeft = IsLeft(line);

        yield return StartCoroutine(ShowDialogueCoroutine(characterSprite, nameText, line.text, isLeft, line));
    }

    public IEnumerator ShowDialogueData(DialogueData dialogueData)
    {
        if (dialogueData == null || dialogueData.Lines == null)
            yield break;

        foreach (DialogueLine line in dialogueData.Lines)
        {
            yield return StartCoroutine(ShowDialogue(line));
        }
    }

    private IEnumerator ShowDialogueCoroutine(Sprite characterSprite, string nameText, string dialogueText, bool isLeft, DialogueLine line)
    {
        if (line.waitBeforeReveal > 0f)
        {
            yield return new WaitForSecondsRealtime(line.waitBeforeReveal);
        }

        GameObject overlay = new GameObject("DialogueOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject character = null;

        if (characterSprite != null)
        {
            character = new GameObject("CharacterImage");
            character.transform.SetParent(mainCanvas.transform, false);
            Image characterImage = character.AddComponent<Image>();
            characterImage.sprite = characterSprite;
            RectTransform characterRect = character.GetComponent<RectTransform>();
            characterRect.sizeDelta = new Vector2(300f, 300f);

            if (isLeft)
            {
                characterRect.anchorMin = new Vector2(0f, 0.33f);
                characterRect.anchorMax = new Vector2(0f, 0.33f);
                characterRect.pivot = new Vector2(0f, 0f);
                characterRect.anchoredPosition = new Vector2(Screen.width * 0.02f, -Screen.width * 0.17f);
                characterRect.sizeDelta = new Vector2(800, 800 * 1.2988f);
            }
            else
            {
                characterRect.anchorMin = new Vector2(1f, 0.33f);
                characterRect.anchorMax = new Vector2(1f, 0.33f);
                characterRect.pivot = new Vector2(1f, 0f);
                characterRect.anchoredPosition = new Vector2(-Screen.width * 0.02f, -Screen.width * 0.17f);
                characterRect.sizeDelta = new Vector2(800, 800 * 1.2988f);
            }
        }

        GameObject dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(mainCanvas.transform, false);
        Image panelImage = dialoguePanel.AddComponent<Image>();
        panelImage.sprite = panelImageSource;
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0.33f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject nameTextObject = new GameObject("NameText");
        nameTextObject.transform.SetParent(dialoguePanel.transform, false);
        Text nameUIText = nameTextObject.AddComponent<Text>();
        nameUIText.font = NameFont;
        nameUIText.fontSize = 100;
        nameUIText.color = Color.yellow;
        nameUIText.alignment = TextAnchor.MiddleLeft;
        RectTransform nameTextRect = nameTextObject.GetComponent<RectTransform>();
        nameTextRect.anchorMin = new Vector2(0.05f, 0.75f);
        nameTextRect.anchorMax = new Vector2(0.95f, 0.95f);
        nameTextRect.offsetMin = Vector2.zero;
        nameTextRect.offsetMax = Vector2.zero;
        nameUIText.text = nameText;

        GameObject dialogueTextObject = new GameObject("DialogueText");
        dialogueTextObject.transform.SetParent(dialoguePanel.transform, false);
        Text dialogueUIText = dialogueTextObject.AddComponent<Text>();
        dialogueUIText.font = DialogueFont;
        dialogueUIText.fontSize = 80;
        dialogueUIText.color = Color.white;
        dialogueUIText.alignment = TextAnchor.UpperLeft;
        RectTransform textRect = dialogueTextObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.2f);
        textRect.anchorMax = new Vector2(0.95f, 0.7f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        dialogueUIText.text = "";

        daehwaON = true;
        int charIndex = 0;
        int typingSoundDelay = 0;

        float revealDuration = Mathf.Max(0f, line.textRevealDuration);
        float perLetterDelay = dialogueText.Length > 0 ? revealDuration / dialogueText.Length : 0f;

        while (charIndex < dialogueText.Length)
        {
            dialogueUIText.text += dialogueText[charIndex];
            charIndex++;

            if (typingSoundDelay >= 3 && !accelerate)
            {
                TypingSound.Play();
                typingSoundDelay = 0;
            }
            typingSoundDelay++;

            float delay = accelerate ? fastLetterDelay : perLetterDelay;
            yield return new WaitForSecondsRealtime(delay);
        }

        dialogueUIText.text = dialogueText;

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

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        keepBlinking = false;
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        daehwaON = false;
        accelerate = false;

        Destroy(overlay);
        Destroy(dialoguePanel);

        if (character != null)
        {
            Destroy(character);
        }

        if (line.waitAfterReveal > 0f)
        {
            yield return new WaitForSecondsRealtime(line.waitAfterReveal);
        }
    }

    private IEnumerator BlinkTriangle(Image triangleImage, System.Func<bool> condition)
    {
        while (condition())
        {
            triangleImage.enabled = !triangleImage.enabled;
            yield return new WaitForSeconds(0.8f);
        }

        triangleImage.enabled = false;
    }

    private string GetSpeakerName(DialogueLine line)
    {
        if (!string.IsNullOrEmpty(line.speakerNameOverride))
            return line.speakerNameOverride;

        switch (line.speakerTag)
        {
            case DialogueSpeakerTag.Sori:
                return "소리";
            case DialogueSpeakerTag.Spirit:
                return "정령";
            case DialogueSpeakerTag.Narration:
                return "";
            default:
                return "";
        }
    }

    private bool IsLeft(DialogueLine line)
    {
        switch (line.speakerTag)
        {
            case DialogueSpeakerTag.Spirit:
                return true;
            case DialogueSpeakerTag.Sori:
                return false;
            case DialogueSpeakerTag.Narration:
                return true;
            default:
                return true;
        }
    }

    private Sprite GetCharacterSprite(DialogueLine line)
    {
        int expressionIndex = (int)line.expressionTag;

        switch (line.speakerTag)
        {
            case DialogueSpeakerTag.Sori:
                if (soriSprites != null && expressionIndex >= 0 && expressionIndex < soriSprites.Length)
                    return soriSprites[expressionIndex];
                break;

            case DialogueSpeakerTag.Spirit:
                if (spiritSprites != null && expressionIndex >= 0 && expressionIndex < spiritSprites.Length)
                    return spiritSprites[expressionIndex];
                break;

            case DialogueSpeakerTag.Narration:
                return null;
        }

        return null;
    }
}
