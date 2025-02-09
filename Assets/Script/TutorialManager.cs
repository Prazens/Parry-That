using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    [Header("기본 UI참조")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font DescriptionFont;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private StrikerManager strikerManager;
    // [SerializeField] private StageManager strikerController;
    [SerializeField] private ScoreManager ScoreManager;

    [Header("캐릭터 이미지")]
    [SerializeField] private Sprite[] CharacterSprite;

    private List<float> ChartTimeList = new List<float>();
    private List<int> chartIdxList = new List<int>();

    DatabaseManager databaseManager;
    private int daehwaIndex = 0;
    private bool patternComplete = false;
    private bool isDaehwa = true;
    public static bool isTutorial = false;
    private void Awake()
    {
        // 튜토리얼 시작시 소환/세팅할 것들
        databaseManager = GameObject.FindObjectOfType<DatabaseManager>();

        // 공격 설명 텍스트 생성
        GameObject GameDescription = new GameObject("GameDescription");
        GameDescription.transform.SetParent(mainCanvas.transform, false);
        Text GameDescriptionText = GameDescription.AddComponent<Text>();
        GameDescriptionText.font = DescriptionFont;
        GameDescriptionText.fontSize = 55 * (Screen.width / 1080);
        GameDescriptionText.color = Color.white;
        GameDescriptionText.alignment = TextAnchor.MiddleCenter;
        RectTransform NameTextRect = GameDescription.GetComponent<RectTransform>();
        NameTextRect.anchorMin = new Vector2(0.05f, 0.25f);
        NameTextRect.anchorMax = new Vector2(0.95f, 0.35f);
        NameTextRect.offsetMin = Vector2.zero;
        NameTextRect.offsetMax = Vector2.zero;
        GameDescriptionText.text = "[타이밍에 맞춰 탭을 하여 공격을 받아치세요!]";

        GameDescription.SetActive(true);

        ChartTimeList.AddRange(new float[] { 4f, 12f, 20f, 28f, 36f, 44f, 52f }); // 차트 반복 시작 시간
        chartIdxList.AddRange(new int[] { 0, 3, 9, 12, 16, 24 }); // 각 게임의 시작 채보 인덱스  // 25가 마지막
    }
    private void Start()
    {
        StartCoroutine(Daehwa1());  // 처음 대화 시작
        StageManager.isActive = false;  // 게임 비활성화
    }
    private int count = 0;   // 임시 테스트용
    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        // Debug.Log($"{currentTime}");
        if (daehwaIndex >= ChartTimeList.Count) return; //모든 대화 끝나면 그냥 RETURN
        if (!isDaehwa)  // 대화 중이 아닌 상황 (게임 중)
        {
            if (currentTime >= ChartTimeList[daehwaIndex])   // idx번째 대화 -> idx번째 게임 -> idx+1 번째 대화
            {   // 한 패턴 지났을 때 패턴 성공했는지 판단
                // Debug.LogError($"대화인덱스: {daehwaIndex}");
                checkComplete();
                // if (!patternComplete)   // 패턴 성공 못했을 시
                if (count < 1)  // 임시 조건: 2번 시행 후 진행
                {
                    Debug.LogError($"if문 안: {count}");
                    stageManager.ChangeTime(ChartTimeList[daehwaIndex - 1]);
                    stageManager.RestartAudio(8f);  // 마지막 게임은 시간 다른듯. 설정 필요 // 준비 시간 + 패턴 시간 -> 준비시간은 시간만 되돌리고 채보 인덱스 안넣는 방식으로 대기 구현, 작동할진 잘 모르겠음
                    foreach (GameObject striker in strikerManager.strikerList)
                    {
                        StrikerController strikerController = striker.GetComponent<StrikerController>();
                        strikerController.currentNoteIndex = chartIdxList[daehwaIndex - 1];
                    }
                    Debug.LogError($"CURRENT TIME 설정: {currentTime}");
                    count++;  // 임시 테스트용
                    return;
                }
                Debug.LogError($"오디오 멈춤: {count}");
                stageManager.AudioPause();
                isDaehwa = true;
                
            }
        }
    }
    private IEnumerator Daehwa1()
    {
        isDaehwa = true;

        string Daehwa1_Text1 = "어디 있느냐 마왕!!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[9], "소리", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "진정하세요! 지금 용사님은 한주먹거리도 안 되십니다!\n무기를 드릴 테니까, 우선 잔챙이들을 상대로 연습부터 하시죠!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text2, true));

        string Daehwa1_Text3 = "그럴 시간이 없어! 내 시험을 망쳤다고!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[9], "소리", Daehwa1_Text3, false));

        string Daehwa1_Text4 = "제발 진정하십시오! 어차피 공부도 안 하셨잖습니까!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[2], "정령", Daehwa1_Text4, true));

        string Daehwa1_Text5 = "(…맞긴 한데 기분 나쁘네.)";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "소리", Daehwa1_Text5, false));

        string Daehwa1_Text6 = "저 녀석들은 제 직장 후배였는데, 마왕이 타락시켜 버렸습니다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "정령", Daehwa1_Text6, true));

        string Daehwa1_Text7 = "물범이 띄우는 느낌표의 리듬에 맞춰 공격을 받아치세요!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa1_Text7, true));

        isDaehwa = false;

        // 튜토 게임 1
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa2());
    }

    private IEnumerator Daehwa2()
    {
        string Daehwa2_Text1 = "잘하셨습니다! 정확한 타이밍에 받아치면 저들이 치유가 될 겁니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "정령", Daehwa2_Text1, true));

        string Daehwa2_Text2 = "공격을 받아치면 쟤네가 다치는거 아니야?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "소리", Daehwa2_Text2, false));

        string Daehwa2_Text3 = "좀 맞아야 정신을 차리지 않을까요?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa2_Text3, true));

        string Daehwa2_Text4 = "그런가..?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[6], "소리", Daehwa2_Text4, false));

        string Daehwa2_Text5 = "아무튼, 이제 더 강한 공격도 같이 오니 용사님부터 걱정하세요!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa2_Text5, true));

        isDaehwa = false;
        count = 0;

        // 튜토 게임 2 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa3());
    }

    private IEnumerator Daehwa3()
    {
        string Daehwa3_Text1 = "저 친구들이 고통받는 모습을 지켜봐야 하다니! 얼마나 성실한 애들이었는데!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[2], "정령", Daehwa3_Text1, true));

        string Daehwa3_Text2 = "가끔 탕비실을 생선으로 꽉 채워 놓는 건 마음에 안 들긴 했지만!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "정령", Daehwa3_Text2, true));

        string Daehwa3_Text3 = "생선? 다같이 먹으면 좋은 거 아냐?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "소리", Daehwa3_Text3, false));

        string Daehwa3_Text4 = "공간이 없다고 제 아몬드를 갖다 버렸습니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "정령", Daehwa3_Text4, true));

        string Daehwa3_Text5 = "...좀 더 때릴 맛이 나네.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[5], "소리", Daehwa3_Text5, false));

        isDaehwa = false;
        count = 0;

        // 튜토 게임 3 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa4());
    }

    private IEnumerator Daehwa4()
    {
        string Daehwa4_Text1 = "훌륭하십니다! 제대로 두들겨 팼으니 이제 저놈들도 좀 쉬어야 할 겁니다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa4_Text1, true));

        string Daehwa4_Text2 = "그럼 해치운 건가?!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "소리", Daehwa4_Text2, false));

        string Daehwa4_Text3 = "..아뇨. 이제 근접 공격을 하는 놈들이 나올 겁니다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "정령", Daehwa4_Text3, true));

        isDaehwa = false;

        // 튜토 게임 4 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa5());
    }

    private IEnumerator Daehwa5()
    {
        string Daehwa5_Text1 = "얘네들 진짜 성의없게 생겼다. 그치?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "소리", Daehwa5_Text1, false));

        string Daehwa5_Text2 = "솔직히 말하자면, 저보다는 공들여서 디자인된 것 같습니다.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "정령", Daehwa5_Text2, true));

        string Daehwa5_Text3 = "..미안.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "소리", Daehwa5_Text3, false));

        isDaehwa = false;

        // 튜토 게임 5 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "그런데..세상에 소리가 없어진 거 아니었어?\n이 메트로놈 소리랑 받아칠 때 나는 소리 같은 건 다 뭐야?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "소리", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "그건 이 지역에 남은 마지막 소리입니다.\n용사님께서 저들을 해방시키지 못하면 그마저도 날아갈 겁니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "정령", Daehwa6_Text2, true));

        string Daehwa4_Text3 = "소리도 없는데 우리 대화는 어떻게 하고 있는 건데?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[8], "소리", Daehwa4_Text3, false));

        string Daehwa4_Text4 = "그야 이건 텍스트잖아요.공부 안 한 티 좀 내지 마세요.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa4_Text4, true));

        string Daehwa4_Text5 = "...";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "소리", Daehwa4_Text5, false));

        isDaehwa = false;

        // 튜토 게임 6 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        //string Daehwa7_Text1 = "끼야오!! 정말 잘하셨습니다!! 제가 사람 보는 눈이 참으로 좋았던 모양입니다!";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "주인공", Daehwa7_Text1, false));

        //string Daehwa7_Text2 = "...듣고 계신가요?";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa7_Text2, false));

        //string Daehwa7_Text3 = "와! 고양이들이다!!! 얘네 진짜 귀엽다!";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa7_Text3, false));

        //string Daehwa7_Text2 = "용사님. 고양이 놈들은 햄스터의 천적인데..";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa7_Text2, false));

        //string Daehwa7_Text2 = "...듣고 계신가요?";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "부인공", Daehwa7_Text2, false));
        
        
        // 엔딩 애니메이션


        // Main Scene 전환
        DatabaseManager.isTutorialDone = true;
        databaseManager.SaveTutorialDone();
        SceneManager.LoadScene("Main");

        yield break;
    }

    

    private void checkComplete()
    {
        //stiker clear이후 stiker destroy가 되게 끔 구현해야함
        List<GameObject> strikerList_ = strikerManager.strikerList;
        if (strikerList_.Count == 0) patternComplete = true;


        //or hp = 0인지를 확인 하는거로 바꿔도 됨

        // StrikerController? strikerController = null;
        // foreach (GameObject striker in strikerList_)
        // {
        //     strikerController = striker.GetComponent<StrikerController>();
        //     if(strikerController.hp != 0) patternComplete = false; 
        // }
    }


}
