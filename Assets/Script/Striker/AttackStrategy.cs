using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    // 공격 인터페이스

    public void InitStrategy();

    public void NormalReady(AttackType attackType);
    public void NormalAttack(AttackType attackType);
    public void HoldReady();
    public void HoldAttack();
    public void HoldEndReady();
    public void HoldEndAttack();

    public void NormalHit(AttackType attackType);
    public void NormalFail(AttackType attackType);
    public void HoldHit();
    public void HoldFail();
}

public class DrumAttStrategy : IAttack
{
    // 드럼몬 공격 클래스
    // 원거리 공격, 투사체 발사

    private StrikerGeneral executerStriker = null;
    private Queue<GameObject> exclamationQueue = new Queue<GameObject>();
    private Queue<GameObject> projectiveQueue = new Queue<GameObject>();

    public void InitStrategy()
    {

    }
    

    public void NormalReady(AttackType attackType)
    {
        
    }

    public void NormalAttack(AttackType attackType)
    {

    }

    public void HoldReady()
    {

    }

    public void HoldAttack()
    {

    }
    
    public void HoldEndReady()
    {

    }

    public void HoldEndAttack()
    {

    }
}

public class GuitarAttStrategy : IAttack
{
    // 기타몬 공격 클래스
    // 근접 공격, 스트라이커에게도 블레이드 존재

    private StrikerGeneral executerStriker = null;
    private Queue<GameObject> exclamationQueue = new Queue<GameObject>();

    public void InitStrategy()
    {

    }
    

    public void NormalReady(AttackType attackType)
    {
        
    }

    public void NormalAttack(AttackType attackType)
    {

    }

    public void HoldReady()
    {

    }

    public void HoldAttack()
    {

    }
    
    public void HoldEndReady()
    {
        
    }

    public void HoldEndAttack()
    {

    }
}
