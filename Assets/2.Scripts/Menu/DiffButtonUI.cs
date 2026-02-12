using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MenuManager;

public class DiffButtonUI : MonoBehaviour
{
    [SerializeField] private Sprite normalButton;
    [SerializeField] private Sprite hardButton;
    [SerializeField] private GameObject modeButtonObj;
    private Button modeButton;

    public void InitUI(int difficulty)
    {
        modeButton = modeButtonObj.GetComponent<Button>();

        modeButton.onClick.AddListener(WhenButtonClicked);

        if (difficulty == 0)
        {
            modeButtonObj.GetComponent<Image>().sprite = normalButton;
        }
        else
        {
            modeButtonObj.GetComponent<Image>().sprite = hardButton;
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
        if (MenuManager.Instance.currentState == MenuState.StageSelect)
        {
            return;
        }

        MenuManager.Instance.UpdateCurStage(MenuManager.Instance.stageIndex[0], MenuManager.Instance.stageIndex[1] == 0 ? 1 : 0);
    }
}
