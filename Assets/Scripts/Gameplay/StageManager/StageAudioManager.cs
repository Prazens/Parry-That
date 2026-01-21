using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager에 있던 오디오 관련 필드/함수만 분리.
/// (원래 스크립트에 없던 함수는 추가하지 않음)
///
/// 포함:
/// - musicSource, musicOffset, musicPlayed, savedMusicTime
/// - RestartAudio(float RollBackTime)
/// - ResetAudio()
/// - AudioPause()
/// - AudioUnPause()
/// </summary>
public class StageAudioManager : MonoBehaviour
{
    [SerializeField] public AudioSource musicSource;

    public float musicOffset;     // PlayerPrefs에서 로드
    public bool musicPlayed = false;

    private float savedMusicTime;

    private void Awake()
    {
        // 기존 StageManager.Awake() 그대로
        musicOffset = PlayerPrefs.GetFloat("musicOffset", 2);
        if (musicSource != null)
        {
            musicSource.volume = PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("bgmVolume", 1);
        }
    }

    // 튜토리얼에서 노래 n초 전으로 되돌리는 용도의 함수 (기존 StageManager.RestartAudio 그대로)
    public void RestartAudio(float RollBackTime)
    {
        if (musicSource == null) return;

        if (musicSource.isPlaying)
        {
            float newTime = Mathf.Max(musicSource.time - RollBackTime, 0f);
            musicSource.Stop();
            musicSource.time = newTime;
            musicSource.Play();
        }
    }

    // 기존 StageManager.ResetAudio 그대로
    public void ResetAudio()
    {
        if (musicSource == null) return;

        musicSource.time = 0f;
        musicSource.Play();
        musicPlayed = true;
    }

    // 기존 StageManager의 PauseStage에서 하던 "시간 저장 + Pause"를 여기로 이동
    public void AudioPause()
    {
        if (musicSource == null) return;
        if (!musicPlayed) return;

        savedMusicTime = musicSource.time;
        musicSource.Pause();
    }

    // 기존 StageManager의 ResumeStage(ResumeAfterDelay)에서 하던 "시간 복원 + Play"를 여기로 이동
    public void AudioUnPause()
    {
        if (musicSource == null) return;
        if (!musicPlayed) return;

        musicSource.time = savedMusicTime;
        musicSource.Play();
    }
}
