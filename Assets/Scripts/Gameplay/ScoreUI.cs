using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public GameObject scoreDisplay;
    public GameObject judgeDisplayPrefab;
    public Sprite[] judgeImages = new Sprite[4];
    public GameObject hpDisplay;
    public GameObject[] heartDisplays = new GameObject[10];
    public GameObject heartDisplayPrefab;
    public Sprite[] heartImages = new Sprite[2];
    public ScoreManager scoreManager;

    private readonly string[] judgeStrings = { "BAD", "BLOCKED", "PARRIED", "PERFECT", "PARRIED", "BLOCKED" };

    private Vector3[] initialPosition = new Vector3[2];

    public void Initialize_UI()
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = "0";
        initialPosition[0] = scoreDisplay.transform.position;
        initialPosition[1] = hpDisplay.transform.position;
        DisplayScore(0);

        GameObject heartDisplay;
        for (int i = 0; i < 10; i++)
        {
            heartDisplay = Instantiate(heartDisplayPrefab, hpDisplay.transform);
            heartDisplay.transform.localPosition = new Vector3(27 * (i % 5), -27 * (i / 5), 0);
            
            // // Debug.Log($"Heart Display spawn at: {heartDisplay.transform.position}");

            heartDisplays[i] = heartDisplay;
            heartDisplay.GetComponent<Image>().sprite = heartImages[0];
        }
    }

    public void HideAll()
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = " ";
        foreach (GameObject heartDisplay in heartDisplays)
        {
            Destroy(heartDisplay);
        }
    }

    public void DisplayScore(int score)
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(score);

        StopCoroutine("BounceUp");
        scoreDisplay.transform.position = initialPosition[0];
        StartCoroutine(BounceUp(scoreDisplay));
    }

    public void DisplayHP(int hpValue, bool isHeal = false)
    {
        if (!TutorialManager.isTutorial) 
        {
            if (!isHeal)
            {
                heartDisplays[hpValue].GetComponent<Image>().sprite = heartImages[1];
            }
            else
            {
                heartDisplays[hpValue - 1].GetComponent<Image>().sprite = heartImages[0];
            }

            StopCoroutine("BounceUp");
            hpDisplay.transform.position = initialPosition[1];
            StartCoroutine(BounceUp(hpDisplay));
        }
        
    }

    public void DisplayJudge(int judge, Direction direction)
    {
        Vector3 generatePosition = Vector3.up;
        GameObject judgeDisplay;

        switch (direction)
        {
            case Direction.Up:
                generatePosition = Vector3.up * Screen.height / 6;
                break;

            case Direction.Down:
                generatePosition = Vector3.down * Screen.height / 6;
                break;

            case Direction.Left:
                generatePosition = Vector3.left * 120;
                break;

            case Direction.Right:
                generatePosition = Vector3.right * 120;
                break;
        }

        judgeDisplay = Instantiate(judgeDisplayPrefab);
        judgeDisplay.transform.SetParent(transform);
        judgeDisplay.transform.localScale = new Vector3(.12f, .12f, 0);
        judgeDisplay.transform.position = new Vector3(Screen.width / 2, Screen.height / 2) + generatePosition;
        judgeDisplay.GetComponent<Image>().sprite = judgeImages[math.abs(judge - 3)];
        StartCoroutine(JudgeBounceUp(judgeDisplay));
    }

    IEnumerator BounceUp(GameObject gameObject)
    {
        for (int i = 3; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.up * 19 / 3f * Screen.height / 800;
        }
        for (int i = 9; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.down * i * i / 15 * Screen.height / 800;
            yield return null;
        }
        yield break;
    }

    IEnumerator JudgeBounceUp(GameObject gameObject)
    {
        for (int i = 3; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.up * 19 / 3f * Screen.height / 800;
        }
        for (int i = 9; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.down * i * i / 15 * Screen.height / 800;
            yield return null;
        }
        yield return new WaitForSecondsRealtime( 0.4f ); 
        Destroy(gameObject);
        yield break;
    }
}
