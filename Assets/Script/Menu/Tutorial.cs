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
        // 임시 구현
        DatabaseManager.isTutorialDone = true;
        theDatabase.SaveTutorialDone();
        SceneManager.LoadScene("Main");
    }
}
