using UnityEngine;

public class SignalTester : MonoBehaviour
{
    public NoticeAnim targetSignal;
    public float testBPM = 120f;

    void Update()
    {
        // Q: 생성 (Spawn)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 스와이프 테스트를 위해 랜덤 회전 적용
            if (targetSignal.signalType != SignalType.Tap)
            {
                float[] angles = { 0f, 90f, 180f, 270f };
                targetSignal.transform.rotation = Quaternion.Euler(0, 0, angles[Random.Range(0, angles.Length)]);
            }
            targetSignal.SpawnSignal(testBPM);
        }

        // 🔥 S: 패링 직전 (Pre-Hit)
        if (Input.GetKeyDown(KeyCode.S))
        {
            targetSignal.PreHit(testBPM);
        }

        // W: 성공 (Hit)
        if (Input.GetKeyDown(KeyCode.W))
        {
            targetSignal.HitSignal(testBPM, notice => notice.gameObject.SetActive(false));
        }

        // E: 실패 (Miss)
        if (Input.GetKeyDown(KeyCode.E))
        {
            targetSignal.MissSignal(testBPM, notice => notice.gameObject.SetActive(false));
        }
    }
}