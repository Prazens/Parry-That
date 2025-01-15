using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱���
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
        DamageOverlayImage.gameObject.SetActive(true);

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
            DamageOverlayImage.color = new Color(1f, 1f, 1f, 0f); // �ʱ� ���İ� (����)

            RectTransform rectTransform = DamageOverlayImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            Debug.Log("DamageOverlay ��ã��");
        }
    }

    public void ShowDamageOverlayEffect()
    {
        if (isFading) return;
        StartCoroutine(FadeEffect());
        Debug.Log("�Լ� ȣ�� �Ϸ�");
    }

    private IEnumerator FadeEffect()
    {
        if (DamageOverlayImage == null) yield break;

        isFading = true;

        Debug.Log("���̵��� ����");
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration * 0.2f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsedTime / (fadeDuration / 2));
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(maxAlpha);

        Debug.Log("���̵�ƿ� ����");
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

    // PlayerManager���� hp�� ���� �� or ScoreManager���� �ǰ� ���� ������ ��
    // UIManager.Instance.ShowDamageOverlayEffect(); ȣ��
}