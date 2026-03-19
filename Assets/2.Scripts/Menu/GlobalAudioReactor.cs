using UnityEngine;

public class GlobalAudioReactor : MonoBehaviour
{
    [Header("BGM 소스")]
    public AudioSource bgmSource;

    [Header("전체 효과 조절")]
    public float multiplier = 10f;  
    public float smoothSpeed = 10f; 

    private float[] spectrumData = new float[256];
    private float currentIntensity = 0f;

    void Update()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            // 1. 소리 분석
            bgmSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
            float bassValue = 0f;
            for (int i = 0; i < 10; i++) 
            {
                bassValue += spectrumData[i];
            }

            // 2. 값 스무딩
            float targetIntensity = bassValue * multiplier;
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);

            // 3. 🌟핵심: 씬 전체의 모든 셰이더에게 "_GlobalAudio" 라는 이름으로 값을 뿌립니다!🌟
            Shader.SetGlobalFloat("_GlobalAudio", currentIntensity);
        }
    }
}