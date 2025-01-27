using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMoving : MonoBehaviour
{
    [SerializeField] float m_force = 0f;
    [SerializeField] Vector3 m_offset = Vector3.zero;

    Quaternion m_originRot;

    // Start is called before the first frame update
    void Start()
    {
        m_originRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    StartCoroutine(ShakeAndResetCoroutine());
        //}
    }

    public void CameraShake()
    {
        StartCoroutine(ShakeAndResetCoroutine());
    }

    IEnumerator ShakeAndResetCoroutine()
    {
        Vector3 t_originEuler = transform.eulerAngles;

        for (int i = 0; i < 3; i++) // ��鸲 �ݺ� Ƚ��
        {
            float t_rotX = Random.Range(-m_offset.x, m_offset.x);
            float t_rotY = Random.Range(-m_offset.y, m_offset.y);
            float t_rotZ = Random.Range(-m_offset.z, m_offset.z);

            Vector3 t_randomRot = t_originEuler + new Vector3(t_rotX, t_rotY, t_rotZ);
            Quaternion t_rot = Quaternion.Euler(t_randomRot);

            while (Quaternion.Angle(transform.rotation, t_rot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, t_rot, m_force * Time.deltaTime);
                yield return null;
            }
        }

        // ��鸲 ���� �� �ʱ�ȭ
        while (Quaternion.Angle(transform.rotation, m_originRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_originRot, m_force * Time.deltaTime);
            yield return null;
        }
        transform.rotation = m_originRot; 
    }
}