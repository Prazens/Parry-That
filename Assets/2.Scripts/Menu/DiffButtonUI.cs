using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static MenuManager;

public class DiffButtonUI : MonoBehaviour
{
    [SerializeField] private Sprite normalButton;
    [SerializeField] private Sprite hardButton;
    [SerializeField] private GameObject modeButtonObj;
    [SerializeField] private AudioClip fireOnSound;
    [SerializeField] private AudioClip fireOffSound;
    private Button modeButton;
    private int difficulty;  // 0: Normal, 1: Hard

    public void InitUI(int difficulty)
    {
        // modeButton = modeButtonObj.GetComponent<Button>();

        // modeButton.onClick.AddListener(WhenButtonClicked);

        if (difficulty == 0)
        {
            modeButtonObj.GetComponent<Image>().sprite = normalButton;
            this.difficulty = 0;
        }
        else
        {
            modeButtonObj.GetComponent<Image>().sprite = hardButton;
            this.difficulty = 1;
        }
    }

    public void SetVisibility(bool isVisible)
    {
        if (isVisible)
        {
            modeButtonObj.SetActive(true);
        }
        else
        {
            modeButtonObj.SetActive(false);
        }
    }

    public void WhenButtonClicked()
    {
        Debug.Log("DiffButton clicked");
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
        {
            return;
        }

        difficulty = 1 - difficulty;  // Toggle between 0 and 1

        if (difficulty == 0)
        {
            modeButtonObj.GetComponent<Image>().sprite = normalButton;
            MenuAudioManager.Instance.Play(fireOffSound, AudioTag.SFX, false);
        }
        else
        {
            modeButtonObj.GetComponent<Image>().sprite = hardButton;
            MenuAudioManager.Instance.Play(fireOnSound, AudioTag.SFX, false);
        }
        MenuManager.Instance.UpdateCurStage(MenuManager.Instance.stageIndex[0], MenuManager.Instance.stageIndex[1] == 0 ? 1 : 0);
    }
}
