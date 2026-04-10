using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldAttackNoticeHandler : MonoBehaviour, IAttackHandler<AttackNoticeContext>
{
    [SerializeField] private GameObject[] exclamations;
    [SerializeField] private AudioClip[] prepareSounds;
    private AudioSource audioSource;
    private Coroutine currentCoroutine = null;

    private void Start()
    {
        var audioSourceObject = GameObject.Find("Audio Source");
        if (audioSourceObject != null)
            audioSource = audioSourceObject.GetComponent<AudioSource>();

        ForceStop();
    }

    public void OnNotice(AttackNoticeContext context)
    {
        var flow = StageFlowManager.Instance;
        float noticeBeat = context.note.noticeBeat;
        float arriveBeat = context.note.arriveBeat;
        float durationSec = flow.BeatToSec(arriveBeat) - flow.BeatToSec(noticeBeat);
        AttackType attackType = (AttackType)context.note.type;

        if (attackType == AttackType.HoldStart)
        {
            Appear(durationSec);
            if (context.judgeables.Count >= 2)
                context.judgeables[1].AddOnDestroy(_ => ForceStop());
        }
        else if (attackType == AttackType.HoldStop)
        {
            Disappear(durationSec);
        }
    }

    void IAttackHandler.OnNotice(IAttackContext context)
        => OnNotice((AttackNoticeContext)context);

    public void OnAttackStart(AttackNoticeContext context)
    {

    }

    void IAttackHandler.OnAttackStart(IAttackContext context)
        => OnAttackStart((AttackNoticeContext)context);

    public void OnJudge(JudgeContext context)
    {

    }

    public void Appear(float durationSec)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(durationSec, true));
    }

    public void Disappear(float durationSec)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(Showing(durationSec, false));
    }

    private IEnumerator Showing(float durationSec, bool isAppear)
    {
        float intervalSec = durationSec / 2.0f;
        for (int i = 0; i < 3; i++)
        {
            bool isAppeared = exclamations[i].activeSelf;
            exclamations[i].SetActive(isAppear);
            if (i < 2)
            {
                if (isAppear && !isAppeared)
                {
                    audioSource.PlayOneShot(prepareSounds[i], PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("enemyVolume", 1));
                }
                else if (!isAppear && isAppeared)
                {
                    audioSource.PlayOneShot(prepareSounds[1 - i], PlayerPrefs.GetFloat("masterVolume", 1) * PlayerPrefs.GetFloat("enemyVolume", 1));
                }

                yield return new WaitForSeconds(intervalSec);
            }
        }
    }

    public void ForceStop()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        exclamations[0].SetActive(false);
        exclamations[1].SetActive(false);
        exclamations[2].SetActive(false);
    }
}
