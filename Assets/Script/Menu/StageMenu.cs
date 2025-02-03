using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageMenu : MonoBehaviour
{
    Image imgStage1;
    Image Sword;
    GameObject Star0;
    GameObject Star1;
    GameObject Star2;
    GameObject Star3;
    private float elapsedTime = 0f;
    // 스테이지 점수
    DatabaseManager theDatabase;
    [SerializeField] TextMeshProUGUI txtStageScore;

    private bool EnableStageMenuText = true;
    GameObject StageMenuTextObj;
    TextMeshProUGUI StageMenuText;
    private Color StageMenuText_originalColor;
    float idleTime = 0f;
    bool isFadingIn = true;
    float fadeInTimer = 0f;
    private float fadeInStartTime = 0f;

    void Start()
    {
        RectTransform imgHistoryRect = GameObject.Find("Img_History").GetComponent<RectTransform>();
        imgStage1 = GameObject.Find("Img_Stage1").GetComponent<Image>(); ;
        Sword = GameObject.Find("Img_Sword").GetComponent<Image>();
        Star0 = GameObject.Find("Img_Star0");
        Star1 = GameObject.Find("Img_Star1");
        Star2 = GameObject.Find("Img_Star2");
        Star3 = GameObject.Find("Img_Star3");
        StageMenuTextObj = GameObject.Find("StageMenuText");
        StageMenuText = StageMenuTextObj.GetComponent<TextMeshProUGUI>();
        StageMenuTextObj.SetActive(false);
        if (StageMenuText != null)
        {
            StageMenuText_originalColor = StageMenuText.color;
        }

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
            // CD 회전
            imgStage1.rectTransform.Rotate(0, 0, 1.7f * Time.deltaTime);  

            // 칼 둥둥 떠다니는 느낌
            elapsedTime += Time.deltaTime;
            float newY = Sword.rectTransform.anchoredPosition.y + Mathf.Sin(elapsedTime) * 0.05f;
            Sword.rectTransform.anchoredPosition = new Vector2(Sword.rectTransform.anchoredPosition.x, newY);
        }

        if (TitleMenu.SwordUpEnd & EnableStageMenuText)
        {
            if (Input.anyKey || Input.GetMouseButton(0))
            {
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            if (idleTime >= 5f) // 5초 이상 입력 없으면 활성화
            {
                StageMenuTextObj.SetActive(true);
                EnableStageMenuText = false;
            }
        }

        if (StageMenuText != null & !EnableStageMenuText)
        {
            if (isFadingIn)
            {
                // 처음 페이드 인 (0 → 1)
                fadeInTimer += Time.deltaTime / 2f; // 2초 동안 페이드 인
                float alpha = Mathf.Clamp01(fadeInTimer);
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);

                if (fadeInTimer >= 1f)
                {
                    isFadingIn = false;
                    fadeInStartTime = Time.time;
                }
            }
            else
            {
                float elapsedTime = Time.time - fadeInStartTime;
                float alpha = (Mathf.Sin(elapsedTime * 1f + Mathf.PI / 3) * 0.35f + 0.65f);
                StageMenuText.color = new Color(StageMenuText_originalColor.r, StageMenuText_originalColor.g, StageMenuText_originalColor.b, alpha);
            }
        }


        // 임시로 update에 구현
        txtStageScore.text = string.Format("{0:#,##0}", theDatabase.score[TitleMenu.SelectedLV]);     

        switch (theDatabase.star[TitleMenu.SelectedLV])
        {
            case 0:
                Star0.SetActive(true);
                Star1.SetActive(false);
                Star2.SetActive(false);
                Star3.SetActive(false);
                break;
            case 1:
                Star0.SetActive(false);
                Star1.SetActive(true);
                Star2.SetActive(false);
                Star3.SetActive(false);
                break;
            case 2:
                Star0.SetActive(false);
                Star1.SetActive(false);
                Star2.SetActive(true);
                Star3.SetActive(false);
                break;
            case 3:
                Star0.SetActive(false);
                Star1.SetActive(false);
                Star2.SetActive(false);
                Star3.SetActive(true);
                break;
            default:
                Debug.Log("잘못된 데이터베이스 정보(Star)");
                break;
        }
    }

    private void OnEnable()
    {
        
    }
}
