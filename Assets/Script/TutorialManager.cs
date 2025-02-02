using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    [Header("캐릭터 이미지")]
    [SerializeField] private Sprite[] CharacterSprite;
    private void Awake()
    {
        // 튜토리얼 시작시 소환/세팅할 것들



    }
    private void Start()
    {
        StartCoroutine(Daehwa1());
    }
    private IEnumerator Daehwa1()
    {
        string Daehwa1_Text1 = "가나다라마바사아자차카타파하가나다라마바사아자차카타파하 가나다라마바사아자차카타파하";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "주인공", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "하파타카차자아사바마라다나가 하파타카차자아사바마라다나가하파타카차자아사바마라다나가";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa1_Text2, true));


        // 다음 함수 호출
    }
}
