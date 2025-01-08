using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public ScoreManager scoreManager;
    [SerializeField] private int hp;
    public Direction currentDirection = Direction.Up;  // 쉴드 방향
    public GameObject shield;
    public bool isShieldMoving = false;

    private Vector3[] directionMove = { Vector3.zero, Vector3.up, Vector3.down, Vector3.left, Vector3.right };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 쉴드 움직임
    public void ShieldMove(Direction direction)
    {
        // 방향 변화가 있을 때
        if (direction != currentDirection && direction != Direction.None)
        {
            shield.transform.position = directionMove[(int)direction] / 2;
            currentDirection = direction;

            if (direction == Direction.Left || direction == Direction.Right)
            {
                shield.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                shield.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        // 쉴드가 튀는 움직임을 보이지 않을 때
        if (!isShieldMoving)
        {
            StartCoroutine(ShieldBounce(direction));
        }
    }

    // 쉴드가 튀는 듯한 움직임
    IEnumerator ShieldBounce(Direction direction)
    {
        isShieldMoving = true;
        shield.transform.position += directionMove[(int)currentDirection] / 10;
        yield return new WaitForSeconds(0.1f);
        shield.transform.position -= directionMove[(int)currentDirection] / 10;
        isShieldMoving = false;
        yield break;
    }
}
