using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageMenu : MonoBehaviour
{
    Image imgStage1;
    Image Sword;
    private float elapsedTime = 0f;
    // �������� ����
    DatabaseManager theDatabase;
    [SerializeField] TextMeshProUGUI txtStageScore = null;

    void Start()
    {
        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        imgStage1 = GameObject.Find("Img_Stage1").GetComponent<Image>(); ;
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        theDatabase = FindObjectOfType<DatabaseManager>();

        imgHistoryRect.anchorMin = new Vector2(0, 0.75f);
        imgHistoryRect.anchorMax = new Vector2(1, 1);

        imgHistoryRect.offsetMin = Vector2.zero;
        imgHistoryRect.offsetMax = Vector2.zero;

        imgHistoryRect.pivot = new Vector2(0.5f, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (TitleMenu.SwordUpEnd)
        {
            // CD ȸ��
            imgStage1.rectTransform.Rotate(0, 0, 1.7f * Time.deltaTime);  

            // Į �յ� ���ٴϴ� ����
            elapsedTime += Time.deltaTime;
            float newY = Sword.rectTransform.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            Sword.rectTransform.anchoredPosition = new Vector2(Sword.rectTransform.anchoredPosition.x, newY);
        }

        // �ӽ÷� update�� ����
        txtStageScore.text = string.Format("{0:#,##0}", theDatabase.score[TitleMenu.SelectedLV]);
    }
}
