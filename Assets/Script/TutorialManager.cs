using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    [Header("기본 UI참조")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font DescriptionFont;

    [Header("캐릭터 이미지")]
    [SerializeField] private Sprite[] CharacterSprite;

    private void Awake()
    {
        // 튜토리얼 시작시 소환/세팅할 것들

        // 공격 설명 텍스트 생성
        GameObject GameDescription = new GameObject("GameDescription");
        GameDescription.transform.SetParent(mainCanvas.transform, false);
        Text GameDescriptionText = GameDescription.AddComponent<Text>();
        GameDescriptionText.font = DescriptionFont;
        GameDescriptionText.fontSize = 40;
        GameDescriptionText.color = Color.white;
        GameDescriptionText.alignment = TextAnchor.MiddleCenter;
        RectTransform NameTextRect = GameDescription.GetComponent<RectTransform>();
        NameTextRect.anchorMin = new Vector2(0.05f, 0.25f);
        NameTextRect.anchorMax = new Vector2(0.95f, 0.35f);
        NameTextRect.offsetMin = Vector2.zero;
        NameTextRect.offsetMax = Vector2.zero;
        GameDescriptionText.text = "[타이밍에 맞춰 탭을 하여 공격을 받아치세요!]";

        GameDescription.SetActive(true);
    }
    private void Start()
    {
        StartCoroutine(Daehwa1());
    }
    private IEnumerator Daehwa1()
    {
        string Daehwa1_Text1 = "어디 있느냐 마왕!!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "진정하세요! 지금 용사님은 한주먹거리도 안 되십니다!\n무기를 드릴 테니까, 우선 잔챙이들을 상대로 연습부터 하시죠!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text2, true));

        string Daehwa1_Text3 = "그럴 시간이 없어! 내 시험을 망쳤다고!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text3, false));

        string Daehwa1_Text4 = "제발 진정하십시오!어차피 공부도 안 하셨잖습니까!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text4, true));

        string Daehwa1_Text5 = "(…맞긴 한데 기분 나쁘네.)";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text5, false));

        string Daehwa1_Text6 = "저 녀석들은 제 직장 후배였는데, 마왕이 타락시켜 버렸습니다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text6, true));

        string Daehwa1_Text7 = "물범이 띄우는 느낌표의 리듬에 맞춰 공격을 받아치세요!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text7, true));



        /*
            string Daehwa1_Text1 = "용사님, 용사님! 저기 타락한 정령이 있어요.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정?령", Daehwa1_Text1, true));

            string Daehwa1_Text2 = "아, 이것 좀 놔봐. 난 마왕 죽이러 가야한다니까?";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text2, true));

            string Daehwa1_Text3 = "(제일 약한 드럼몬한테도 지게 생긴 놈이 뭐래...)";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정?령", Daehwa1_Text3, true));

            string Daehwa1_Text4 = "제일 약한 드럼몬한테도 지게 생긴 놈이 뻘소리 한다는 눈빛으로 보지 마라.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text4, true));

            string Daehwa1_Text5 = "힉!!! 어떻게 알았지?";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정?령", Daehwa1_Text5, true));

            string Daehwa1_Text6 = "제일 약한 드럼몬한테도 지게 생긴 놈이 뻘소리 한다는 눈빛으로 보지 마라.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa1_Text4, true));

            string Daehwa1_Text7 = "아니 근데 용사님이 허약한건 팩트잖아...요.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정?령", Daehwa1_Text6, true));

            string Daehwa1_Text8 = "";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정?령", Daehwa1_Text8, true));
        */

        // 튜토 게임 1 함수
        // 

        StartCoroutine(Daehwa2());
    }

    private IEnumerator Daehwa2()
    {
        string Daehwa2_Text1 = "잘하셨습니다! 정확한 타이밍에 받아치면 저들에게 역으로 피해를 줄 겁니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa2_Text1, true));

        string Daehwa2_Text2 = "이제 더 강한 공격도 같이 오니 조심하십시오!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa2_Text2, true));

        // 튜토 게임 2 함수
        // 
        
        StartCoroutine(Daehwa3());
    }

    private IEnumerator Daehwa3()
    {
        string Daehwa3_Text1 = "저 친구들이 고통받는 모습을 지켜봐야 하다니! 얼마나 성실한 애들이었는데!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa3_Text1, true));

        string Daehwa3_Text2 = "가끔 탕비실을 생선으로 꽉 채워 놓는 건 마음에 안 들긴 했지만!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa3_Text2, true));

        string Daehwa3_Text3 = "생선? 다같이 먹으면 좋은 거 아냐?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa3_Text3, false));

        string Daehwa3_Text4 = "공간이 없다고 제 아몬드를 갖다 버렸습니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa3_Text4, true));

        string Daehwa3_Text5 = "...좀 더 때릴 맛이 나네.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa3_Text5, false));

        // 튜토 게임 3 함수
        // 
        StartCoroutine(Daehwa4());
    }

    private IEnumerator Daehwa4()
    {
        string Daehwa4_Text1 = "그런데..세상에 소리가 없어진 거 아니었어?\n이 메트로놈 소리랑 받아칠 때 나는 소리 같은 건 다 뭐야?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa4_Text1, false));

        string Daehwa4_Text2 = "그건 이 지역에 남은 마지막 소리입니다.\n용사님께서 저들을 해방시키지 못하면 그마저도 날아갈 겁니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa4_Text2, true));

        string Daehwa4_Text3 = "소리도 없는데 우리 대화는 어떻게 하고 있는 건데?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "소리", Daehwa4_Text3, false));

        string Daehwa4_Text4 = "그야 이건 텍스트잖아요.공부 안 한 티 좀 내지 마세요.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa4_Text4, true));

        // 튜토 게임 4 함수
        // 
        StartCoroutine(Daehwa5());
    }

    private IEnumerator Daehwa5()
    {
        string Daehwa5_Text1 = "내가 그린 기린 그림은 잘 그린 기린 그림이고 니가 그린 기린 그림은 잘 못그린 기린 그림이다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "주인공", Daehwa5_Text1, false));

        string Daehwa5_Text2 = "에레레렐레 레레렐렐";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa5_Text2, false));

        // 튜토 게임 5 함수
        // 

        StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "내가 그린 기린 그림은 잘 그린 기린 그림이고 니가 그린 기린 그림은 잘 못그린 기린 그림이다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "주인공", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "에레레렐레 레레렐렐";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa6_Text2, false));

        // 튜토 게임 6 함수
        // 

        StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        string Daehwa7_Text1 = "내가 그린 기린 그림은 잘 그린 기린 그림이고 니가 그린 기린 그림은 잘 못그린 기린 그림이다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "주인공", Daehwa7_Text1, false));

        string Daehwa7_Text2 = "에레레렐레 레레렐렐";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa7_Text2, false));

        // Main Scene 전환
    }


}
