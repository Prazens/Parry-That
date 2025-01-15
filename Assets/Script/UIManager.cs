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

        if (DamageOverlayImage == null)
        {
            Debug.Log("DamageOverlay ��ã��");
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