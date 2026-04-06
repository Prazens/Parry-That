using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedProjectile : MonoBehaviour
{
    public Direction direction;
    public Vector3 targetPosition;

    void Start()
    {
        StartCoroutine(ParriedProjectileMovement(direction, targetPosition));
    }

    public void Initialize_PP(Vector3 startPosition, Direction moveDirection, Vector3 reachPosition)
    {
        transform.position = startPosition;
        direction = moveDirection;
        targetPosition = reachPosition;
    }

    // 임시 테스트용으로 만든 패링된 투사체 움직임
    private IEnumerator ParriedProjectileMovement(Direction moveDirection, Vector3 reachPosition)
    {
        for (int i = 0; i < 12; i++)
        {
            transform.position += transform.TransformDirection(Vector3.up) / 10;
            yield return null;
        }

        // 스트라이커까지의 각도 측정
        Vector3 reachVector = transform.position - reachPosition;
        transform.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(reachVector.y, reachVector.x) * Mathf.Rad2Deg + 90);

        while (true)
        {
            transform.position += transform.TransformDirection(Vector3.up) / 10;

            // 스트라이커를 지나칠 경우 : 파괴
            if (moveDirection == Direction.Up)
            {
                if (transform.position.y >= reachPosition.y)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (transform.position.y <= reachPosition.y)
                {
                    Destroy(gameObject);
                }
            }

            yield return null;
        }
    }

}
