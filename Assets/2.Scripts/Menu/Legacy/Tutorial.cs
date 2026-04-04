using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    DatabaseManager theDatabase;
    private void Start()
    {
        theDatabase = FindObjectOfType<DatabaseManager>();
    }
    public void SkipButtonOn()
    {
        // �ӽ� ����
        DatabaseManager.isTutorialDone = true;
        theDatabase.SaveTutorialDone();
        SceneManager.LoadScene("testMain");
    }
}
