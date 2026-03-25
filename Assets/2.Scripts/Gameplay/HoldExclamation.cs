using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldExclamation : MonoBehaviour
{
    [SerializeField] private AudioClip[] prepareSounds;
    private AudioSource audioSource;
    private Coroutine currentCoroutine = null;

    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);

        var audioSourceObject = GameObject.Find("Audio Source");
        if (audioSourceObject != null)
            audioSource = audioSourceObject.GetComponent<AudioSource>();
    }

    public void Appear(float durationSec)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(durationSec, true));
    }

    private IEnumerator Showing(float durationSec, bool isAppear)
    {
        float intervalSec = durationSec / 2.0f;
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

                yield return new WaitForSeconds(intervalSec);
            }
        }
    }

    public void Disappear(float durationSec)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(durationSec, false));
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
