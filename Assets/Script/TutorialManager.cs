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
    private PlayerManager playerManager;

    [Header("캐릭터 이미지")]
    [SerializeField] private Sprite[] CharacterSprite;

    private List<float> ChartTimeList = new List<float>();
    private List<int> chartIdxList = new List<int>();

    DatabaseManager databaseManager;
    public int daehwaIndex = 0;
    private bool patternComplete = false;
    public bool isDaehwa = true;
    public static bool isTutorial = false;
    private bool isRollBack = false;

    Text GameDescriptionText;
    public static int[] StrikerNum = { 0, 1, 2, 3, 7, 11, 13 };   // 12까지 존재. // 각 패턴 스트라이커 시작 인덱스

    private void Awake()
    {
        // 튜토리얼 시작시 소환/세팅할 것들
        databaseManager = GameObject.FindObjectOfType<DatabaseManager>();
        isTutorial = true;

        // 공격 설명 텍스트 생성
        GameObject GameDescription = new GameObject("GameDescription");
        GameDescription.transform.SetParent(mainCanvas.transform, false);
        GameDescriptionText = GameDescription.AddComponent<Text>();
        GameDescriptionText.font = DescriptionFont;
        GameDescriptionText.fontSize = 65;
        GameDescriptionText.color = Color.white;
        GameDescriptionText.alignment = TextAnchor.MiddleCenter;
        RectTransform NameTextRect = GameDescription.GetComponent<RectTransform>();
        NameTextRect.anchorMin = new Vector2(0.05f, 0.25f);
        NameTextRect.anchorMax = new Vector2(0.95f, 0.35f);
        NameTextRect.offsetMin = Vector2.zero;
        NameTextRect.offsetMax = Vector2.zero;
        GameDescriptionText.text = "리듬에 맞춰 화면을 탭하여 노란색 공격을 패링하세요.";

        GameDescription.SetActive(true);

        ChartTimeList.AddRange(new float[] {4f, 12f, 24f, 36f, 48f, 60f, 72f  }); ; // 차트 반복 시작 시간   // 
        chartIdxList.AddRange(new int[] { 0, 3, 9, 12, 16, 24, 26 }); // 각 게임의 시작 채보 인덱스  // 25가 마지막
    }
    private void Start()
    {
        playerManager = GameObject.Find("Player(Clone)").GetComponent<PlayerManager>();
        StartCoroutine(Daehwa1());  // 처음 대화 시작
        // count = 0;
        StageManager.isActive = false;  // 게임 비활성화
    }

    // private int count = 0;   // 임시 테스트용
    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        // // Debug.Log($"{currentTime}");
        if (daehwaIndex >= ChartTimeList.Count) 
            return; //모든 대화 끝나면 그냥 RETURN

        if (!isDaehwa)  // 대화 중이 아닌 상황 (게임 중)
        {

            //// Debug.LogWarning($"시간: {currentTime} :{ChartTimeList[daehwaIndex - 1]} ~ {ChartTimeList[daehwaIndex]}, 대화인덱스:{daehwaIndex}");
            if (currentTime >= ChartTimeList[daehwaIndex] + stageManager.musicOffset)   // idx: 게임 끝난 후의 목표 대화 idx
            {   // 한 패턴 지났을 때 패턴 성공했는지 판단
                // // Debug.LogError($"대화인덱스: {currentTime} :{daehwaIndex}");
                // checkComplete1();




                //if (count < 1)  // 임시 조건: 2번 시행 후 진행
                //if (!patternComplete)   // 패턴 성공 못했을 시
                //{
                //    // // Debug.LogError($"if문 안: {count}");
                //    // // Debug.LogError($"스트라이크 활성화 시간: {currentTime}");
                //    // stageManager.ChangeTime(ChartTimeList[daehwaIndex - 1]);
                //    // stageManager.RestartAudio(ChartTimeList[daehwaIndex] - ChartTimeList[daehwaIndex - 1]);
                //    /*
                //    foreach (GameObject striker in strikerManager.strikerList)
                //    {
                        
                //        StrikerController strikerController = striker.GetComponent<StrikerController>();
                //        strikerController.currentNoteIndex = chartIdxList[daehwaIndex - 1];
                //        strikerController.hp = strikerManager.charts[daehwaIndex - 1].notes.Length;
                //        strikerController.hpControl = strikerController.hpBar.transform.GetChild(0);
                //        strikerController.hpControl.transform.localScale = new Vector3(0, 1, 1);
                //    }
                //    */
                //    // strikerManager.ClearStrikers();
                //    // strikerManager.InitStriker(daehwaIndex - 1);    // 현재 패턴의 스트라이커들을 재생성

                //    //foreach (GameObject striker in strikerManager.strikerList)
                //    //{
                //    //    StrikerController strikerController = striker.GetComponent<StrikerController>();
                //    //    // Debug.LogError($"패턴 실패, 재생성: {striker.name} hp: {strikerController.hp}");
                //    //}
                //    // // Debug.LogError($"CURRENT TIME 설정: {currentTime}");
                //    // count++;  // 임시 테스트용
                //    // isRollBack = true;
                //    return;
                //}
                // // Debug.LogError($"오디오 멈춤: {count}, {currentTime}");
                // strikerManager.ClearStrikers();
                // strikerManager.InitStriker(daehwaIndex);
                stageManager.AudioPause();
                isDaehwa = true;
                
            }
            //if (isRollBack)
            //{
                
            //    // // Debug.LogError($"{currentTime},{strikerManager.charts[daehwaIndex - 1].appearTime * (60f / strikerManager.charts[daehwaIndex - 1].bpm) + playerManager.musicOffset}");
            //    if (currentTime >= strikerManager.charts[daehwaIndex - 1].appearTime * (60f / strikerManager.charts[daehwaIndex - 1].bpm) + playerManager.musicOffset)
            //    {
            //        // // Debug.LogError("롤백if문");
            //        strikerManager.strikerList[daehwaIndex - 1].SetActive(true);
            //        isRollBack = false;
            //    }
            //}
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

        GameDescriptionText.text = "리듬에 맞춰 화면을 탭하여 노란색 공격을 패링하세요.";

        isDaehwa = false;

        // 튜토 게임 1
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기 (isDaehwa가 true가 되면 다음 코드 실행됨)
        StageManager.isActive = false;
        // patternComplete = false;
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

        GameDescriptionText.text = "적의 빨간색 공격은 강패링으로 막아내야합니다.\n탭한 상태에서 적절한 방향으로 드래그하면 이어서 강패링을 진행할 수 있습니다.";

        isDaehwa = false;
        // count = 0;

        // 튜토 게임 2 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        // patternComplete = false;
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

        GameDescriptionText.text = "적절한 방향으로 스와이프하여 빠르게 강패링하세요.";

        isDaehwa = false;
        // count = 0;

        // 튜토 게임 3 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        // patternComplete = false;
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

        GameDescriptionText.text = "다른 방향으로 스와이프하여 방향을 바꾸며 강패링할 수 있습니다.";

        isDaehwa = false;

        // 튜토 게임 4 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        // patternComplete = false;
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

        GameDescriptionText.text = "화면을 계속 드래그 하면서 연속으로 방향을 바꿀 수 있습니다.";

        isDaehwa = false;

        // 튜토 게임 5 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        // patternComplete = false;
        StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "그런데..세상에 소리가 없어진 거 아니었어?\n이 메트로놈 소리랑 받아칠 때 나는 소리 같은 건 다 뭐야?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "소리", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "그건 이 지역에 남은 마지막 소리입니다.\n용사님께서 저들을 해방시키지 못하면 그마저도 날아갈 겁니다!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "정령", Daehwa6_Text2, true));

        string Daehwa6_Text3 = "소리도 없는데 우리 대화는 어떻게 하고 있는 건데?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[8], "소리", Daehwa6_Text3, false));

        string Daehwa6_Text4 = "그야 이건 텍스트잖아요.공부 안 한 티 좀 내지 마세요.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa6_Text4, true));

        string Daehwa6_Text5 = "...";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "소리", Daehwa6_Text5, false));

        GameDescriptionText.text = "적의 파란색 공격은 세번째 리듬에 맞춰 홀드하고,\n마지막 순간에 해당 방향으로 스와이프하여 막아내야 합니다.";

        isDaehwa = false;

        // 튜토 게임 6 함수
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // 패턴 성공할 때까지 대기
        StageManager.isActive = false;
        // patternComplete = false;
        StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        //if (!DatabaseManager.isTutorialDone)
        //{
        //    string Daehwa7_Text1 = "튜토리얼은 설정창에서 다시 진행할 수 있어요. 튜토리얼을 훌륭히 해낸다면 새로운 장면을 보실지도..?";
        //    yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa7_Text1, true));
        //}
        //else
        //{
                // 엔딩 애니메이션
        //}
        string Daehwa7_Text2 = "좋아요! 이제 마왕을 잡으러 떠나요!!!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "정령", Daehwa7_Text2, true));

        // Debug.Log("대화 파이널");
        // 엔딩 애니메이션


        // Main Scene 전환
        SceneLinkage.StageLV = 0;
        DatabaseManager.isTutorialDone = true;
        isTutorial = false;
        databaseManager.SaveTutorialDone();
        SceneManager.LoadScene("Main");

        yield break;
    }

    // checkComplete1,2 중에서 1의 방식을 우선적으로 구현. (2로 구현시 많은 버그 예상)

    // 인덱스에 해당하는 스트라이커들을 생성한 상태에서 사용. 그 스트라이커 전부 체력이 0이 되었는지 확인하는 함수
    private void checkComplete1()
    {

        List<GameObject> strikerList_ = strikerManager.strikerList;
        bool isClear = true;

        for (int i = 0; i < strikerList_.Count; i++)
        {
            GameObject striker = strikerList_[i];
            StrikerController strikerController = striker.GetComponent<StrikerController>();

            // Debug.LogError($"CheckComplete: {striker.name} hp: {strikerController.hp}");

            if (strikerController.hp != 0)
            {
                isClear = false; // 클리어 조건 미달
            }
        }
        if (isClear) patternComplete = true;    // 모든 스트라이커들이 체력이 0이면 patternComplete = true
        // Debug.LogError($"{patternComplete}");

    }

    // 모든 스트라이커를 생성한 상태에서 사용. 그 중에서 특정 인덱스에 해당하는 이들이 체력이 모두 0이 되었는지 확인하는 함수 -> 
    private void checkComplete2(int daehwaIndex) { 

        List<GameObject> strikerList_ = strikerManager.strikerList;
        bool isClear = true;

        int startIndex = StrikerNum[daehwaIndex - 1]; // 시작 인덱스
        int endIndex = StrikerNum[daehwaIndex];      // 끝 인덱스

        for (int i = startIndex; i < endIndex; i++)
        {
            if (i >= strikerList_.Count)
            {
                // Debug.LogError($"스트라이커 인덱스 {i}가 유효하지 않습니다.");
                continue;
            }

            GameObject striker = strikerList_[i];
            StrikerController strikerController = striker.GetComponent<StrikerController>();

            // Debug.LogError($"CheckComplete: {striker.name} hp: {strikerController.hp}");

            if (strikerController.hp != 0)
            {
                isClear = false; // 클리어 조건 미달
            }
        }
        if (isClear) patternComplete = true;
        // Debug.LogError($"{patternComplete}");

    }

    public void SkipOn()
    {
        SceneLinkage.StageLV = 0;
        DatabaseManager.isTutorialDone = true;
        isTutorial = false;
        databaseManager.SaveTutorialDone();
        SceneManager.LoadScene("Main");
    }
}
