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

        if (SceneLinkage.StageLV == 0)
        {
            operation = SceneManager.LoadSceneAsync("Stage");
        }
        else
        {
            operation = SceneManager.LoadSceneAsync("CutScene");
        }

        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            float targetProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            progressbar.value = Mathf.Lerp(progressbar.value, targetProgress, Time.deltaTime * 5f);
            loadtext.text = $"{(progressbar.value * 100):F0}%";

            if (progressbar.value >= 0.999f && targetProgress >= 1f)
            {
                loadtext.text = "Done!";
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
