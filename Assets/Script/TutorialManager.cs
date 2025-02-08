using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
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

    [Header("ĳ���� �̹���")]
    [SerializeField] private Sprite[] CharacterSprite;
    [SerializeField] private List<float> daewaTimeList;

    DatabaseManager databaseManager;
    private int daewaIndex = 0;
    private bool patternComplete = true;

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
    }
    private void Start()
    {

    }
    //stage의 currentime 불러와서 일정 시간이 될 경우 대화창이 나타나게끔 합니다. 대화 종료시 isActive로 재개됩니다. 
    //patternComplete bool로 패턴을 반복하는지 확인합니다
    
    private void Update()
    {
        float currentTime = StageManager.Instance.currentTime;
        if(daewaIndex >= daewaTimeList.Count) return;
        if (currentTime >= daewaTimeList[daewaIndex]) 
            {
                checkComplete();
                if(!patternComplete)
                {
                    stageManager.ChangeTime(daewaTimeList[daewaIndex -1]);
                    return;
                }
                //current time 정지, stage정지
                StageManager.Instance.isActive = false;
                //대화 이벤트 가져오기
                switch(daewaIndex)
                {
                    case 0:
                        StartCoroutine(Daehwa1());
                        break;
                    case 1:
                        StartCoroutine(Daehwa2());
                        break;
                    case 2:
                        StartCoroutine(Daehwa3());
                        break;
                    case 3:
                        StartCoroutine(Daehwa4());
                        break;
                    case 4:
                        StartCoroutine(Daehwa5());
                        break;
                    case 5:
                        StartCoroutine(Daehwa6());
                        break;
                    case 6:
                        StartCoroutine(Daehwa_Final());
                        break;
                    default:
                        break;
                }
                //stage 재개
                StageManager.Instance.isActive = true;
                //대화 index 증가
                daewaIndex += 1;
            }
    }
    private IEnumerator Daehwa1()
    {
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



        

        // Ʃ�� ���� 1 �Լ�
        // 

        //StartCoroutine(Daehwa2());
    }

    private IEnumerator Daehwa2()
    {
        string Daehwa2_Text1 = "���ϼ̽��ϴ�! ��Ȯ�� Ÿ�ֿ̹� �޾�ġ�� ���鿡�� ������ ���ظ� �� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa2_Text1, true));

        string Daehwa2_Text2 = "���� �� ���� ���ݵ� ���� ���� �����Ͻʽÿ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa2_Text2, true));

        // Ʃ�� ���� 2 �Լ�
        // 
        
        //StartCoroutine(Daehwa3());
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

        // Ʃ�� ���� 3 �Լ�
        // 
        //StartCoroutine(Daehwa4());
    }

    private IEnumerator Daehwa4()
    {
        string Daehwa4_Text1 = "�׷���..���� �Ҹ��� ������ �� �ƴϾ���?\n�� ��Ʈ�γ� �Ҹ��� �޾�ĥ �� ���� �Ҹ� ���� �� �� ����?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[4], "�Ҹ�", Daehwa4_Text1, false));

        string Daehwa4_Text2 = "�װ� �� ������ ���� ������ �Ҹ��Դϴ�.\n���Բ��� ������ �ع��Ű�� ���ϸ� �׸����� ���ư� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[1], "����", Daehwa4_Text2, true));

        string Daehwa4_Text3 = "�Ҹ��� ���µ� �츮 ��ȭ�� ��� �ϰ� �ִ� �ǵ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[8], "�Ҹ�", Daehwa4_Text3, false));

        string Daehwa4_Text4 = "�׾� �̰� �ؽ�Ʈ�ݾƿ�.���� �� �� Ƽ �� ���� ������.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa4_Text4, true));

        // Ʃ�� ���� 4 �Լ�
        // 
        //StartCoroutine(Daehwa5());
    }

    private IEnumerator Daehwa5()
    {
        string Daehwa5_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa5_Text1, false));

        string Daehwa5_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa5_Text2, false));

        // Ʃ�� ���� 5 �Լ�
        // 

        //StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa6_Text2, false));

        // Ʃ�� ���� 6 �Լ�
        // 

        //StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        string Daehwa7_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text1, false));

        string Daehwa7_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text2, false));

        // Main Scene ��ȯ
        DatabaseManager.isTutorialDone = true;
        databaseManager.SaveTutorialDone();
        SceneManager.LoadScene("Main");
    }
    private void checkComplete()
    {
        //stiker clear이후 stiker destroy가 되게 끔 구현해야함
        List<GameObject> strikerList_ = strikerManager.strikerList;
        if(strikerList_.Count == 0) patternComplete = true;


        //or hp = 0인지를 확인 하는거로 바꿔도 됨

        // StrikerController? strikerController = null;
        // foreach (GameObject striker in strikerList_)
        // {
        //     strikerController = striker.GetComponent<StrikerController>();
        //     if(strikerController.hp != 0) patternComplete = false; 
        // }
    }

}
