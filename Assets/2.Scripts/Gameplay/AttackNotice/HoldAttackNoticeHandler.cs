using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldAttackNoticeContext : IAttackContext
{
    public NoteData note { get; }
    public List<Judgeable> judgeables = new();

    public HoldAttackNoticeContext(NoteData note, List<Judgeable> judgeables)
    {
        this.note = note;
        this.judgeables = judgeables;
    }
}

public class HoldAttackNoticeHandler : MonoBehaviour, IAttackHandler<HoldAttackNoticeContext>
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

    public void OnNotice(HoldAttackNoticeContext context)
    {
        var flow = StageFlowManager.Instance;
        float noticeBeat = context.note.noticeBeat;
        float arriveBeat = context.note.arriveBeat;
        float durationSec = flow.BeatToSec(arriveBeat) - flow.BeatToSec(noticeBeat);
        AttackType attackType = (AttackType)context.note.type;

        if (attackType == AttackType.HoldStart)
        {
            Appear(durationSec);
        }
        else if (attackType == AttackType.HoldFinishStrong)
        {
            Disappear(durationSec);
        }


        context.judgeables?[1]?.AddOnDestroy(_ => ForceStop());
    }

    void IAttackHandler.OnNotice(IAttackContext context)
        => OnNotice((HoldAttackNoticeContext)context);

    public void OnAttackStart(HoldAttackNoticeContext context)
    {

    }

    void IAttackHandler.OnAttackStart(IAttackContext context)
        => OnAttackStart((HoldAttackNoticeContext)context);

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
            exclamations[i].SetActive(isAppear);
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
