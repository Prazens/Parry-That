using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prologue : MonoBehaviour
{
    [SerializeField] GameObject tmp_Background;  // 임시 구현 - 씬 로딩 시간동안 검정화면으로 미리 바꿔두는 용도

    public void Start()
    {
        if (tmp_Background != null)
        {
            tmp_Background.SetActive(false);    // 임시 구현
            Debug.Log("tmp_Background이 비활성화되었습니다.");


            if (DatabaseManager.isTutorialDone) // 튜토리얼 이미 완료했을 경우
            {
                tmp_Background.SetActive(true);  // 임시 구현
                Debug.Log("Tutorial 완료됨. Main 씬으로 전환.");
                SceneManager.LoadScene("Main"); // Main 전환
            }
        }
        else
        {
            Debug.LogError("tmp_Background 없음");
        }
    }
    public void SkipButtonOn()
    {
        SceneLinkage.StageLV = 0;
        SceneManager.LoadScene("Tutorial");
    }
}
