using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;

    [Header("ĳ���� �̹���")]
    [SerializeField] private Sprite[] CharacterSprite;
    private void Awake()
    {
        // Ʃ�丮�� ���۽� ��ȯ/������ �͵�



    }
    private void Start()
    {
        StartCoroutine(Daehwa1());
    }
    private IEnumerator Daehwa1()
    {
        string Daehwa1_Text1 = "�����ٶ󸶹ٻ������īŸ���ϰ����ٶ󸶹ٻ������īŸ���� �����ٶ󸶹ٻ������īŸ����";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa1_Text1, false));

        string Daehwa1_Text2 = "����Ÿī���ھƻ�ٸ���ٳ��� ����Ÿī���ھƻ�ٸ���ٳ�������Ÿī���ھƻ�ٸ���ٳ���";
        yield return StartCoroutine(dialogueManager.ShowDialogue(CharacterSprite[0], "���ΰ�", Daehwa1_Text2, true));


        // ���� �Լ� ȣ��
    }
}
