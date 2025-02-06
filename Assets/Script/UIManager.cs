using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱���
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image DamageOverlayImage;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float maxAlpha = 0.05f;

    public GameObject[] ParticleParried;
    public GameObject[] ParticlePerfect;

    private bool isFading = false;
    private Sprite cachedDamageOverlaySprite;

    [SerializeField] private GameObject[] cutScenes;

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
        while (elapsedTime < fadeDuration * 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsedTime / (fadeDuration / 2));
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(maxAlpha);

        Debug.Log("���̵�ƿ� ����");
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration * 0.99f)
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


    // [SerializeField] private GameObject particlePrefab;

    private Vector2 position_up = new Vector2(0, 0.6f);
    private Vector2 position_down = new Vector2(0, -0.6f);

    public void ShowParticle(Direction direction, bool perfect)   // �и� ��ƼŬ ����Ʈ ȿ��
    {
        Vector2 spawnPos = new Vector2(0, 0);
        switch (direction)  // �¿� �и� �߰� �� �ش� �κ� �۾� �ʿ�
        {
            case Direction.Up:
                spawnPos = position_up; break;

            case Direction.Down:
                spawnPos = position_down; break;

            // case Direction.Left:

            // case Direction.Right:

        }
        //GameObject particleObj = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

        //ParticleSystem ps = particleObj.GetComponentInChildren<ParticleSystem>();
        //if (ps != null)
        //{
        //    ps.Play();
        //}

        ////////////////////////////////////////////////////////////////////////////////////////////
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
            string animationTriggerName = ParticleParried[randNum].name;
            // particle.Play(animationTriggerName);

            float length = particle.GetCurrentAnimatorStateInfo(0).length;
            StartCoroutine(DestroyAfterAnimation(particleObj2, (float)length));
        }

    }


    private System.Collections.IEnumerator DestroyAfterAnimation(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    private IEnumerator EaseInEffect(GameObject targetUIImage, Direction direction, float _duration)
    {
        float elapsedTime = 0;
        float duration = _duration;

        Animator animator = targetUIImage.GetComponent<Animator>();
        animator.SetTrigger("cutIn");

        Image uiImage = targetUIImage.GetComponent<Image>();
        RectTransform uiElement = targetUIImage.GetComponent<RectTransform>();

        Vector2 startPos = uiElement.anchoredPosition;

        while (elapsedTime < duration + 0.12f)
        {
            if (isStop1)
            {
                elapsedTime = 0;
                isStop1 = false;
                break;
            }
            if (isStop2)
            {
                elapsedTime = 0;
                isStop2 = false;
                break;
            }

            float t = elapsedTime / duration;
            t = Mathf.Sqrt(Mathf.Sqrt(t)); // Ease out ���� (t^4)

            // ��ġ �̵� (Ease In)
            switch (direction)
            {
                case Direction.Up:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.right * 1600, t);
                    break;
                case Direction.Down:
                    uiElement.anchoredPosition = Vector2.Lerp(startPos, startPos + Vector2.left * 1600, t);
                    break;
            }
            
            // ��� ���� (Ease out)
            uiImage.color = Color.Lerp(Color.black, Color.white, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector2 currentPos = uiElement.anchoredPosition;
        while (elapsedTime < 0.3f)
        {
            float t = elapsedTime / 0.3f;

            // ��ġ �̵�
            uiElement.anchoredPosition = Vector2.Lerp(currentPos, startPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        uiImage.color = Color.white;
        uiElement.anchoredPosition = startPos;
        animator.SetTrigger("cutOut");
    }

    private bool isStop1 = false;
    private bool isStop2 = false;

    public void CutInDisplay(float _duration, bool isHide = false)
    {
        if (isHide)
        {
            isStop1 = true;
            isStop2 = true;
        }
        else
        {
            StartCoroutine(EaseInEffect(cutScenes[0], Direction.Up, _duration));
            StartCoroutine(EaseInEffect(cutScenes[1], Direction.Down, _duration));
        }
    }

}