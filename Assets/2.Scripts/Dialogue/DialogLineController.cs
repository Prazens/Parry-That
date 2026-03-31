using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogLineController : MonoBehaviour
{
    [SerializeField]
    private GameObject speakerObject; // 대화하는 캐릭터의 GameObject
    [SerializeField]
    private TMP_Text dialogText; // 대사 텍스트 TMP_Text 컴포넌트

    public void InitLine(string text, Sprite speakerSprite)
    {
        dialogText.text = text;
        dialogText.maxVisibleCharacters = 0; // 글자 하나씩 보이도록 초기화
        speakerObject.GetComponent<SpriteRenderer>().sprite = speakerSprite;
    }

    public void typeChar()
    {
        dialogText.maxVisibleCharacters++;
    }

    public void DisappearLine()
    {
        // 대사 사라지는 효과 구현 (예: 페이드 아웃)
        // StartCoroutine(FadeOutCoroutine());
    }
}
