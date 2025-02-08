using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱���
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image DamageOverlayImage;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float maxAlpha;

    public GameObject[] ParticleParried;
    public GameObject[] ParticlePerfect;

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
        int randNum = Random.Range(0, 3);
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

}