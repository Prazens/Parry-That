using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogLineController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private RectTransform dialogBar;
    [SerializeField] private RectTransform speakerIcon;
    [SerializeField] private Ease ease = Ease.OutBack;

    [SerializeField] private AudioSource typingSoundSource;
    [SerializeField] private int typingSoundInterval = 3;

    [Header("R모드")]
    [SerializeField] private bool isRightSide = false;
    [Header("바 설정")]
    [SerializeField] private float maxBarWidth = 1000f;
    private DialogueLine currentLine;
    private string currentSpeakerName;
    private Sprite currentSpeakerSprite;

    private Sequence dlSequence;
    private Sequence appearSequence;
    private Sequence textSequence;
    private Sequence disappearSequence;

    private StageDialogManager sdm;
    private float barDelay;
    private LayoutElement le;

    private Vector2 originalSpeakerIconSize;

    private void Awake()
    {
        Debug.Log("DialogLineController: Awake");

        sdm = StageDialogManager.Instance;
        le = transform.GetComponent<LayoutElement>();
        originalSpeakerIconSize = speakerIcon.sizeDelta;
        barDelay = sdm.speakerCreation + sdm.speakerDuration + sdm.lineCreation + sdm.lineDuration;
    }

    public void Init(DialogueLine line, string speakerName, Sprite speakerSprite, bool isLeft)
    {
        Debug.Log($"DialogLineController: Initializing line - Speaker: {line.speakerTag}, Expression: {line.expressionTag}, Text: {line.text}");

        currentLine = line;
        currentSpeakerName = speakerName;
        currentSpeakerSprite = speakerSprite;
        isRightSide = !isLeft;

        Image iconImage = speakerIcon.GetChild(0).GetComponent<Image>();
        iconImage.sprite = currentSpeakerSprite;
        iconImage.enabled = currentSpeakerSprite != null;

        speakerIcon.sizeDelta = Vector2.zero;
        dialogBar.anchoredPosition = new Vector2(0, speakerIcon.anchoredPosition.y);
    }

    public void FinishDialogLine()
    {
        Debug.Log("DialogLineController: FinishDialogLine");

        sdm.FinishDialogLine();
        Destroy(gameObject);
    }

    public Sequence MakeSequence()
    {
        Debug.Log("DialogLineController: MakeSequence");

        dlSequence = DOTween.Sequence();
        dlSequence.OnComplete(() => FinishDialogLine());
        appearSequence = DOTween.Sequence();
        textSequence = DOTween.Sequence();
        disappearSequence = DOTween.Sequence();

        LineAppear();
        dlSequence.Insert(currentLine.waitBeforeReveal, appearSequence);
        TextAppear();
        dlSequence.Insert(currentLine.waitBeforeReveal + barDelay, textSequence);
        LineDisappear();
        dlSequence.Insert(currentLine.waitBeforeReveal + currentLine.textRevealDuration + currentLine.waitAfterReveal - barDelay,
                          disappearSequence);
        return dlSequence;
    }

    private void LineAppear()
    {
        Debug.Log("DialogLineController: Starting LineAppear");

        tmpText.text = currentLine.text;
        float textBarWidth = tmpText.GetPreferredValues().x + originalSpeakerIconSize.x * 1.1f;
        textBarWidth = Mathf.Min(textBarWidth, maxBarWidth);
        tmpText.text = "";

        if (currentSpeakerSprite != null)
        {
            appearSequence.Append(speakerIcon.DOSizeDelta(originalSpeakerIconSize, sdm.speakerCreation).SetEase(ease));
        }

        if (isRightSide)
        {
            appearSequence.Insert(sdm.speakerCreation + sdm.speakerDuration,
                dialogBar.DOAnchorPosX(-textBarWidth, sdm.lineCreation).SetEase(ease));
        }
        else
        {
            appearSequence.Insert(sdm.speakerCreation + sdm.speakerDuration,
                dialogBar.DOAnchorPosX(textBarWidth, sdm.lineCreation).SetEase(ease));
        }
    }

    private void TextAppear()
    {
        Debug.Log("DialogLineController: Starting TextAppear");

        DOTween.Kill(tmpText);

        tmpText.text = currentLine.text;
        tmpText.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpText.textInfo;
        int visibleCharCount = textInfo.characterCount;

        if (visibleCharCount <= 0)
            return;

        float charDuration = (currentLine.textRevealDuration - barDelay) / visibleCharCount;
        if (charDuration < 0f)
            charDuration = 0f;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int meshIndex = 0; meshIndex < textInfo.meshInfo.Length; meshIndex++)
        {
            originalVertices[meshIndex] = (Vector3[])textInfo.meshInfo[meshIndex].vertices.Clone();
        }

        int typingSoundDelay = 0;
        int visibleIndex = 0;

        for (int characterIndex = 0; characterIndex < textInfo.characterCount; characterIndex++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[characterIndex];
            if (!charInfo.isVisible)
                continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vIndex = charInfo.vertexIndex;

            Vector3 v0 = originalVertices[matIndex][vIndex + 0];
            Vector3 v1 = originalVertices[matIndex][vIndex + 1];
            Vector3 v2 = originalVertices[matIndex][vIndex + 2];
            Vector3 v3 = originalVertices[matIndex][vIndex + 3];
            Vector3 center = (v0 + v1 + v2 + v3) / 4f;

            float insertTime = visibleIndex * charDuration;

            SetCharScale(
                textInfo,
                matIndex,
                vIndex,
                center,
                v0,
                v1,
                v2,
                v3,
                0f
            );
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);

            bool shouldPlayTypingSound = typingSoundDelay >= typingSoundInterval;

            textSequence.InsertCallback(insertTime, () =>
            {
                if (shouldPlayTypingSound && typingSoundSource != null && typingSoundSource.clip != null)
                {
                    typingSoundSource.PlayOneShot(typingSoundSource.clip);
                }
            });

            Tween tween = DOVirtual.Float(0f, 1f, charDuration * 1.1f, (scale) =>
            {
                SetCharScale(
                    textInfo,
                    matIndex,
                    vIndex,
                    center,
                    v0,
                    v1,
                    v2,
                    v3,
                    scale
                );

                tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
            })
            .SetEase(ease)
            .SetTarget(tmpText);

            textSequence.Insert(insertTime, tween);

            if (typingSoundDelay >= typingSoundInterval)
            {
                typingSoundDelay = 0;
            }
            typingSoundDelay++;

            visibleIndex++;
        }
    }

    private void LineDisappear()
    {
        Debug.Log("DialogLineController: Starting LineDisappear");

        disappearSequence.InsertCallback(0f, () => tmpText.transform.gameObject.SetActive(false));

        disappearSequence.Insert(sdm.lineDuration, dialogBar.DOAnchorPosX(0, sdm.lineCreation).SetEase(ease));

        if (currentSpeakerSprite != null)
        {
            disappearSequence.Insert(barDelay - sdm.speakerCreation,
                                     speakerIcon.DOSizeDelta(Vector2.zero, sdm.speakerCreation).SetEase(ease));
        }

        disappearSequence.Insert(barDelay,
            DOTween.To(() => le.preferredHeight, h => le.preferredHeight = h, 0f, sdm.lineUpDuration)
            .SetEase(ease)
        );
    }

    private void SetCharScale(TMP_TextInfo textInfo, int matIndex, int vIndex,
                              Vector3 center, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
                              float scale)
    {
        Debug.Log($"DialogLineController: SetCharScale: {scale}");

        Vector3[] verts = textInfo.meshInfo[matIndex].vertices;
        verts[vIndex + 0] = center + (v0 - center) * scale;
        verts[vIndex + 1] = center + (v1 - center) * scale;
        verts[vIndex + 2] = center + (v2 - center) * scale;
        verts[vIndex + 3] = center + (v3 - center) * scale;
    }

    private void SetCharAlpha(TMP_TextInfo textInfo, int matIndex, int vIndex, byte alpha)
    {
        Color32[] colors = textInfo.meshInfo[matIndex].colors32;

        for (int v = 0; v < 4; v++)
        {
            Color32 c = colors[vIndex + v];
            c.a = alpha;
            colors[vIndex + v] = c;
        }
    }
}