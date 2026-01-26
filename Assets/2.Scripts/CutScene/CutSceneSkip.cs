using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipButton : MonoBehaviour
{
    [SerializeField] GameObject tmp_Background;  // �ӽ� ���� - �� �ε� �ð����� ����ȭ������ �̸� �ٲ�δ� �뵵

    public void Start()
    {
        if (tmp_Background != null)
        {
            tmp_Background.SetActive(false);    // �ӽ� ����
            // Debug.Log("tmp_Background�� ��Ȱ��ȭ�Ǿ����ϴ�.");

            if (false)
            //if (DatabaseManager.isTutorialDone) // Ʃ�丮�� �̹� �Ϸ����� ���
            {
                tmp_Background.SetActive(true);  // �ӽ� ����
                // Debug.Log("Tutorial �Ϸ��. Main ������ ��ȯ.");
                SceneManager.LoadScene("Main"); // Main ��ȯ
            }
        }
        else
        {
            // Debug.LogError("tmp_Background ����");
        }
    }
    public void SkipButtonOn()
    {
        FindObjectOfType<CutSceneManager>().EndCutScene();
    }
}
