using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidPlugin : MonoBehaviour
{
    private AndroidJavaObject ajo;

    private void Awake() =>
        ajo = new AndroidJavaObject(className: "com.example.unityvibration.UnityVibration");

    public void Vibrate(long duration, int amplitude)
    {
        ajo.Call(methodName: "vibration", duration, amplitude);
    }

    // Vibrate(duration: x, amplitude: y);
    // -> x ms ����, ���� y (0~255)�� ���� ������

    // ���� ������ �κ� �ۼ�
    // �Ʒ� ex: ��ġ�� ������ ���� ����
    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vibrate(duration: 100, amplitude: 128);
        }
    }
}
