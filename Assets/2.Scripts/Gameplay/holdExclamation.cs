using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class holdExclamation : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] prepareSounds;
    // public AudioClip prepareSoundHoldOut;
    private int phase = 0;
    private Coroutine currentCoroutine = null;


    void Start()
    {
        phase = 0;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);
    }

    public void Appear(float bpm, float intervalBeat)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(bpm, intervalBeat, true));
    }

    private IEnumerator Showing(float bpm, float intervalBeat, bool isAppear)
    {
        for (int i = 0; i < 3; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isAppear);
            if (i < 2)
            {
                if (isAppear)
                {
                    audioSource.PlayOneShot(prepareSounds[i], PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("enemyVolume", 1));
                }
                else
                {
                    audioSource.PlayOneShot(prepareSounds[1 - i], PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("enemyVolume", 1));
                }
            }
            
            yield return new WaitForSeconds(intervalBeat / bpm * 60f);
        }
        yield break;
    }

    public void Disappear(float bpm, float intervalBeat)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(bpm, intervalBeat, false));
    }

    public void ForceStop()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);
    }
}
