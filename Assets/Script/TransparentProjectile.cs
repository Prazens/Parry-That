using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentProjectile : MonoBehaviour
{
    public Transform target; // Player Transform
    public AudioSource musicSource; // 음악 소스
    public float speed = 5.0f; // 투사체 속도

    private void Update()
    {
        if (target == null) return;

        // 투사체를 플레이어로 이동
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 플레이어와의 거리 계산(투사체 사라지는거 perfect로 이후 변경 필요!)
        if (Vector3.Distance(transform.position, target.position) <= 0.6f)
        {
            PlayMusic();
            Destroy(gameObject); // 투사체 삭제
        }
    }

    private void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play(); // 음악 재생
            Debug.Log("Music started!");
        }
    }
}
