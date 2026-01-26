using UnityEngine;

/// <summary>
/// 싱글톤 패턴을 구현한 제네릭 클래스
/// <para>게임 전체 전역 싱글톤</para>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual bool DontDestroy => true;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (DontDestroy)
            {
                DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
            }
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }
}