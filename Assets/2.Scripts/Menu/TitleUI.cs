using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 🔥 DOTween 필수

[RequireComponent(typeof(AudioSource))] 
public class TitleUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image titleImg;
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Header("머티리얼(쉐이더) 연결")]
    public Material flickerMaterial;   
    public Material dissolveMaterial;  
    
    [Header("BGM 플레이어 설정")]
    public AudioClip[] bgmList;        
    private AudioSource bgmSource;     
    
    [Header("발광(Flicker) 민감도 세팅")]
    public float minGlow = 1f;         // 평소의 기본 밝기
    public float maxGlow = 4f;         // 쿵! 할 때 최대 밝기 한계치
    public float glowMultiplier = 20f; // 🔥 스펙트럼 값을 얼마나 증폭시킬지 (소리가 작으면 이 값을 올리세요)
    public float smoothSpeed = 15f;    // 🔥 불빛이 부드럽게 켜지고 꺼지는 속도

    [Header("연출 설정")]
    [SerializeField] private float swipeThreshold = 50f;
    public float transitionDur;

    [Header("Camera")]
    public GameObject mainCamera;
    
    private Tween textPulseTween; 
    
    // 🔥 스펙트럼 분석용 변수로 교체
    private float[] spectrumData = new float[256]; 
    private float currentGlow = 0f;

    public void InitUI()
    {
        mainCamera.transform.position = new Vector3(0, 0, -10);

        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();

        if (bgmList.Length > 0)
        {
            int randomIndex = Random.Range(0, bgmList.Length); 
            bgmSource.clip = bgmList[randomIndex];             
            bgmSource.loop = true;                             
            bgmSource.volume = 1f;                             
            bgmSource.Play();                                  
        }

        titleImg.material = Instantiate(flickerMaterial);
        titleImg.color = Color.white; 
        currentGlow = minGlow; // 초기 밝기 세팅

        if (textPulseTween != null) textPulseTween.Kill();
        titleText.gameObject.SetActive(true);
        titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, 1f);
        textPulseTween = titleText.DOFade(0.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void Update()
    {
        // 🔥 스와이프 전일 때, '저음(Bass)' 스펙트럼을 분석하여 발광시킴
        if (StageDBManager.Instance.isFirstLaunch && bgmSource != null && bgmSource.isPlaying)
        {
            // 1. 소리 주파수 분석
            bgmSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
            float bassValue = 0f;
            
            // 0~9번 인덱스는 주로 묵직한 베이스/드럼 영역입니다.
            for (int i = 0; i < 10; i++) 
            {
                bassValue += spectrumData[i];
            }

            // 2. 목표 밝기 계산 (기본 밝기 + 드럼 소리 * 증폭값), 최대 밝기(maxGlow)를 넘지 않게 제한
            float targetGlow = Mathf.Clamp(minGlow + (bassValue * glowMultiplier), minGlow, maxGlow);
            
            // 3. 부드러운 전환 (Lerp)
            currentGlow = Mathf.Lerp(currentGlow, targetGlow, Time.deltaTime * smoothSpeed);

            // 4. 머티리얼에 값전달
            if (titleImg.material.HasProperty("_GlobalAudio"))
            {
                titleImg.material.SetFloat("_GlobalAudio", currentGlow);
            }
        }
    }

    public void OnSwipeUp()
    {
        if (StageDBManager.Instance.isFirstLaunch)
        {
            StageDBManager.Instance.isFirstLaunch = false;

            if (textPulseTween != null) textPulseTween.Kill();
            titleText.gameObject.SetActive(false);

            if (bgmSource != null)
            {
                bgmSource.DOFade(0f, transitionDur).OnComplete(() => bgmSource.Stop());
            }

            StartDissolveLogo();
            SlideUpUI();
            
            MenuManager.Instance.sword.StartSwordUp(-MenuManager.Instance.height / 7, transitionDur);
        }
    }

    private void StartDissolveLogo()
    {
        float dur = transitionDur / 1.2f;
        titleImg.material = Instantiate(dissolveMaterial);
        
        if(titleImg.material.HasProperty("_Cut"))
        {
            titleImg.material.SetFloat("_Cut", 0f);
            titleImg.material.DOFloat(1f, "_Cut", dur).SetEase(Ease.Linear);
        }
    }

    private void SlideUpUI()
    {
        RectTransform titleRect = transform.GetComponent<RectTransform>();
        RectTransform menuRect = MenuManager.Instance.transform.GetComponent<RectTransform>();

        titleRect.DOAnchorPosY(-MenuManager.Instance.height, transitionDur).SetEase(Ease.OutQuad);
        menuRect.DOAnchorPosY(0f, transitionDur).SetEase(Ease.OutQuad);
    }
}