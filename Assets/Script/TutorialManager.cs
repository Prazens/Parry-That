using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    [Header("�⺻ UI����")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Font DescriptionFont;

    [Header("ĳ���� �̹���")]
    [SerializeField] private Sprite[] CharacterSprite;

    private void Awake()
    {
        // Ʃ�丮�� ���۽� ��ȯ/������ �͵�

        // ���� ���� �ؽ�Ʈ ����
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
        GameDescriptionText.text = "[Ÿ�ֿ̹� ���� ���� �Ͽ� ������ �޾�ġ����!]";

        GameDescription.SetActive(true);
    }
    private void Start()
    {
        StartCoroutine(Daehwa1());
    }
    private IEnumerator Daehwa1()
    {
        string Daehwa1_Text1 = "��� �ִ��� ����!!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "�����ϼ���! ���� ������ ���ָ԰Ÿ��� �� �ǽʴϴ�!\n���⸦ �帱 �״ϱ�, �켱 ��ì�̵��� ���� �������� �Ͻ���!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text2, true));

        string Daehwa1_Text3 = "�׷� �ð��� ����! �� ������ ���ƴٰ�!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text3, false));

        string Daehwa1_Text4 = "���� �����Ͻʽÿ�!������ ���ε� �� �ϼ��ݽ��ϱ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text4, true));

        string Daehwa1_Text5 = "(���±� �ѵ� ��� ���ڳ�.)";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text5, false));

        string Daehwa1_Text6 = "�� �༮���� �� ���� �Ĺ迴�µ�, ������ Ÿ������ ���Ƚ��ϴ�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text6, true));

        string Daehwa1_Text7 = "������ ���� ����ǥ�� ���뿡 ���� ������ �޾�ġ����!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa1_Text7, true));



        /*
            string Daehwa1_Text1 = "����, ����! ���� Ÿ���� ������ �־��.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "��?��", Daehwa1_Text1, true));

            string Daehwa1_Text2 = "��, �̰� �� ����. �� ���� ���̷� �����Ѵٴϱ�?";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text2, true));

            string Daehwa1_Text3 = "(���� ���� �巳�����׵� ���� ���� ���� ����...)";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "��?��", Daehwa1_Text3, true));

            string Daehwa1_Text4 = "���� ���� �巳�����׵� ���� ���� ���� ���Ҹ� �Ѵٴ� �������� ���� ����.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text4, true));

            string Daehwa1_Text5 = "��!!! ��� �˾���?";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "��?��", Daehwa1_Text5, true));

            string Daehwa1_Text6 = "���� ���� �巳�����׵� ���� ���� ���� ���Ҹ� �Ѵٴ� �������� ���� ����.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa1_Text4, true));

            string Daehwa1_Text7 = "�ƴ� �ٵ� ������ ����Ѱ� ��Ʈ�ݾ�...��.";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "��?��", Daehwa1_Text6, true));

            string Daehwa1_Text8 = "";
            yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "��?��", Daehwa1_Text8, true));
        */

        // Ʃ�� ���� 1 �Լ�
        // 

        StartCoroutine(Daehwa2());
    }

    private IEnumerator Daehwa2()
    {
        string Daehwa2_Text1 = "���ϼ̽��ϴ�! ��Ȯ�� Ÿ�ֿ̹� �޾�ġ�� ���鿡�� ������ ���ظ� �� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa2_Text1, true));

        string Daehwa2_Text2 = "���� �� ���� ���ݵ� ���� ���� �����Ͻʽÿ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa2_Text2, true));

        // Ʃ�� ���� 2 �Լ�
        // 
        
        StartCoroutine(Daehwa3());
    }

    private IEnumerator Daehwa3()
    {
        string Daehwa3_Text1 = "�� ģ������ ����޴� ����� ���Ѻ��� �ϴٴ�! �󸶳� ������ �ֵ��̾��µ�!!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa3_Text1, true));

        string Daehwa3_Text2 = "���� ������� �������� �� ä�� ���� �� ������ �� ��� ������!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa3_Text2, true));

        string Daehwa3_Text3 = "����? �ٰ��� ������ ���� �� �Ƴ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa3_Text3, false));

        string Daehwa3_Text4 = "������ ���ٰ� �� �Ƹ�带 ���� ���Ƚ��ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa3_Text4, true));

        string Daehwa3_Text5 = "...�� �� ���� ���� ����.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa3_Text5, false));

        // Ʃ�� ���� 3 �Լ�
        // 
        StartCoroutine(Daehwa4());
    }

    private IEnumerator Daehwa4()
    {
        string Daehwa4_Text1 = "�׷���..���� �Ҹ��� ������ �� �ƴϾ���?\n�� ��Ʈ�γ� �Ҹ��� �޾�ĥ �� ���� �Ҹ� ���� �� �� ����?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa4_Text1, false));

        string Daehwa4_Text2 = "�װ� �� ������ ���� ������ �Ҹ��Դϴ�.\n���Բ��� ������ �ع��Ű�� ���ϸ� �׸����� ���ư� �̴ϴ�!";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa4_Text2, true));

        string Daehwa4_Text3 = "�Ҹ��� ���µ� �츮 ��ȭ�� ��� �ϰ� �ִ� �ǵ�?";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "�Ҹ�", Daehwa4_Text3, false));

        string Daehwa4_Text4 = "�׾� �̰� �ؽ�Ʈ�ݾƿ�.���� �� �� Ƽ �� ���� ������.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "����", Daehwa4_Text4, true));

        // Ʃ�� ���� 4 �Լ�
        // 
        StartCoroutine(Daehwa5());
    }

    private IEnumerator Daehwa5()
    {
        string Daehwa5_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa5_Text1, false));

        string Daehwa5_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa5_Text2, false));

        // Ʃ�� ���� 5 �Լ�
        // 

        StartCoroutine(Daehwa6());
    }

    private IEnumerator Daehwa6()
    {
        string Daehwa6_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa6_Text1, false));

        string Daehwa6_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa6_Text2, false));

        // Ʃ�� ���� 6 �Լ�
        // 

        StartCoroutine(Daehwa_Final());
    }

    private IEnumerator Daehwa_Final()
    {
        string Daehwa7_Text1 = "���� �׸� �⸰ �׸��� �� �׸� �⸰ �׸��̰� �ϰ� �׸� �⸰ �׸��� �� ���׸� �⸰ �׸��̴�.";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text1, false));

        string Daehwa7_Text2 = "���������� ��������";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa7_Text2, false));

        // Main Scene ��ȯ
    }


}
