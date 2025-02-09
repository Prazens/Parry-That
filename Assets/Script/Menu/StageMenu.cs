using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;

public class StageMenu : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Image imgStage1;
    Image Sword;
    GameObject Star0;
    GameObject Star1;
    GameObject Star2;
    GameObject Star3;
    private float elapsedTime = 0f;
    // �������� ����
    DatabaseManager theDatabase;
    [SerializeField] TextMeshProUGUI txtStageName;
    [SerializeField] TextMeshProUGUI txtStageScore;

    private bool EnableStageMenuText = true;
    GameObject StageMenuTextObj;
    TextMeshProUGUI StageMenuText;
    private Color StageMenuText_originalColor;
    float idleTime = 0f;
    bool isFadingIn = true;
    float fadeInTimer = 0f;
    private float fadeInStartTime = 0f;

    [SerializeField] private GameObject BlackOverlayObj;
    private Image BlackOverlay;

    private string[] StageName = { "Beat Master", "Stage2", "Stage3", "Stage4" };

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

        BlackOverlay = BlackOverlayObj.GetComponent<Image>();
        RectTransform BlackOverlayRT = BlackOverlay.GetComponent<RectTransform>();
        BlackOverlayRT.anchorMin = new Vector2(0, 0);
        BlackOverlayRT.anchorMax = new Vector2(1, 1);
        Color originalOverlayColor = BlackOverlay.color;
        BlackOverlay.color = new Color (originalOverlayColor.r, originalOverlayColor.g, originalOverlayColor.b, 0f);
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

            if (idleTime >= 5f) // 5�� �̻� �Է� ������ Ȱ��ȭ
            {
                StageMenuTextObj.SetActive(true);
                EnableStageMenuText = false;
            }
        }

        if (StageMenuText != null & !EnableStageMenuText)
        {
            if (isFadingIn)
            {
                // ó�� ���̵� �� (0 �� 1)
                fadeInTimer += Time.deltaTime / 2f; // 2�� ���� ���̵� ��
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


        // �ӽ÷� update�� ����
        txtStageScore.text = string.Format("{0:#,##0}", theDatabase.score[currentIndex + 1]);
        txtStageName.text = StageName[currentIndex];
        switch (theDatabase.star[currentIndex + 1])
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
                Debug.Log("�߸��� �����ͺ��̽� ����(Star)");
                break;
        }
    }

    public void SelectStage()
    {
        StartCoroutine(SelectStageCoroutine());
    }
    public IEnumerator SelectStageCoroutine()
    {
        BlackOverlayObj.SetActive(true);
        Vector2 startPosition = Sword.rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, Screen.height * 1.5f);
        float elapsedTime = 0f;
        float duration = 1f; // �ִϸ��̼� ���� �ð�

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            Sword.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            float overlayAlpha = (t > 0.8f) ? 1f : Mathf.Clamp01(t * 1.3f);
            BlackOverlay.color = new Color(0f, 0f, 0f, overlayAlpha);

            yield return null; // ���� �����ӱ��� ���
        }

        SceneLinkage.StageLV = currentIndex + 1;
        SceneManager.LoadScene("Loading");
    }
    private void OnEnable()
    {
        
    }

    // ���⼭���� �¿� �������� ���� �ڵ�
    [Header("Stage Objects")]
    public List<RectTransform> stageObjects;  
    public static int currentIndex = 0;

    private float threshold = 300f;
    private float swipeSpeed = 0.6f;   // ����
    private float transitionTime = 0.3f; // �ִϸ��̼� �ð�

    private float screenWidth;
    private bool isDragging = false;

    private float minScale = 0.8f;            // �翷�� �� �ּ� ������
    private float maxScale = 1.0f;            // �߾��� �� �ִ� ������
    private float distanceToFullDark = 600f;  // �߾ӿ��� �̸�ŭ �������� ��Ӱ�
    private Color darkColor = new Color(1f, 1f, 1f, 0.5f);
    private Color brightColor = new Color(1f, 1f, 1f, 1f);

    private void Awake()
    {
        screenWidth = Screen.width;

        // ��� ������Ʈ�� "���� �ε���" �������� �ڸ� ��ġ
        UpdateStagePositions();

        // ũ��/���� ����
        UpdateScaleAndColor();

        // "����, �翷"�� �Ѱ�, ������ ��, ��� �Ǵ� �Լ�
        ActivateOnlyRelevantObjects();
    }

    private void ActivateOnlyRelevantObjects()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            stageObjects[i].gameObject.SetActive(false);
        }

        stageObjects[currentIndex].gameObject.SetActive(true);
        if (currentIndex - 1 >= 0)
            stageObjects[currentIndex - 1].gameObject.SetActive(true);
        if (currentIndex + 1 < stageObjects.Count)
            stageObjects[currentIndex + 1].gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        float deltaX = eventData.delta.x * swipeSpeed;

        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf)
                continue;

            stageObjects[i].anchoredPosition += new Vector2(deltaX, 0);
        }

        UpdateScaleAndColor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float centerX = stageObjects[currentIndex].anchoredPosition.x;

        if (Mathf.Abs(centerX) > threshold)
        {
            // ���� ��������(centerX < 0) �� currentIndex + 1
            if (centerX < 0)
            {
                if (currentIndex < stageObjects.Count - 1)
                {
                    currentIndex++;
                }
            }
            // ������ ��������(centerX > 0) �� currentIndex - 1
            else
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                }
            }
        }

        // ��ġ ����(�ڷ�ƾ)
        StopAllCoroutines();
        StartCoroutine(SmoothMove());
    }

    private IEnumerator SmoothMove()
    {
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < stageObjects.Count; i++)
        {
            startPositions.Add(stageObjects[i].anchoredPosition);
        }
        Dictionary<int, Vector2> targetPos = CalculateTargetPositions();

        ActivateOnlyRelevantObjects();

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            for (int i = 0; i < stageObjects.Count; i++)
            {
                if (!stageObjects[i].gameObject.activeSelf)
                    continue;

                Vector2 sp = startPositions[i];
                Vector2 ep = targetPos[i];
                stageObjects[i].anchoredPosition = Vector2.Lerp(sp, ep, t);
            }

            UpdateScaleAndColor();
            yield return null;
        }

        foreach (var kv in targetPos)
        {
            if (stageObjects[kv.Key].gameObject.activeSelf)
            {
                stageObjects[kv.Key].anchoredPosition = kv.Value;
            }
        }

        UpdateScaleAndColor();
    }

    private Dictionary<int, Vector2> CalculateTargetPositions()
    {
        Dictionary<int, Vector2> result = new Dictionary<int, Vector2>();

        for (int i = 0; i < stageObjects.Count; i++)
        {
            int diff = i - currentIndex;

            Vector2 pos;
            if (diff == 0)
            {
                pos = Vector2.zero;
            }
            else if (diff == 1)
            {
                pos = new Vector2(screenWidth, 0);
            }
            else if (diff == -1)
            {
                pos = new Vector2(-screenWidth, 0);
            }
            else if (diff > 1)
            {
                pos = new Vector2(screenWidth * 2, 0);
            }
            else
            {
                pos = new Vector2(-screenWidth * 2, 0);
            }

            result[i] = pos;
        }

        return result;
    }

    private void UpdateScaleAndColor()
    {
        for (int i = 0; i < stageObjects.Count; i++)
        {
            if (!stageObjects[i].gameObject.activeSelf)
                continue;

            float dist = Mathf.Abs(stageObjects[i].anchoredPosition.x);
            float factor = Mathf.Clamp01(dist / distanceToFullDark);

            // ������ ����
            float scaleVal = Mathf.Lerp(maxScale, minScale, factor);
            stageObjects[i].localScale = new Vector3(scaleVal, scaleVal, 1f);

            // �÷� ���� 
            Image img = stageObjects[i].GetComponent<Image>();
            if (img)
            {
                img.color = Color.Lerp(brightColor, darkColor, factor);
            }
        }
    }


    private void UpdateStagePositions()
    {
        var targets = CalculateTargetPositions();
        foreach (var kv in targets)
        {
            stageObjects[kv.Key].anchoredPosition = kv.Value;
        }
    }

}
