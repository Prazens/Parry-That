using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLinkage : MonoBehaviour
{
    public static int StageLV = 0;

    private void Awake()
    {
        if (FindObjectsOfType<SceneLinkage>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }
}
