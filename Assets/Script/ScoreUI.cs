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
    public GameObject hpDisplay;
    public GameObject judgeDisplayPrefab;
    public Sprite[] judgeImages = new Sprite[4];
    public ScoreManager scoreManager;

    private readonly string[] judgeStrings = { "HIT", "GUARD", "BOUNCE", "PARFECT", "BOUNCE", "GUARD" };

    private Vector3[] initialPosition = new Vector3[2];

    public void Initialize_UI()
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = "0";
        initialPosition[0] = scoreDisplay.transform.position;
        initialPosition[1] = hpDisplay.transform.position;
        DisplayScore(0);
        DisplayHP(100);
    }

    public void DisplayScore(int score)
    {
        scoreDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(score);

        StopCoroutine("TextBounceUp");
        scoreDisplay.transform.position = initialPosition[0];
        StartCoroutine(TextBounceUp(scoreDisplay));
    }

    public void DisplayHP(int hpValue)
    {
        hpDisplay.GetComponent<TextMeshProUGUI>().text = Convert.ToString(hpValue) + "/" + "100";

        StopCoroutine("TextBounceUp");
        hpDisplay.transform.position = initialPosition[1];
        StartCoroutine(TextBounceUp(hpDisplay));
    }

    public void DisplayJudge(int judge, Direction direction)
    {
        Vector3 generatePosition = Vector3.up;
        GameObject judgeDisplay;

        switch (direction)
        {
            case Direction.Up:
                generatePosition = Vector3.up * 100;
                break;

            case Direction.Down:
                generatePosition = Vector3.down * 100;
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

        judgeDisplay.transform.position = new Vector3(240, 400) + generatePosition;
        judgeDisplay.GetComponent<Image>().sprite = judgeImages[math.abs(judge - 3)];
        StartCoroutine(JudgeBounceUp(judgeDisplay));
    }

    IEnumerator TextBounceUp(GameObject gameObject)
    {
        for (int i = 3; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.up * 19 / 3f;
        }
        for (int i = 9; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.down * i * i / 15;
            yield return null;
        }
        yield break;
    }

    IEnumerator JudgeBounceUp(GameObject gameObject)
    {
        for (int i = 3; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.up * 19 / 3f;
        }
        for (int i = 9; i >= 1; i--)
        {
            gameObject.transform.position += Vector3.down * i * i / 15;
            yield return null;
        }
        yield return new WaitForSeconds( 0.4f ); 
        Destroy(gameObject);
        yield break;
    }
}
