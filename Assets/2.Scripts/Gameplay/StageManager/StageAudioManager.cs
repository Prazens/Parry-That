using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StageManager에 있던 오디오 관련 필드/함수만 분리.
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

    public void ApplyStageData(StageData stageData)
    {
        if (stageData == null) return;

        // BGM 적용(없으면 기존 클립 유지)
        if (musicSource != null && stageData.Bgm != null)
        {
            musicSource.clip = stageData.Bgm;
        }

        // 오프셋 적용: override가 true일 때만 스테이지 값을 사용
        if (stageData.OverrideMusicOffset)
        {
            musicOffset = stageData.MusicOffset;
        }
        // override가 false면 기존대로 PlayerPrefs에서 읽어온 전역 musicOffset 유지
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

    public void StopAudio()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    // 기존 StageManager의 PauseStage에서 하던 "시간 저장 + Pause"를 여기로 이동
    public void AudioPause()
    {
        if (musicSource == null) return;

        savedMusicTime = musicSource.time;
        musicSource.Pause();
    }

    // 기존 StageManager의 ResumeStage(ResumeAfterDelay)에서 하던 "시간 복원 + Play"를 여기로 이동
    public void AudioUnPause()
    {
        if (musicSource == null) return;

        musicSource.time = savedMusicTime;
        musicSource.Play();
    }
}
