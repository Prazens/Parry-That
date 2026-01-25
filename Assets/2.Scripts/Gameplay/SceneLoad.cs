using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoad : MonoBehaviour
{
    public Slider progressbar;
    public TextMeshProUGUI loadtext;

    public float minimumLoadTime = 0.5f; // �ּ� �ε� �ð�
    private float elapsedTime = 0f; // ��� �ð�

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation operation;
        //추후 클리어 완료 시 컷씬이 재생되지 않게 해야 할 수도?
        if (SceneLinkage.StageLV == 0)
        {
            operation = SceneManager.LoadSceneAsync("Tutorial");
        }
        else
        {
            operation = SceneManager.LoadSceneAsync("CutScene");
        }

    /*
        switch(SceneLinkage.StageLV)
        {
            case 0:
                operation = SceneManager.LoadSceneAsync("Tutorial");
                break;
            case 1:
                if (false)
                // if (DatabaseManager.isStage1Done)
                    operation = SceneManager.LoadSceneAsync("Stage1");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene");
                break;
            case 2:
                if (false)
                // if (DatabaseManager.isStage2Done)
                    operation = SceneManager.LoadSceneAsync("Stage2");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 2");
                break;
            case 3:
                if (false)
                // if (DatabaseManager.isStage3Done)
                    operation = SceneManager.LoadSceneAsync("Beat Master");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 3");
                break;

            case 4:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 4");
                break;

            case 5:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 4");
                break;

            case 6:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("EndScene");
                break;

            case 7:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene");
                break;

            case 8:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 2");
                break;

            case 9:
                if (false)
                    // if (DatabaseManager.isStage4Done)
                    operation = SceneManager.LoadSceneAsync("testScene_Boss");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 3");
                break;
            ///
        

            default:
                // Debug.Log("�������� �ε� ����");
                break;
        }
    */
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float targetProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            progressbar.value = Mathf.Lerp(progressbar.value, targetProgress, Time.deltaTime * 5f);
            loadtext.text = $"{(progressbar.value * 100):F0}%";

            // �ε� �Ϸ� ����
            if (progressbar.value >= 0.999f && targetProgress >= 1f)
            {
                loadtext.text = "Done!";
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
