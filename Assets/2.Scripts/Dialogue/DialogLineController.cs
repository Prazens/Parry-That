using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEditor.Overlays;

public class DialogLineController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private RectTransform dialogBar;
    [SerializeField] private RectTransform speakerIcon;
    [SerializeField] private Ease ease = Ease.OutBack; // 글자 등장 애니메이션 이징 함수

    [Header("R모드")]
    [SerializeField] private bool isRightSide = false; // 오른쪽 발화자인지 여부 (R모드)

    // 현재 대사 정보
    private DialogueLine currentLine;

    // 트윈 시퀀스, dlSequence에 appearSequence, textSequence, disappearSequence를 차례로 넣어서 관리
    private Sequence dlSequence;
    private Sequence appearSequence;
    private Sequence textSequence;
    private Sequence disappearSequence;

    // 편의성
    private StageDialogManager sdm;
    private float barDelay;
    private LayoutElement le;

    // 초기값 저장
    private Vector2 originalSpeakerIconSize;

    private void Awake()
    {
        Debug.Log("DialogLineController: Awake");

        // StageDialogManager에서 Instantiate 후 이 스크립트의 Init()이 호출된다고 가정
        // 편의성 값들과 초기값 저장
        sdm = StageDialogManager.Instance;
        le = transform.GetComponent<LayoutElement>();
        originalSpeakerIconSize = speakerIcon.sizeDelta;
        barDelay = sdm.speakerCreation + sdm.speakerDuration + sdm.lineCreation + sdm.lineDuration;
    }

    /// <summary>
    /// 대사 정보로 초기화
    /// <para>발화자 아이콘과 대화창을 초기 상태로 설정</para>
    /// </summary>
    /// <param name="line">초기화할 대사 정보</param>
    public void Init(DialogueLine line)
    {
        Debug.Log($"DialogLineController: Initializing line - Speaker: {line.speakerTag}, Expression: {line.expressionTag}, Text: {line.text}");

        currentLine = line;
        // 발화자 아이콘 설정
        speakerIcon.GetChild(0).GetComponent<Image>().sprite = sdm.speakerIcons[(int)currentLine.speakerTag][currentLine.expressionTag];
        speakerIcon.sizeDelta = Vector2.zero;  // 아이콘 초기 크기 0

        dialogBar.anchoredPosition = new Vector2(0, speakerIcon.anchoredPosition.y);  // 아이콘 안보이게
    }

    public void FinishDialogLine()
    {
        Debug.Log("DialogLineController: FinishDialogLine");

        // 대화 라인 하나 종료 처리
        sdm.FinishDialogLine();
        Destroy(gameObject);
    }

    /// <summary>
    /// 대사 재생 시퀀스 생성
    /// </summary>
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

    /// <summary>
    /// 대화창과 발화자 아이콘이 등장하는 효과 구현
    /// </summary>
    private void LineAppear()
    {
        Debug.Log("DialogLineController: Starting LineAppear");

        tmpText.text = currentLine.text;
        float textBarWidth = tmpText.GetPreferredValues().x + originalSpeakerIconSize.x * 1.1f;
        tmpText.text = "";

        appearSequence.Append(speakerIcon.DOSizeDelta(originalSpeakerIconSize, sdm.speakerCreation).SetEase(ease));
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

    /// <summary>
    /// 글자 하나씩 등장하는 효과 구현
    /// <para>DOTween의 DOVirtual.Float을 활용하여 글자마다 개별적으로 애니메이션 적용</para>
    /// </summary>
    private void TextAppear()
    {
        Debug.Log("DialogLineController: Starting TextAppear");

        // 기존 트윈이 있다면 중단
        DOTween.Kill(tmpText);

        tmpText.text = currentLine.text;

        // 텍스트를 설정한 직후엔 TMP가 메시 정보를 아직 계산하지 않은 상태
        // ForceMeshUpdate()를 호출해야 textInfo가 최신화됨
        tmpText.ForceMeshUpdate();
        float charDuration = (currentLine.textRevealDuration - barDelay)
                             / tmpText.textInfo.characterCount;  // 글자당 할당 시간 계산

        TMP_TextInfo textInfo = tmpText.textInfo;

        // 원본 버텍스 위치를 보관할 배열
        // meshInfo의 vertices를 직접 수정하기 때문에 원본을 따로 복사해 두지 않으면
        // "원래 이 글자가 어디에 있어야 하는지"를 알 수 없게 됨
        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            originalVertices[m] = (Vector3[])textInfo.meshInfo[m].vertices.Clone();
        }

        // 글자 수만큼 순회
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            Debug.Log($"DialogLineController: Animating character {i} of {textInfo.characterCount}");

            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 공백, 줄바꿈 등 렌더링되지 않는 글자는 건너뜀
            // isVisible이 false인 글자는 vertexIndex 값이 무의미함
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vIndex = charInfo.vertexIndex;

            // 이 글자의 원본 버텍스 4개
            Vector3 v0 = originalVertices[matIndex][vIndex + 0];  // 좌하단
            Vector3 v1 = originalVertices[matIndex][vIndex + 1];  // 좌상단
            Vector3 v2 = originalVertices[matIndex][vIndex + 2];  // 우상단
            Vector3 v3 = originalVertices[matIndex][vIndex + 3];  // 우하단

            // 글자의 중심점 계산
            // 스케일은 "중심을 기준으로 버텍스를 밀고 당기는 것"이므로 중심이 필요
            Vector3 center = (v0 + v1 + v2 + v3) / 4f;

            // 클로저 캡처를 위해 로컬 변수로 복사
            int capturedMatIndex = matIndex;
            int capturedVIndex = vIndex;
            Vector3 capturedCenter = center;
            Vector3 capturedV0 = v0, capturedV1 = v1, capturedV2 = v2, capturedV3 = v3;

            // 애니메이션 시작 전에 글자를 완전히 숨겨둠 (scale 0 상태)
            SetCharScale(textInfo, capturedMatIndex, capturedVIndex,
                         capturedCenter, capturedV0, capturedV1, capturedV2, capturedV3,
                         0f);
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);

            // DOVirtual.Float : float 값을 보간하면서 콜백을 매 프레임 호출
            // DOTween의 Ease, Delay, OnComplete 등을 그대로 사용할 수 있음
            Tween t = DOVirtual.Float(0f, 1f, charDuration * 1.1f, (scale) =>
            {
                // ForceMeshUpdate 이후 textInfo 참조는 유효하게 유지됨
                SetCharScale(textInfo, capturedMatIndex, capturedVIndex,
                             capturedCenter, capturedV0, capturedV1, capturedV2, capturedV3,
                             scale);

                // 수정한 내용을 GPU에 반영 — 이 호출 없이는 화면에 아무 변화 없음
                tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
            })
            .SetEase(ease)
            .SetTarget(tmpText);  // DOTween.Kill(tmpText) 로 일괄 중단 가능하게 태그
            textSequence.Insert(i * charDuration, t);  // 시퀀스에 추가
        }
    }

    /// <summary>
    /// 글자 사라지는 효과 구현
    /// </summary>
    private void LineDisappear()
    {
        Debug.Log("DialogLineController: Starting LineDisappear");

        // 텍스트 사라지는 애니메이션 없이 그냥 사라지게 해놓음
        // 효과 주는 것도 괜찮을 듯한데 일단은 간단하게 처리
        disappearSequence.InsertCallback(0f, () => tmpText.transform.gameObject.SetActive(false));

        disappearSequence.Insert(sdm.lineDuration, dialogBar.DOAnchorPosX(0, sdm.lineCreation).SetEase(ease));
        disappearSequence.Insert(barDelay - sdm.speakerCreation,
                                 speakerIcon.DOSizeDelta(Vector2.zero, sdm.speakerCreation).SetEase(ease));
        disappearSequence.Insert(barDelay,
            DOTween.To(() => le.preferredHeight, h => le.preferredHeight = h, 0f, sdm.lineUpDuration)
            .SetEase(ease)
        );
    }

    /// <summary>
    /// 글자 하나의 버텍스 4개를 중심점 기준으로 scale 배율만큼 이동시키는 함수
    /// </summary>
    /// <param name="textInfo">텍스트 정보</param>
    /// <param name="matIndex">재질 인덱스</param>
    /// <param name="vIndex">버텍스 인덱스</param>
    /// <param name="center">중심점</param>
    /// <param name="v0">첫 번째 버텍스</param>
    /// <param name="v1">두 번째 버텍스</param>
    /// <param name="v2">세 번째 버텍스</param>
    /// <param name="v3">네 번째 버텍스</param>
    /// <param name="scale">목표 스케일</param>
    private void SetCharScale(TMP_TextInfo textInfo, int matIndex, int vIndex,
                              Vector3 center, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
                              float scale)
    {
        Debug.Log($"DialogLineController: SetCharScale: {scale}");

        Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

        // 핵심 수식:
        // 새 위치 = 중심 + (원본 위치 - 중심) × scale
        // scale=0 이면 중심으로 완전히 수렴, scale=1 이면 원본 위치와 동일
        verts[vIndex + 0] = center + (v0 - center) * scale;
        verts[vIndex + 1] = center + (v1 - center) * scale;
        verts[vIndex + 2] = center + (v2 - center) * scale;
        verts[vIndex + 3] = center + (v3 - center) * scale;
    }

    /// <summary>
    /// 글자 하나의 알파값을 설정하는 함수
    /// <para>알파값은 0~255 범위의 byte로 입력</para>
    /// <para>지금은 사용하지 않음</para>
    /// </summary>
    /// <param name="textInfo">텍스트 정보</param>
    /// <param name="matIndex">재질 인덱스</param>
    /// <param name="vIndex">버텍스 인덱스</param>
    /// <param name="alpha">알파값</param>
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