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
        float charDuration = (currentLine.textRevealDuration - barDelay) / tmpText.textInfo.characterCount;

        TMP_TextInfo textInfo = tmpText.textInfo;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            originalVertices[m] = (Vector3[])textInfo.meshInfo[m].vertices.Clone();
        }

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            Debug.Log($"DialogLineController: Animating character {i} of {textInfo.characterCount}");

            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vIndex = charInfo.vertexIndex;

            Vector3 v0 = originalVertices[matIndex][vIndex + 0];
            Vector3 v1 = originalVertices[matIndex][vIndex + 1];
            Vector3 v2 = originalVertices[matIndex][vIndex + 2];
            Vector3 v3 = originalVertices[matIndex][vIndex + 3];

            Vector3 center = (v0 + v1 + v2 + v3) / 4f;

            int capturedMatIndex = matIndex;
            int capturedVIndex = vIndex;
            Vector3 capturedCenter = center;
            Vector3 capturedV0 = v0, capturedV1 = v1, capturedV2 = v2, capturedV3 = v3;

            SetCharScale(textInfo, capturedMatIndex, capturedVIndex,
                         capturedCenter, capturedV0, capturedV1, capturedV2, capturedV3,
                         0f);
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);

            Tween t = DOVirtual.Float(0f, 1f, charDuration * 1.1f, (scale) =>
            {
                SetCharScale(textInfo, capturedMatIndex, capturedVIndex,
                             capturedCenter, capturedV0, capturedV1, capturedV2, capturedV3,
                             scale);

                tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
            })
            .SetEase(ease)
            .SetTarget(tmpText);

            textSequence.Insert(i * charDuration, t);
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