using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면 연출 및 UI 관리
/// </summary>
public class TitleUI : MonoBehaviour
{
    private Vector2 startPos;
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float slideDuration;

    [SerializeField] private Image titleImg;
    [SerializeField] private TextMeshProUGUI titleText;

    public float transitionDur;

    [Header("Camera")]
    public GameObject mainCamera;
    // 카메라 무브먼트가 많아지면 따로 스크립트 분리 고려

    public bool isActivated = false;

    /// <summary>
    /// TitleUI 초기화
    /// </summary>
    public void InitUI()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10);  // 임시로 값 하드코딩
        titleImg.color = new Color(titleImg.color.r, titleImg.color.g, titleImg.color.b, 1f);
    }

    void Update()
    {
        // 타이틀 안내 문구 펄스 효과
        if (titleText != null)
        {
            float alpha = (Mathf.Sin(Time.time * 1f) * 0.35f + 0.65f);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, alpha);
        }
    }

    /// <summary>
    /// 스와이프 업 이벤트 처리
    /// </summary>
    public void OnSwipeUp()
    {
        if (!isActivated)
        {
            isActivated = true;
            titleText.gameObject.SetActive(false);
            StartCoroutine(LogoFade());
            StartCoroutine(MoveUI(transitionDur));
            MenuManager.Instance.sword.StartSwordUp(-MenuManager.Instance.height / 7, transitionDur);
        }
    }

    /// <summary>
    /// UI를 ease out 효과로 아래로 이동시키는 코루틴
    /// </summary>
    /// <param name="targetY">World 좌표 기준 이동 대상 Y좌표</param>
    /// <param name="duration">이동에 걸리는 시간(초)</param>
    private IEnumerator MoveUI(float duration)
    {
        RectTransform titleUIMoveRect = transform.GetComponent<RectTransform>();
        RectTransform menuUIMoveRect = MenuManager.Instance.transform.GetComponent<RectTransform>();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easeOutT = 1f - Mathf.Pow(1f - t, 2);

            titleUIMoveRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, -MenuManager.Instance.height), easeOutT);
            menuUIMoveRect.anchoredPosition = titleUIMoveRect.anchoredPosition + new Vector2(0f, MenuManager.Instance.height);

            yield return null;
        }
        titleUIMoveRect.anchoredPosition = new Vector2(0f, -MenuManager.Instance.height);
        menuUIMoveRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 로고 페이드 아웃 코루틴
    /// </summary>
    private IEnumerator LogoFade()
    {
        float dur = transitionDur / 1.5f;
        float elapsedTime = 0f;
        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / dur);
            titleImg.color = new Color(titleImg.color.r, titleImg.color.g, titleImg.color.b, alpha);

            yield return null;
        }
        titleImg.color = new Color(titleImg.color.r, titleImg.color.g, titleImg.color.b, 0f);
    }
}
