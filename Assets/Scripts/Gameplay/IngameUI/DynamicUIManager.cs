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

    // -------------------------
    // HUD
    // -------------------------
    public GameObject scoreDisplay;
    public GameObject judgeDisplayPrefab;
    public Sprite[] judgeImages = new Sprite[4];

    public GameObject hpDisplay;
    public GameObject[] heartDisplays = new GameObject[10];
    public GameObject heartDisplayPrefab;
    public Sprite[] heartImages = new Sprite[2];

    private Vector3[] initialPosition = new Vector3[2];

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
    // CutIn (구버전 그대로)
    // -------------------------
    [Header("CutIn")]
    [SerializeField] private GameObject[] cutScenes; // 0: Up, 1: Down

    public bool isStop1 = false;
    public bool isStop2 = false;

    // ✅ 두번째부터 안 나오는 문제 방지용 최소치:
    // - 같은 컷인이 다시 들어오면 이전 코루틴을 끊고(StopCoroutine),
    //   상태/트리거를 리셋한 뒤 다시 시작.
    private Coroutine cutInRoutine1;
    private Coroutine cutInRoutine2;

    // -------------------------
    // CutIn Sound (요청: DynamicUIManager로 다시)
    // -------------------------
    [Header("CutIn Sound")]
    [SerializeField] private AudioSource cutInAudioSource;
    [SerializeField] private AudioClip cutInLoopClip;     // 컷인(홀드 중) 루프/지속 사운드
    [SerializeField] private AudioClip cutInEndClip;      // 컷인 종료(홀드 끝) 사운드
    [SerializeField] private float cutInSoundVolume = 1f;

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
    public void Initialize_UI()
    {
        if (TutorialManager.isTutorial)
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
        }
        if (hpDisplay != null)
        {
            initialPosition[1] = hpDisplay.transform.position;
        }

        DisplayScore(0);

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

    public void DisplayHP(int hp, bool heal = false)
    {
        if (TutorialManager.isTutorial) return;
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
        hpDisplay.transform.position = initialPosition[1];
        StartCoroutine(BounceUp(hpDisplay));
    }

    public void DisplayJudge(int judge, Direction direction)
    {
        if (judgeDisplayPrefab == null) return;

        Vector3 generatePosition = Vector3.up;
        switch (direction)
        {
            case Direction.Up: generatePosition = Vector3.up * Screen.height / 6; break;
            case Direction.Down: generatePosition = Vector3.down * Screen.height / 6; break;
            case Direction.Left: generatePosition = Vector3.left * 120; break;
            case Direction.Right: generatePosition = Vector3.right * 120; break;
        }

        GameObject judgeDisplay = Instantiate(judgeDisplayPrefab);
        judgeDisplay.transform.SetParent(transform);
        judgeDisplay.transform.localScale = new Vector3(0.12f, 0.12f, 0f);
        judgeDisplay.transform.position = new Vector3(Screen.width / 2, Screen.height / 2) + generatePosition;

        int spriteIndex = math.abs(judge - 3);
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
            case Direction.Up: spawnPos = position_up; break;
            case Direction.Down: spawnPos = position_down; break;
        }

        GameObject particleObj2;
        int randNum = UnityEngine.Random.Range(0, 3);

        if (perfect) particleObj2 = Instantiate(ParticlePerfect[randNum], spawnPos, Quaternion.identity);
        else particleObj2 = Instantiate(ParticleParried[randNum], spawnPos, Quaternion.identity);

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
    // ✅ CutIn : 구버전 UIManager 로직 "그대로"
    // =========================================================
    private IEnumerator EaseInEffect(GameObject targetUIImage, Direction direction, float _duration)
    {
        float elapsedTime = 0f;
        float duration = _duration;

        Animator animator = targetUIImage.GetComponent<Animator>();
        if (animator != null)
        {
            // 트리거가 씹히는 경우 방지(연속 호출 시)
            animator.ResetTrigger("cutIn");
            animator.ResetTrigger("cutOut");
            animator.SetTrigger("cutIn");
        }

        Image uiImage = targetUIImage.GetComponent<Image>();
        RectTransform uiElement = targetUIImage.GetComponent<RectTransform>();

        Vector2 startPos = uiElement.anchoredPosition;

        // ⭐ 시작 상태 강제(두번째부터 안 나오는 원인 제거)
        if (uiImage != null) uiImage.color = Color.white;
        uiElement.anchoredPosition = startPos;

        while (elapsedTime < duration + 0.12f)
        {
            if (targetUIImage == cutScenes[0] && isStop1)
            {
                elapsedTime = 0f;
                isStop1 = false;
                break;
            }
            if (targetUIImage == cutScenes[1] && isStop2)
            {
                elapsedTime = 0f;
                isStop2 = false;
                break;
            }

            float t = (duration <= 0f) ? 1f : (elapsedTime / duration);
            t = Mathf.Clamp01(t);
            t = Mathf.Sqrt(Mathf.Sqrt(Mathf.Sqrt(t)));

            switch (direction)
            {
                case Direction.Up:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.right * 1600, t);
                    break;
                case Direction.Down:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.left * 1600, t);
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
            animator.ResetTrigger("cutIn");
            animator.ResetTrigger("cutOut");
            animator.SetTrigger("cutOut");
        }
    }

    public void CutInDisplay(float _duration, bool isHide = false)
    {
        if (cutScenes == null || cutScenes.Length < 2) return;

        if (isHide)
        {
            // ✅ 구버전 방식: stop flag만 올려서 코루틴이 “원복+cutOut”까지 가도록 유도
            isStop1 = true;
            isStop2 = true;

            // ✅ 사운드도 같이 끄기 (요청)
            StopCutInHoldSound();
            PlayCutInEndSound();
            return;
        }

        // ✅ 연속 호출되면 이전 코루틴 정리(두번째부터 안 나오는 문제 방지)
        if (cutInRoutine1 != null)
        {
            StopCoroutine(cutInRoutine1);
            cutInRoutine1 = null;
        }
        if (cutInRoutine2 != null)
        {
            StopCoroutine(cutInRoutine2);
            cutInRoutine2 = null;
        }

        // stop 플래그 잔존 방지
        isStop1 = false;
        isStop2 = false;

        cutInRoutine1 = StartCoroutine(EaseInEffect(cutScenes[0], Direction.Up, _duration));
        cutInRoutine2 = StartCoroutine(EaseInEffect(cutScenes[1], Direction.Down, _duration));

        // ✅ 사운드 (요청: DynamicUIManager로 이동)
        PlayCutInHoldSound();
    }

    // =========================================================
    // ✅ CutIn Sound (DynamicUIManager)
    // =========================================================
    public void PlayCutInHoldSound()
    {
        if (cutInAudioSource == null) return;
        if (cutInLoopClip == null) return;

        if (!cutInAudioSource.isPlaying || cutInAudioSource.clip != cutInLoopClip)
        {
            cutInAudioSource.Stop();
            cutInAudioSource.clip = cutInLoopClip;
            cutInAudioSource.loop = true;
            cutInAudioSource.volume = cutInSoundVolume * PlayerPrefs.GetFloat("masterVolume", 1f) * PlayerPrefs.GetFloat("playerVolume", 1f);
            cutInAudioSource.Play();
        }
    }

    public void StopCutInHoldSound()
    {
        if (cutInAudioSource == null) return;
        if (cutInAudioSource.clip == cutInLoopClip)
        {
            cutInAudioSource.Stop();
            cutInAudioSource.clip = null;
        }
    }

    public void PlayCutInEndSound()
    {
        if (cutInAudioSource == null) return;
        if (cutInEndClip == null) return;

        cutInAudioSource.PlayOneShot(
            cutInEndClip,
            cutInSoundVolume * PlayerPrefs.GetFloat("masterVolume", 1f) * PlayerPrefs.GetFloat("playerVolume", 1f)
        );
    }
}
