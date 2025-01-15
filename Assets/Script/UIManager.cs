using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 싱글톤
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image DamageOverlayImage;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float maxAlpha = 0.5f;

    private bool isFading = false;
    private Sprite cachedDamageOverlaySprite;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RectTransform rt = DamageOverlayImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        if (DamageOverlayImage == null)
        {
            Debug.Log("DamageOverlay 못찾음");
        }
        else
        {
            SetAlpha(0f);
        }
    }


    public void ShowDamageOverlayEffect()
    {
        if (isFading) return;
        StartCoroutine(FadeEffect());
        Debug.Log("함수 호출 완료");
    }

    private IEnumerator FadeEffect()
    {
        if (DamageOverlayImage == null) yield break;

        isFading = true;

        Debug.Log("페이드인 시작");
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration * 0.2f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsedTime / (fadeDuration / 2));
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(maxAlpha);

        Debug.Log("페이드아웃 시작");
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration * 0.8f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, elapsedTime / (fadeDuration / 2));
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0f);

        isFading = false;
    }

    private void SetAlpha(float alpha)
    {
        if (DamageOverlayImage != null)
        {
            Color color = DamageOverlayImage.color;
            color.a = alpha;
            DamageOverlayImage.color = color;
        }
    }

    // PlayerManager에서 hp가 깎일 때 or ScoreManager에서 피격 판정 나왔을 때
    // UIManager.Instance.ShowDamageOverlayEffect(); 호출


    [SerializeField] private GameObject particlePrefab;

    private Vector2 position_up = new Vector2(0, 0.6f);
    private Vector2 position_down = new Vector2(0, -0.6f);

    public void ShowParticle(Direction direction)   // 패링 파티클 이펙트 효과
    {
        Vector2 spawnPos = new Vector2(0, 0);
        switch (direction)
        {
            case Direction.Up:
                spawnPos = position_up; break;

            case Direction.Down:
                spawnPos = position_down; break;

            // case Direction.Left:

            // case Direction.Right:

        }
        GameObject particleObj = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

        ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }
}