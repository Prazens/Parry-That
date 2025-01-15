using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedProjectileManager : MonoBehaviour
{
    public GameObject parriedProjectilePrefab;
    public GameObject[] strikerLocations;

    // 패링된 투사체 생성 후 움직임
    public void CreateParriedProjectile(Vector3 initialPosition, Direction targetDirection)
    {
        GameObject tempProjectile;
        Vector3 finalPosition = strikerLocations[(int)targetDirection - 1].transform.position;

        // 패링된 투사체 생성
        tempProjectile = Instantiate(parriedProjectilePrefab);

        Debug.Log("패링된 투사체 생성됨");

        tempProjectile.GetComponent<ParriedProjectile>().Initialize_PP(initialPosition, targetDirection, finalPosition);
        
        // 랜덤으로 양 옆중 한 방향으로 발사
        switch (targetDirection)
        {
            case Direction.Up:
                tempProjectile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 2) == 0 ? 45 : -45);
                break;
            
            case Direction.Down:
                tempProjectile.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 2) == 0 ? 135 : -135);
                break;
        }
    }
}
