using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image DamageOverlayImage;
    [SerializeField] private float fadeDuration = 1f; 
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

        if (DamageOverlayImage == null)
        {
            StartCoroutine(LoadDamageOverlay());
        }
        else
        {
            SetAlpha(0f);
        }
    }

    private IEnumerator LoadDamageOverlay()
    {
        ResourceRequest request = Resources.LoadAsync<Sprite>("Sprites/DamageOverlay");
        yield return request;

        if (request.asset != null)
        {
            cachedDamageOverlaySprite = request.asset as Sprite;
            GameObject overlayGO = new GameObject("DamageOverlayImage");
            overlayGO.transform.SetParent(this.transform, false);

            DamageOverlayImage = overlayGO.AddComponent<Image>();
            DamageOverlayImage.sprite = cachedDamageOverlaySprite;
            DamageOverlayImage.color = new Color(1f, 1f, 1f, 0f); // 초기 알파값 (투명)

            RectTransform rectTransform = DamageOverlayImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            Debug.Log("DamageOverlay 못찾음");
        }
    }

    public void ShowDamageOverlayEffect()
    {
        if (isFading) return;
        StartCoroutine(FadeEffect());
    }

    private IEnumerator FadeEffect()
    {
        if (DamageOverlayImage == null) yield break;

        isFading = true;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsedTime / (fadeDuration / 2));
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(maxAlpha);

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration / 2)
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
}