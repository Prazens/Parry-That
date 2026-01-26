using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedProjectile : MonoBehaviour
{
    public Direction direction;
    public Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ParriedProjectileMovement(direction, targetPosition));
    }

    public void Initialize_PP(Vector3 startPosition, Direction moveDirection, Vector3 reachPosition)
    {
        gameObject.transform.position = startPosition;
        direction = moveDirection;
        targetPosition = reachPosition;

    }

    // 임시 테스트용으로 만든 패링된 투사체 움직임
    public IEnumerator ParriedProjectileMovement(Direction moveDirection, Vector3 reachPosition)
    {
        // Debug.Log("패링된 투사체 움직임 시작");
        for (int i = 0; i < 12; i++)
        {
            gameObject.transform.position += gameObject.transform.TransformDirection(Vector3.up) / 10;
            yield return null;
        }

        // 스트라이커까지의 각도 측정
        Vector3 reachVector = gameObject.transform.position - reachPosition;
        gameObject.transform.rotation = Quaternion.Euler(0, 0,
            Mathf.Atan2(reachVector.y, reachVector.x) * Mathf.Rad2Deg + 90);

        for (int i = 0; ; i++)
        {
            gameObject.transform.position += gameObject.transform.TransformDirection(Vector3.up) / 10;

            // 스트라이커를 지나칠 경우 : 파괴
            if (moveDirection == Direction.Up)
            {
                if (gameObject.transform.position.y >= reachPosition.y)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (gameObject.transform.position.y <= reachPosition.y)
                {
                    Destroy(gameObject);
                }
            }

            yield return null;
        }
    }

}
