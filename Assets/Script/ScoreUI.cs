using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public GameObject scoreDisplay;
    public GameObject judgeDisplayPrefab;
    public ScoreManager scoreManager;

    private readonly string[] judgeStrings = { "HIT", "GUARD", "BOUNCE", "PARFECT", "BOUNCE", "GUARD" };

    private Vector3 initialPosition;

    public void Initialize_UI()
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = "0";
        initialPosition = scoreDisplay.transform.position;

    }

    public void DisplayScore(int score)
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(score);

        StopCoroutine("TextBounceUp");
        scoreDisplay.transform.position = initialPosition;
        StartCoroutine(TextBounceUp());
    }

    public void DisplayJudge(int judge, Direction direction)
    {
        Vector3 generatePosition = Vector3.up;
        GameObject judgeDisplay;
        Color color;

        switch (direction)
        {
            case Direction.Up:
                generatePosition = Vector3.up * 100;
                break;

            case Direction.Down:
                generatePosition = Vector3.down * 100;
                break;

            case Direction.Left:
                generatePosition = Vector3.left * 100;
                break;

            case Direction.Right:
                generatePosition = Vector3.right * 100;
                break;
        }

        judgeDisplay = Instantiate(judgeDisplayPrefab);
        judgeDisplay.GetComponent<TextMeshProUGUI>().text = judgeStrings[judge];

        ColorUtility.TryParseHtmlString("#A07FF7", out color);
        judgeDisplay.GetComponent<TextMeshProUGUI>().color = color;
        
        StartCoroutine(JudgeBounceUp(judgeDisplay));
    }

    IEnumerator TextBounceUp()
    {
        scoreDisplay.transform.position += Vector3.down * 49;
        for (int i = 1; i <= 24; i++)
        {
            scoreDisplay.transform.position += Vector3.up * i * i / 100;
            yield return null;
        }
        yield break;
    }

    IEnumerator JudgeBounceUp(GameObject gameObject)
    {
        gameObject.transform.position += Vector3.down * 49;
        for (int i = 1; i <= 24; i++)
        {
            gameObject.transform.position += Vector3.up * i * i / 100;
            yield return null;
        }

        Destroy(gameObject);
        yield break;
    }
}
