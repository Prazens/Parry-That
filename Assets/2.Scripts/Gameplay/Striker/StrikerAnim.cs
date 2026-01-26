using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerAnim : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Animator holdSpriteAnimator = null;
    [SerializeField] private Animator bladeAnimator = null;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetDirection(int direction)
    {
        animator.SetInteger("direction", direction);
        if (bladeAnimator != null)
            bladeAnimator.SetInteger("bladeDirection", direction);
    }

    public void SetAttackType(int attackType)
    {
        animator.SetInteger("attackType", attackType);
        if (bladeAnimator != null)
            bladeAnimator.SetInteger("attackType", attackType);
    }
}
