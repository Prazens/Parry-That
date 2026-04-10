using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public enum AudioTag
{
    None,
    BGM,
    Enemy,
    Player,
    SFX,
    All
}

public class MenuAudioManager : Singleton<MenuAudioManager>
{
    protected override bool DontDestroy => false;
    public AudioSource BGMSound;
    public AudioSource SFXSound;

    // Start is called before the first frame update
    void Start()
    {
        if (BGMSound == null)
        {
            Debug.LogError("MenuAudioManager에 BGMSound AudioSource 컴포넌트가 없습니다");
        }
        if (SFXSound == null)
        {
            Debug.LogError("MenuAudioManager에 SFXSound AudioSource 컴포넌트가 없습니다");
        }
    }

    public void Play(AudioClip audioClip, AudioTag tag, bool loop)
    {
        Debug.Log($"MenuAudioManager Play called with clip: {audioClip?.name ?? "null"}, tag: {tag}, loop: {loop}");
        StartCoroutine(MusicPlay(audioClip, tag, loop));
    }

    public void Stop(AudioTag tag, bool fadeOut = false, float fadeDuration = 1f)
    {
        Debug.Log($"MenuAudioManager Stop called with tag: {tag}, fadeOut: {fadeOut}, fadeDuration: {fadeDuration}");
        if (tag == AudioTag.All)
        {
            if (fadeOut)
            {
                BGMSound.DOFade(0f, fadeDuration).OnComplete(() => BGMSound.Stop());
                SFXSound.DOFade(0f, fadeDuration).OnComplete(() => SFXSound.Stop());
            }
            else
            {
                BGMSound.Stop();
                SFXSound.Stop();
            }
            return;
        }

        AudioSource currentSound = null;

        switch (tag)
        {
            case AudioTag.BGM:
                currentSound = BGMSound;
                break;
            case AudioTag.SFX:
                currentSound = SFXSound;
                break;
        }

        if (currentSound != null)
        {
            if (fadeOut)
            {
                currentSound.DOFade(0f, fadeDuration).OnComplete(() => currentSound.Stop());
            }
            else
            {
                currentSound.Stop();
            }
        }
    }

    /// <summary>
    /// 미리듣기 사운드 재생 코루틴
    /// <para>-1이면 사운드 멈춤</para>
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    IEnumerator MusicPlay(AudioClip audioClip, AudioTag tag, bool loop)
    {
        // if (MenuManager.Instance.currentState != MenuManager.MenuState.StageSelect)
        // {
        //     yield break;
        // }

        AudioSource currentSound = null;

        float volumeMultiplier = PlayerPrefs.GetFloat("masterVolume", 1f);
        switch (tag)
        {
            case AudioTag.BGM:
                volumeMultiplier = PlayerPrefs.GetFloat("bgmVolume", 1f);
                currentSound = BGMSound;
                break;
            case AudioTag.SFX:
                currentSound = SFXSound;
                break;
            default:
                break;
        }

        if (currentSound == null) yield break;
        currentSound.loop = loop;
        currentSound.Stop();

        currentSound.clip = audioClip;

        currentSound.volume = volumeMultiplier;
        Debug.Log($"Playing sound: {audioClip?.name ?? "null"}, Tag: {tag}, Loop: {loop}, Volume: {currentSound.volume}");
        currentSound.Play();
        yield break;
    }

}
