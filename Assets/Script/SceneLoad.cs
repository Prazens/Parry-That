using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoad : MonoBehaviour
{
    public Slider progressbar;
    public TextMeshProUGUI loadtext;

    public float minimumLoadTime = 2.0f; // �ּ� �ε� �ð�
    private float elapsedTime = 0f; // ��� �ð�

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation operation = null;
        switch(SceneLinkage.StageLV)
        {
            case 0:
                operation = SceneManager.LoadSceneAsync("Tutorial");
                break;
            case 1:
                if (DatabaseManager.isStage1Done)
                    operation = SceneManager.LoadSceneAsync("Stage1");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene");
                break;
            case 2:
                if (DatabaseManager.isStage2Done)
                    operation = SceneManager.LoadSceneAsync("Stage2");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 2");
                break;
            case 3:
                if (DatabaseManager.isStage3Done)
                    operation = SceneManager.LoadSceneAsync("Beat Master");
                else
                    operation = SceneManager.LoadSceneAsync("tmpScene 3");
                break;
            ///

            default:
                // Debug.Log("�������� �ε� ����");
                break;
        }
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
