using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManeger : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab; // 투사체 프리팹
    [SerializeField] Transform player; // 플레이어 위치
    public float bpm = 120f; // BPM 설정

    private double currentTime = 0d;
    private StrikerController currentStriker;

    // Update is called once per frame
    void Update()
    {
        if (currentStriker == null) return;

        currentTime += Time.deltaTime;
        if (currentTime >= 60d / bpm)
        {
            SpawnProjectile();
            currentTime -= 60d / bpm;
        }
    }
    private void SpawnProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, currentStriker.transform.position, Quaternion.identity);

        // 투사체의 타겟 설정
        projectile projScript = projectile.GetComponent<projectile>();
        if (projScript != null)
        {
            projScript.target = player;
            projScript.owner = currentStriker; // 소유자 설정
        }
    }

    // Striker 연결
    public void StartSpawning(StrikerController striker)
    {
        currentStriker = striker;
    }
}
