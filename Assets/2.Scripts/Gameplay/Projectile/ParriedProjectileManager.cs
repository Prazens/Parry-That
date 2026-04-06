using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedProjectileManager : MonoBehaviour
{
    public GameObject[] prefabUp; 
    public GameObject[] prefabDown; 
    public GameObject[] prefabBoss; // 0~1 : 약, 2~3 : 강
    public Transform parriedprojectileParent;

    public void ParryProjectile(Direction targetDirection, AttackType attackType, int fixRandom = 0)
    {
        GameObject prefabToSpawn = null;

        // 랜덤으로 사용할 프리팹 선택
        int randomIndex = 0;
        if (attackType == AttackType.Normal)
        {
            randomIndex = Random.Range(0, 2);
            if (fixRandom > 0) randomIndex = fixRandom - 1;
        }
        else if (attackType == AttackType.Strong)
        {
            randomIndex = Random.Range(2, 4);
            if (fixRandom > 0) randomIndex = fixRandom + 1;
        }

        if (targetDirection == Direction.Up)
        {
            prefabToSpawn = prefabUp[randomIndex];
        }
        else if (targetDirection == Direction.Down)
        {
            prefabToSpawn = prefabDown[randomIndex];
        }

        if (prefabToSpawn == null)
            return;

        GameObject spawnedObject = Instantiate(prefabToSpawn, parriedprojectileParent);
        GameObject InGameScreen = GameObject.Find("InGameScreen");

        // 사이즈 및 위치 조절  -- 해상도 관련 문제 해결 시 수정 필요
        if (targetDirection == Direction.Up)
        {
            Vector3 desiredPosition = new Vector3(0, -0.4f, 1);
            Vector3 desiredScale = new Vector3(0.3f, 0.32f, 1);

            SetWorldPositionAndScale(spawnedObject, desiredPosition, desiredScale);
        }
        else if (targetDirection == Direction.Down)
        {
            Vector3 desiredPosition = new Vector3(0, 0, 1);
            Vector3 desiredScale = new Vector3(0.3f, 0.32f, 1);

            SetWorldPositionAndScale(spawnedObject, desiredPosition, desiredScale);
        }

        Animator animator = spawnedObject.GetComponent<Animator>();
        if (animator != null)
        {
            string animationTriggerName = prefabToSpawn.name;
            animator.Play(animationTriggerName);

            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            StartCoroutine(DestroyAfterAnimation(spawnedObject, animationLength));
        }
    }

    // 애니메이션 종료 후 오브젝트 삭제
    private IEnumerator DestroyAfterAnimation(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    private void SetWorldPositionAndScale(GameObject worldObject, Vector3 position, Vector3 scale)
    {
        if (worldObject == null)
        {
            return;
        }
        worldObject.transform.position = position;
        worldObject.transform.localScale = scale;
    }

    public void ParryProjectileBoss(AttackType attackType, Transform bossRoot)
    {
        GameObject prefabToSpawn = null;
        if (attackType == AttackType.Normal) prefabToSpawn = prefabBoss[Random.Range(0, 2)];
        else prefabToSpawn = prefabBoss[Random.Range(2, 4)];

        if (prefabToSpawn == null) return;

        GameObject go = Instantiate(prefabToSpawn, parriedprojectileParent);
        // 화면 공간이 아니라 월드 중심(보스 본체 pivot) 근처로 배치
        SetWorldPositionAndScale(go, bossRoot.position /*+ 오프셋 가능*/, new Vector3(0.3f, 0.32f, 1));

        var animator = go.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(prefabToSpawn.name);
            StartCoroutine(DestroyAfterAnimation(go, animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }
}


/*
public Animator parried_tusache;

private void Start()
{
    parried_tusache = GetComponent<Animator>();
    if (parried_tusache == null) // Debug.Log("애니메이터 컴포넌트 못찾음");
}
public void ParryTusache(Direction targetDirection, int type) // 패링된 탄 애니메이션 시행
{
    // Debug.Log("패링된 투사체 함수 호출됨");
    if (targetDirection == Direction.Up)
    {
        if (type == 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, 2);
            if (randomIndex == 0) parried_tusache.SetTrigger("up1");
            else parried_tusache.SetTrigger("up2");
        }
        else if (type == 1)
        {
            int randomIndex = UnityEngine.Random.Range(0, 2);
            if (randomIndex == 0) parried_tusache.SetTrigger("up3");
            else parried_tusache.SetTrigger("up4");
        }
    }
    else if (targetDirection == Direction.Down)
    {
        if (type == 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, 2);
            if (randomIndex == 0) parried_tusache.SetTrigger("down1");
            else parried_tusache.SetTrigger("down2");
        }
        else if (type == 1)
        {
            int randomIndex = UnityEngine.Random.Range(0, 2);
            if (randomIndex == 0) parried_tusache.SetTrigger("down1");
            else parried_tusache.SetTrigger("down2");
        }
    }

}

*/


/*
public GameObject parriedProjectilePrefab;
public GameObject[] strikerLocations;

// 패링된 투사체 생성 후 움직임
public void CreateParriedProjectile(Vector3 initialPosition, Direction targetDirection)
{
    GameObject tempProjectile;
    Vector3 finalPosition = strikerLocations[(int)targetDirection - 1].transform.position;

    // 패링된 투사체 생성
    tempProjectile = Instantiate(parriedProjectilePrefab);

    // Debug.Log("패링된 투사체 생성됨");

    tempProjectile.GetComponent<ParriedProjectile>().Initialize_PP(initialPosition, targetDirection, finalPosition);

    // 랜덤으로 양 옆중 한 방향으로 발사
    switch (targetDirection)
    {
        case Direction.Up:
            tempProjectile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 2) == 0 ? 60 : -60);
            break;

        case Direction.Down:
            tempProjectile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 2) == 0 ? 120 : -120);
            break;
    }
}
*/
// }
