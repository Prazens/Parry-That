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

    [Header("�⺻ UI����")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font DescriptionFont;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private StrikerManager strikerManager;
    // [SerializeField] private StageManager strikerController;
    [SerializeField] private ScoreManager ScoreManager;

    [Header("ĳ���� �̹���")]
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
        // Ʃ�丮�� ���۽� ��ȯ/������ �͵�
        databaseManager = GameObject.FindObjectOfType<DatabaseManager>();

        // ���� ���� �ؽ�Ʈ ����
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
        GameDescriptionText.text = "[Ÿ�ֿ̹� ���� ���� �Ͽ� ������ �޾�ġ����!]";

        GameDescription.SetActive(true);

        ChartTimeList.AddRange(new float[] { 4f, 12f, 20f, 28f, 36f, 44f, 52f }); // ��Ʈ �ݺ� ���� �ð�
        chartIdxList.AddRange(new int[] { 0, 3, 9, 12, 16, 24 }); // �� ������ ���� ä�� �ε���  // 25�� ������
    }
    private void Start()
    {
        StartCoroutine(Daehwa1());  // ó�� ��ȭ ����
        StageManager.isActive = false;  // ���� ��Ȱ��ȭ
    }
    private int count = 0;   // �ӽ� �׽�Ʈ��
    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        // Debug.Log($"{currentTime}");
        if (daehwaIndex >= ChartTimeList.Count) return; //��� ��ȭ ������ �׳� RETURN
        if (!isDaehwa)  // ��ȭ ���� �ƴ� ��Ȳ (���� ��)
        {
            if (currentTime >= ChartTimeList[daehwaIndex])   // idx��° ��ȭ -> idx��° ���� -> idx+1 ��° ��ȭ
            {   // �� ���� ������ �� ���� �����ߴ��� �Ǵ�
                // Debug.LogError($"��ȭ�ε���: {daehwaIndex}");
                checkComplete();
                // if (!patternComplete)   // ���� ���� ������ ��
                if (count < 1)  // �ӽ� ����: 2�� ���� �� ����
                {
                    Debug.LogError($"if�� ��: {count}");
                    stageManager.ChangeTime(ChartTimeList[daehwaIndex - 1]);
                    stageManager.RestartAudio(8f);  // ������ ������ �ð� �ٸ���. ���� �ʿ� // �غ� �ð� + ���� �ð� -> �غ�ð��� �ð��� �ǵ����� ä�� �ε��� �ȳִ� ������� ��� ����, �۵����� �� �𸣰���
                    foreach (GameObject striker in strikerManager.strikerList)
                    {
                        StrikerController strikerController = striker.GetComponent<StrikerController>();
                        strikerController.currentNoteIndex = chartIdxList[daehwaIndex - 1];
                    }
                    Debug.LogError($"CURRENT TIME ����: {currentTime}");
                    count++;  // �ӽ� �׽�Ʈ��
                    return;
                }
                Debug.LogError($"����� ����: {count}");
                stageManager.AudioPause();
                isDaehwa = true;
                
            }
        }
    }
    private IEnumerator Daehwa1()
    {
        isDaehwa = true;

        string Daehwa1_Text1 = "��� �ִ��� ����!!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[9], "�Ҹ�", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "�����ϼ���! ���� ������ ���ָ԰Ÿ��� �� �ǽʴϴ�!\n���⸦ �帱 �״ϱ�, �켱 ��ì�̵��� ���� �������� �Ͻ���!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text2, true));

        string Daehwa1_Text3 = "�׷� �ð��� ����! �� ������ ���ƴٰ�!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[9], "�Ҹ�", Daehwa1_Text3, false));

        string Daehwa1_Text4 = "���� �����Ͻʽÿ�! ������ ���ε� �� �ϼ��ݽ��ϱ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[2], "����", Daehwa1_Text4, true));

        string Daehwa1_Text5 = "(���±� �ѵ� ��� ���ڳ�.)";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "�Ҹ�", Daehwa1_Text5, false));

        string Daehwa1_Text6 = "�� �༮���� �� ���� �Ĺ迴�µ�, ������ Ÿ������ ���Ƚ��ϴ�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa1_Text6, true));

        string Daehwa1_Text7 = "������ ���� ����ǥ�� ���뿡 ���� ������ �޾�ġ����!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text7, true));

        isDaehwa = false;

        // Ʃ�� ���� 1
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa2());
    }

    private IEnumerator Daehwa2()
    {
        string Daehwa2_Text1 = "���ϼ̽��ϴ�! ��Ȯ�� Ÿ�ֿ̹� �޾�ġ�� ������ ġ���� �� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa2_Text1, true));

        string Daehwa2_Text2 = "������ �޾�ġ�� ���װ� ��ġ�°� �ƴϾ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "�Ҹ�", Daehwa2_Text2, false));

        string Daehwa2_Text3 = "�� �¾ƾ� ������ ������ �������?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa2_Text3, true));

        string Daehwa2_Text4 = "�׷���..?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[6], "�Ҹ�", Daehwa2_Text4, false));

        string Daehwa2_Text5 = "�ƹ�ư, ���� �� ���� ���ݵ� ���� ���� ���Ժ��� �����ϼ���!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa2_Text5, true));

        isDaehwa = false;
        count = 0;

        // Ʃ�� ���� 2 �Լ�
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa3());
    }

    private IEnumerator Daehwa3()
    {
        string Daehwa3_Text1 = "�� ģ������ ����޴� ����� ���Ѻ��� �ϴٴ�! �󸶳� ������ �ֵ��̾��µ�!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[2], "����", Daehwa3_Text1, true));

        string Daehwa3_Text2 = "���� ������� �������� �� ä�� ���� �� ������ �� ��� ������!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "����", Daehwa3_Text2, true));

        string Daehwa3_Text3 = "����? �ٰ��� ������ ���� �� �Ƴ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "�Ҹ�", Daehwa3_Text3, false));

        string Daehwa3_Text4 = "������ ���ٰ� �� �Ƹ�带 ���� ���Ƚ��ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "����", Daehwa3_Text4, true));

        string Daehwa3_Text5 = "...�� �� ���� ���� ����.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[5], "�Ҹ�", Daehwa3_Text5, false));

        isDaehwa = false;
        count = 0;

        // Ʃ�� ���� 3 �Լ�
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa4());
    }

    private IEnumerator Daehwa4()
    {
        string Daehwa4_Text1 = "�Ǹ��Ͻʴϴ�! ����� �ε�� ������ ���� ����鵵 �� ����� �� �̴ϴ�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa4_Text1, true));

        string Daehwa4_Text2 = "�׷� ��ġ�� �ǰ�?!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "�Ҹ�", Daehwa4_Text2, false));

        string Daehwa4_Text3 = "..�ƴ�. ���� ���� ������ �ϴ� ����� ���� �̴ϴ�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa4_Text3, true));

        isDaehwa = false;

        // Ʃ�� ���� 4 �Լ�
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa5());
    }

    private IEnumerator Daehwa5()
    {
        string Daehwa5_Text1 = "��׵� ��¥ ���Ǿ��� �����. ��ġ?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "�Ҹ�", Daehwa5_Text1, false));

        string Daehwa5_Text2 = "������ �����ڸ�, �����ٴ� ���鿩�� �����ε� �� �����ϴ�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[3], "����", Daehwa5_Text2, true));

        string Daehwa5_Text3 = "..�̾�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "�Ҹ�", Daehwa5_Text3, false));

        isDaehwa = false;

        // Ʃ�� ���� 5 �Լ�
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "�׷���..���� �Ҹ��� ������ �� �ƴϾ���?\n�� ��Ʈ�γ� �Ҹ��� �޾�ĥ �� ���� �Ҹ� ���� �� �� ����?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "�Ҹ�", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "�װ� �� ������ ���� ������ �Ҹ��Դϴ�.\n���Բ��� ������ �ع��Ű�� ���ϸ� �׸����� ���ư� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa6_Text2, true));

        string Daehwa4_Text3 = "�Ҹ��� ���µ� �츮 ��ȭ�� ��� �ϰ� �ִ� �ǵ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[8], "�Ҹ�", Daehwa4_Text3, false));

        string Daehwa4_Text4 = "�׾� �̰� �ؽ�Ʈ�ݾƿ�.���� �� �� Ƽ �� ���� ������.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa4_Text4, true));

        string Daehwa4_Text5 = "...";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[7], "�Ҹ�", Daehwa4_Text5, false));

        isDaehwa = false;

        // Ʃ�� ���� 6 �Լ�
        daehwaIndex += 1;
        StageManager.isActive = true;
        stageManager.AudioUnPause();
        yield return new WaitUntil(() => isDaehwa);  // ���� ������ ������ ���
        StageManager.isActive = false;
        patternComplete = false;
        StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        //string Daehwa7_Text1 = "���߿�!! ���� ���ϼ̽��ϴ�!! ���� ��� ���� ���� ������ ���Ҵ� ����Դϴ�!";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text1, false));

        //string Daehwa7_Text2 = "...��� ��Ű���?";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text2, false));

        //string Daehwa7_Text3 = "��! ����̵��̴�!!! ��� ��¥ �Ϳ���!";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text3, false));

        //string Daehwa7_Text2 = "����. ����� ����� �ܽ����� õ���ε�..";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text2, false));

        //string Daehwa7_Text2 = "...��� ��Ű���?";
        //yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text2, false));
        
        
        // ���� �ִϸ��̼�


        // Main Scene ��ȯ
        DatabaseManager.isTutorialDone = true;
        databaseManager.SaveTutorialDone();
        SceneManager.LoadScene("Main");

        yield break;
    }

    

    private void checkComplete()
    {
        //stiker clear���� stiker destroy�� �ǰ� �� �����ؾ���
        List<GameObject> strikerList_ = strikerManager.strikerList;
        if (strikerList_.Count == 0) patternComplete = true;


        //or hp = 0������ Ȯ�� �ϴ°ŷ� �ٲ㵵 ��

        // StrikerController? strikerController = null;
        // foreach (GameObject striker in strikerList_)
        // {
        //     strikerController = striker.GetComponent<StrikerController>();
        //     if(strikerController.hp != 0) patternComplete = false; 
        // }
    }


}
