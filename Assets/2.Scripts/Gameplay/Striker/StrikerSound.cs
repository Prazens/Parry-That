using UnityEngine;

public class StrikerSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    //준비 효과음
    [SerializeField] private AudioClip prepareSoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip prepareSoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //패링 효과음
    [SerializeField] private AudioClip parrySoundNormal;  // 일반 공격 준비 효과음 (type 0)
    [SerializeField] private AudioClip parrySoundStrong;  // 강한 공격 준비 효과음 (type 1)
    //패링 효과음
    [SerializeField] private AudioClip holdingSound;  // 홀드 중
    [SerializeField] private AudioClip holdingEnd;  // 홀드 끝

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void SetHoldingSound(AudioClip clip)
    {
        holdingSound = clip;
    }

    public void PlayPrepareNormal()
    {
        if (audioSource == null || prepareSoundNormal == null) return;
        audioSource.PlayOneShot(prepareSoundNormal, GetEffectiveEnemyVolume());
    }

    public void PlayPrepareStrong()
    {
        if (audioSource == null || prepareSoundStrong == null) return;
        audioSource.PlayOneShot(prepareSoundStrong, GetEffectiveEnemyVolume());
    }

    public void PlayParryNormal()
    {
        if (audioSource == null || parrySoundNormal == null) return;
        audioSource.PlayOneShot(parrySoundNormal, GetEffectivePlayerVolume());
    }

    public void PlayParryStrong()
    {
        if (audioSource == null || parrySoundStrong == null) return;
        audioSource.PlayOneShot(parrySoundStrong, GetEffectivePlayerVolume());
    }

    public void PlayHoldStart()
    {
        if (audioSource == null || holdingSound == null) return;
        audioSource.PlayOneShot(holdingSound, GetEffectivePlayerVolume());
    }

    public void PlayHoldEnd()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        if (holdingEnd == null) return;
        audioSource.PlayOneShot(holdingEnd, GetEffectivePlayerVolume());
    }

    private float GetEffectiveEnemyVolume()
    => PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("enemyVolume", 1);

    private float GetEffectivePlayerVolume()
        => PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1);
}
