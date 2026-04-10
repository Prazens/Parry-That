using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DynamicUIManager : MonoBehaviour
{
    public static DynamicUIManager Instance { get; private set; }

    [SerializeField] private Transform canvasRoot;

    // -------------------------
    // HUD
    // -------------------------
    public GameObject scoreDisplay;
    public GameObject comboDisplay;
    public GameObject judgeDisplayPrefab;
    public Sprite[] judgeImages = new Sprite[4];

    public GameObject hpDisplay;
    public GameObject[] heartDisplays = new GameObject[10];
    public GameObject heartDisplayPrefab;
    public Sprite[] heartImages = new Sprite[2];

    private Vector3[] initialPosition = new Vector3[3];

    // -------------------------
    // FX
    // -------------------------
    [Header("Damage Overlay")]
    [SerializeField] private Image DamageOverlayImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float maxAlpha = 0.6f;
    private bool isFading = false;

    [Header("Particles")]
    public GameObject[] ParticleParried;
    public GameObject[] ParticlePerfect;

    private Vector2 position_up = new Vector2(0f, 0.6f);
    private Vector2 position_down = new Vector2(0f, -0.6f);

    // -------------------------
    // CutIn (구버전 UIManager 그대로 이식)
    // -------------------------
    [Header("CutIn")]
    [SerializeField] private RectTransform cutInParent; // Canvas 아래에 있는 컨테이너
    [SerializeField] private GameObject[] cutScenes; // 0: Up, 1: Down

    public bool isStopEaseIn = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (DamageOverlayImage != null)
        {
            RectTransform rt = DamageOverlayImage.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            SetAlpha(0f);
        }
    }

    // -------------------------
    // HUD
    // -------------------------
    public void Setup_UI()
    {
        // -------------------------
        // HUD 기본 세팅 (원래 코드 그대로)
        // -------------------------
        if (StageFlowManager.Instance.currentStageData.Category == StageCategory.Tutorial)
        {
            if (hpDisplay != null) hpDisplay.SetActive(false);
            if (scoreDisplay != null) scoreDisplay.SetActive(false);
            return;
        }

        if (hpDisplay != null) hpDisplay.SetActive(true);

        if (scoreDisplay != null)
        {
            scoreDisplay.GetComponent<TextMeshProUGUI>().text = "0";
            initialPosition[0] = scoreDisplay.transform.position;
            scoreDisplay.SetActive(true);
        }

        if (comboDisplay != null)
        {
            comboDisplay.GetComponent<TextMeshProUGUI>().text = "0";
            initialPosition[1] = comboDisplay.transform.position;
            comboDisplay.SetActive(true);
        }

        if (hpDisplay != null)
        {
            initialPosition[2] = hpDisplay.transform.position;
        }

        DisplayScore(0);
        DisplayCombo(0);

        if (heartDisplayPrefab != null && hpDisplay != null)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject heartDisplay = Instantiate(heartDisplayPrefab, hpDisplay.transform);
                heartDisplay.transform.localPosition = new Vector3(27 * (i % 5), -27 * (i / 5), 0);

                heartDisplays[i] = heartDisplay;

                if (heartImages != null && heartImages.Length > 0)
                {
                    heartDisplay.GetComponent<Image>().sprite = heartImages[0];
                }
            }
        }
    }


    public void SetCutInObjects(GameObject upObj, GameObject downObj)
    {
        if (cutScenes == null || cutScenes.Length < 2)
            cutScenes = new GameObject[2];

        cutScenes[0] = upObj;
        cutScenes[1] = downObj;
    }

    public void HideAll()
    {
        if (scoreDisplay != null)
        {
            scoreDisplay.GetComponent<TextMeshProUGUI>().text = " ";
        }

        for (int i = 0; i < 10; i++)
        {
            if (heartDisplays[i] != null)
            {
                Destroy(heartDisplays[i]);
            }
        }
    }

    public void DisplayScore(int score)
    {
        if (scoreDisplay == null) return;

        scoreDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(score);

        StopCoroutine("BounceUp");
        scoreDisplay.transform.position = initialPosition[0];
        StartCoroutine(BounceUp(scoreDisplay));
    }

    public void DisplayCombo(int combo)
    {
        if (comboDisplay == null) return;

        comboDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(combo) + " Combo";

        StopCoroutine("BounceUp");
        comboDisplay.transform.position = initialPosition[1];
        StartCoroutine(BounceUp(comboDisplay));
    }

    public void DisplayHP(int hp, bool heal = false)
    {
        if (StageFlowManager.Instance.currentStageData.Category == StageCategory.Tutorial) return;
        if (hpDisplay == null) return;

        if (!heal)
        {
            if (hp >= 0 && hp < heartDisplays.Length && heartDisplays[hp] != null)
                heartDisplays[hp].GetComponent<Image>().sprite = heartImages[1];
        }
        else
        {
            int index = hp - 1;
            if (index >= 0 && index < heartDisplays.Length && heartDisplays[index] != null)
                heartDisplays[index].GetComponent<Image>().sprite = heartImages[0];
        }

        StopCoroutine("BounceUp");
        hpDisplay.transform.position = initialPosition[2];
        StartCoroutine(BounceUp(hpDisplay));
    }

    public void DisplayJudge(int judge, Direction direction)
    {
        if (judgeDisplayPrefab == null) return;

        Vector3 generatePosition = Vector3.up;

        switch (direction)
        {
            case Direction.Up:
                generatePosition = Vector3.up * Screen.height / 6;
                break;
            case Direction.Down:
                generatePosition = Vector3.down * Screen.height / 6;
                break;
            case Direction.Left:
                generatePosition = Vector3.left * 120;
                break;
            case Direction.Right:
                generatePosition = Vector3.right * 120;
                break;
        }

        GameObject judgeDisplay = Instantiate(judgeDisplayPrefab);
        judgeDisplay.transform.SetParent(canvasRoot);
        judgeDisplay.transform.localScale = new Vector3(0.12f, 0.12f, 0f);
        judgeDisplay.transform.position = new Vector3(Screen.width / 2, Screen.height / 2) + generatePosition;

        int spriteIndex = math.abs(judge - 4);

        if (judgeImages != null && spriteIndex >= 0 && spriteIndex < judgeImages.Length)
        {
            judgeDisplay.GetComponent<Image>().sprite = judgeImages[spriteIndex];
        }

        StartCoroutine(JudgeBounceUp(judgeDisplay));
    }

    private IEnumerator BounceUp(GameObject obj)
    {
        for (int i = 3; i >= 1; i--)
        {
            obj.transform.position += Vector3.up * 19 / 3 * Screen.height / 800;
        }

        for (int i = 9; i >= 1; i--)
        {
            obj.transform.position += Vector3.down * i * i / 15 * Screen.height / 800;
            yield return null;
        }
    }

    private IEnumerator JudgeBounceUp(GameObject obj)
    {
        for (int i = 3; i >= 1; i--)
        {
            obj.transform.position += Vector3.up * 19 / 3 * Screen.height / 800;
        }

        for (int i = 9; i >= 1; i--)
        {
            obj.transform.position += Vector3.down * i * i / 15 * Screen.height / 800;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.4f);
        Destroy(obj);
    }

    // -------------------------
    // Damage overlay
    // -------------------------
    public void ShowDamageOverlayEffect()
    {
        if (DamageOverlayImage == null) return;
        if (isFading) return;
        StartCoroutine(FadeEffect());
    }

    private IEnumerator FadeEffect()
    {
        if (DamageOverlayImage == null) yield break;

        isFading = true;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration * 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsedTime / (fadeDuration / 2f));
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(maxAlpha);

        elapsedTime = 0f;

        while (elapsedTime < fadeDuration * 0.99f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, elapsedTime / (fadeDuration / 2f));
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);

        isFading = false;
    }

    private void SetAlpha(float alpha)
    {
        if (DamageOverlayImage == null) return;

        Color color = DamageOverlayImage.color;
        color.a = alpha;
        DamageOverlayImage.color = color;
    }

    // -------------------------
    // Particle
    // -------------------------
    public void ShowParticle(Direction direction, bool perfect)
    {
        Vector2 spawnPos = Vector2.zero;

        switch (direction)
        {
            case Direction.Up:
                spawnPos = position_up;
                break;
            case Direction.Down:
                spawnPos = position_down;
                break;
        }

        GameObject particleObj2;
        int randNum = UnityEngine.Random.Range(0, 3);

        if (perfect)
        {
            particleObj2 = Instantiate(ParticlePerfect[randNum], spawnPos, Quaternion.identity);
        }
        else
        {
            particleObj2 = Instantiate(ParticleParried[randNum], spawnPos, Quaternion.identity);
        }

        Animator particle = particleObj2.GetComponentInChildren<Animator>();
        if (particle != null)
        {
            float length = particle.GetCurrentAnimatorStateInfo(0).length;
            StartCoroutine(DestroyAfterAnimation(particleObj2, length));
        }
    }

    private IEnumerator DestroyAfterAnimation(GameObject target, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(target);
    }

    // =========================================================
    // CutIn : 구버전 UIManager 로직 그대로 이식
    // =========================================================
    private IEnumerator EaseInEffect(GameObject targetUIImage, Direction direction, float _duration)
    {
        float elapsedTime = 0f;
        float duration = _duration;

        Animator animator = targetUIImage.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("cutIn");
        }

        Image uiImage = targetUIImage.GetComponent<Image>();
        RectTransform uiElement = targetUIImage.GetComponent<RectTransform>();

        Vector2 startPos = uiElement.anchoredPosition;

        while (elapsedTime < duration + 0.12f)
        {
            if (isStopEaseIn)
            {
                break;
            }

            float t = (duration <= 0f) ? 1f : (elapsedTime / duration);
            t = Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(t))); // Ease out

            switch (direction)
            {
                case Direction.Up:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.right * 1600f, t);
                    break;

                case Direction.Down:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.left * 1600f, t);
                    break;
            }

            if (uiImage != null)
            {
                uiImage.color = Color.Lerp(Color.black, Color.white, t);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector2 currentPos = uiElement.anchoredPosition;

        elapsedTime = 0f;
        while (elapsedTime < 0.3f)
        {
            float t = elapsedTime / 0.3f;
            uiElement.anchoredPosition = Vector2.Lerp(currentPos, startPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (uiImage != null) uiImage.color = Color.white;
        uiElement.anchoredPosition = startPos;

        if (animator != null)
        {
            animator.SetTrigger("cutOut");
        }
    }

    public void CutInDisplay(float targetTimeSeconds, bool isHide = false)
    {
        if (cutScenes == null || cutScenes.Length < 2) return;

        isStopEaseIn = isHide;
        if (!isHide)
        {
            float duration = targetTimeSeconds - StageFlowManager.Instance.currentTime;
            StartCoroutine(EaseInEffect(cutScenes[0], Direction.Up, duration));
            StartCoroutine(EaseInEffect(cutScenes[1], Direction.Down, duration));
        }
    }
}
