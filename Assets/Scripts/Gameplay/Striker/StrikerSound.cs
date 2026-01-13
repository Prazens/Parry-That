using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] public AudioClip holdingSound;  // 홀드 중
    [SerializeField] private AudioClip holdingEnd;  // 홀드 끝

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayHoldStart()
    {
        if (holdingSound == null) return;
        audioSource.PlayOneShot(holdingSound, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
    }

    public void PlayHoldEnd()
    {
        audioSource.Stop();
        if (holdingEnd == null) return;
        audioSource.PlayOneShot(holdingEnd, PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("playerVolume", 1));
    }
}
