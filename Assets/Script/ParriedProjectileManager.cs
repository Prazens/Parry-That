using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedProjectileManager : MonoBehaviour
{
    public GameObject[] prefabUp; 
    public GameObject[] prefabDown; 
    public Transform parriedTusacheParent; 

    public void ParryTusache(Direction targetDirection, int type)
    {
        // Debug.Log("패링된 투사체 함수 호출됨");

        GameObject prefabToSpawn = null;

        // 랜덤으로 사용할 프리팹 선택
        if (targetDirection == Direction.Up)
        {
            if (type == 0) // 약공격
            {
                int randomIndex = UnityEngine.Random.Range(0, 2);
                prefabToSpawn = prefabUp[randomIndex];
            }
            else if (type == 1) // 강공격
            {
                int randomIndex = UnityEngine.Random.Range(2, 4);
                prefabToSpawn = prefabUp[randomIndex];
            }
        }
        else if (targetDirection == Direction.Down)
        {
            if (type == 0) // 약공격
            {
                int randomIndex = UnityEngine.Random.Range(0, 2);
                prefabToSpawn = prefabDown[randomIndex];
            }
            else if (type == 1) // 강공격
            {
                int randomIndex = UnityEngine.Random.Range(2, 4);
                prefabToSpawn = prefabDown[randomIndex];
            }
        }

        if (prefabToSpawn != null)
        {
            GameObject spawnedObject = Instantiate(prefabToSpawn, parriedTusacheParent);
            GameObject InGameScreen = GameObject.Find("InGameScreen");
            if (transform != null)
            {
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
                
            }
            else
            {
                // // Debug.LogError("생성된 오브젝트에 Transform이 없음");
            }

            Animator animator = spawnedObject.GetComponent<Animator>();
            if (animator != null)
            {
                string animationTriggerName = prefabToSpawn.name;
                animator.Play(animationTriggerName);

                float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                StartCoroutine(DestroyAfterAnimation(spawnedObject, animationLength));
            }
            else
            {
                // // Debug.LogError("Animator가 없음");
            }
        }
        else
        {
            // // Debug.LogError("프리팹 선택 못함");
        }
    }

    // 애니메이션 종료 후 오브젝트 삭제
    private System.Collections.IEnumerator DestroyAfterAnimation(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    void SetWorldPositionAndScale(GameObject worldObject, Vector3 position, Vector3 scale)
    {
        if (worldObject == null)
        {
            // Debug.LogError("월드 오브젝트가 유효하지 않습니다.");
            return;
        }
        worldObject.transform.position = position;
        worldObject.transform.localScale = scale;
    }
    public GameObject[] prefabBoss; // 0~1 : 약, 2~3 : 강

    public void ParryTusacheBoss(int type, Transform bossRoot)
    {
        GameObject prefabToSpawn = null;
        if (type == 0) prefabToSpawn = prefabBoss[UnityEngine.Random.Range(0, 2)];
        else prefabToSpawn = prefabBoss[UnityEngine.Random.Range(2, 4)];

        if (prefabToSpawn == null) return;

        GameObject go = Instantiate(prefabToSpawn, parriedTusacheParent);
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
