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
            case 1:
                operation = SceneManager.LoadSceneAsync("Stage1");
                break;
            case 2:
                operation = SceneManager.LoadSceneAsync("Stage2");
                break;
            case 3:
                operation = SceneManager.LoadSceneAsync("Beat Master");
                break;
            ///

            default:
                Debug.Log("�������� �ε� ����");
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
