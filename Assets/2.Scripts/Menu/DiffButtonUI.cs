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
    private int tempDiff;  // gc 방지용(효과가 있는지는 몰루)

    public void InitUI(int difficulty)
    {
        // modeButton = modeButtonObj.GetComponent<Button>();

        // modeButton.onClick.AddListener(WhenButtonClicked);

        UpdateButtonState(difficulty);
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

    public void UpdateButtonState(int difficulty)
    {
        if (difficulty == 0)
        {
            modeButtonObj.GetComponent<Image>().sprite = normalButton;
        }
        else if (difficulty == 1)
        {
            modeButtonObj.GetComponent<Image>().sprite = hardButton;
        }
        else
        {
            Debug.LogWarning($"Invalid difficulty index: {difficulty}. Expected 0 or 1.");
        }
    }

    public void WhenButtonClicked()
    {
        Debug.Log("DiffButton clicked");
        if (MenuManager.Instance.currentState != MenuState.StageSelect)
        {
            return;
        }

        switch (MenuManager.Instance.stageIndex[1])
        {
            case 0:
                tempDiff = 1;  // Easy에서 Hard로 변경
                MenuAudioManager.Instance.Play(fireOffSound, AudioTag.SFX, false);
                break;
            case 1:
                tempDiff = 0;  // Hard에서 Easy로 변경
                MenuAudioManager.Instance.Play(fireOnSound, AudioTag.SFX, false);
                break;
            default:
                Debug.LogWarning($"Invalid difficulty index: {tempDiff}. Expected 0 or 1.");
                break;
        }
        UpdateButtonState(tempDiff);

        MenuManager.Instance.UpdateCurStage(MenuManager.Instance.stageIndex[0], tempDiff);
    }
}
