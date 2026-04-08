using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class StageTitleVert : MonoBehaviour
{
    private TMP_Text textComponent;

    [Header("사다리꼴 왜곡 비율")]
    [Range(0f, 2f)] public float leftSideHeight = 1.2f;
    [Range(0f, 2f)] public float rightSideHeight = 0.5f;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    // 애니메이션 스크립트에서 글자가 바뀔 때 딱 한 번 호출할 함수
    public void ApplyWarp()
    {
        textComponent.ForceMeshUpdate(); // 강제로 한 번 최신화
        TMP_TextInfo textInfo = textComponent.textInfo;

        if (textInfo.characterCount == 0) return;

        float minX = textComponent.textBounds.min.x;
        float maxX = textComponent.textBounds.max.x;
        float width = maxX - minX;

        if (width <= 0) return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = vertices[vertexIndex + j];
                float t = (v.x - minX) / width;
                v.y *= Mathf.Lerp(leftSideHeight, rightSideHeight, t);
                vertices[vertexIndex + j] = v;
            }
        }
        
        // 찌그러트린 뼈대 적용 끝! 이후엔 절대 건드리지 않음.
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}