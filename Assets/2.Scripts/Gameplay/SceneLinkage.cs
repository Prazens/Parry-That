using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLinkage : MonoBehaviour
{
    public static int StageLV = 1;
    public static bool isNormal = true;  // true: normal, false: hard

    // [stageIndex, difficulty]
    public static int[] stageIndex = new int[] { 0, 0 };

    private void Awake()
    {
        if (FindObjectsOfType<SceneLinkage>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }

    public static int[] ConvertToNewStageIndex(int oldIndex)
    {
        switch (oldIndex)
        {   
            case < 5:
                return new int[] { oldIndex, 0 };
            case 5:
                return new int[] { 4, 1 };
            case 6:
                return new int[] { 1, 1 };
            case 7:
                return new int[] { 2, 1 };
            case 8:
                return new int[] { 3, 1 };
            case 9:
                return new int[] { 5, 0 };
            default:
                Debug.LogError($"Invalid oldIndex: {oldIndex}");
                return new int[] { 0, 0 };
        }
    }

    public static int ConvertToOldIndex(int stageIndex, int difficulty)
    {
        switch (stageIndex)
        {
            case 0:
                return 0;
            case 1:
                return difficulty == 0 ? 1 : 6;
            case 2:
                return difficulty == 0 ? 2 : 7;
            case 3:
                return difficulty == 0 ? 3 : 8;
            case 4:
                return difficulty == 0 ? 4 : 5;
            case 5:
                return 9;
            default:
                Debug.LogError($"Invalid stageIndex and difficulty: {stageIndex}, {difficulty}");
                return 0;
        }
    }
}
