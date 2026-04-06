using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeProjectile : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] private AnimatorOverrideController[] directionOverrides;
    [SerializeField] private Animator bladeAnimator;

    private Vector3 startPosition; // 시작 위치
    private Vector3 targetPosition; // 목표 위치

    private float arriveSec; // 도착 시각
    private float duration;

    public void Setup(Direction location, Vector3 startPos, Vector3 targetPos, float arriveSec, AttackType attackType)
    {
        animator.runtimeAnimatorController = directionOverrides[(int)location - 1];
        bladeAnimator.SetInteger("bladeDirection", (int)location);

        transform.position = startPos;

        startPosition = startPos;
        targetPosition = targetPos;
        this.arriveSec = arriveSec;
        duration = arriveSec - StageFlowManager.Instance.currentTime;

        animator.SetInteger("attackType", (int)attackType);
        bladeAnimator.SetInteger("attackType", (int)attackType);

        StartCoroutine(ActAttack());
    }

    private IEnumerator ActAttack()
    {
        // 출발
        StartCoroutine(LerpPosition(startPosition, targetPosition, arriveSec));

        int randomNum = Random.Range(0, 2);
        animator.SetInteger("randomSelector", randomNum);
        bladeAnimator.SetInteger("randomSelector", randomNum);

        animator.SetTrigger("Attack");
        bladeAnimator.SetTrigger("bladePlay");

        // 공격 모션이 끝날 때까지 대기 후 퇴장
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        StartCoroutine(LerpPosition(targetPosition, startPosition, arriveSec + duration));
        animator.SetBool("movingBack", true);
    }

    private IEnumerator LerpPosition(Vector3 start, Vector3 end, float endSec)
    {
        var flow = StageFlowManager.Instance;
        float duration = endSec - flow.currentTime;
        float fractionOfJourney;

        while (flow.currentTime < endSec)
        {
            fractionOfJourney = (arriveSec - flow.currentTime) / duration;
            transform.position = Vector3.Lerp(end, start, fractionOfJourney);
            yield return null;
        }

        transform.position = end;
    }
}
