using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidPlugin : MonoBehaviour
{
    private AndroidJavaObject ajo;

    private static AndroidPlugin instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ajo = new AndroidJavaObject(className: "com.example.unityvibration.UnityVibration");
        }
        else
        {
            Destroy(gameObject);
        }
    }   

    public void Vibrate(long duration, int amplitude)
    {
        ajo.Call(methodName: "vibration", duration, amplitude);
    }

    // Vibrate(duration: x, amplitude: y);
    // -> x ms 길이, 세기 y (0~255)의 진동 생성함

    // 진동 적용할 부분 작성
    // 아래 ex: 터치할 때마다 진동 생성
    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vibrate(duration: 100, amplitude: 128);
        }
    }
}
