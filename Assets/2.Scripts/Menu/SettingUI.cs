using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private GameObject SettingPanel;
    private bool settingOn = false;
    private MenuManager.MenuState prevState;
    

    // 버튼 누를 때 작동, 초기 슬라이더 값 세팅, 세팅 패널 온오프
    public void Setting()
    {
        settingOn = settingOn ? false : true;
        if (settingOn)
        {
            SettingPanel.SetActive(true);
            SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("bgmOffset", 0f) * 100;
            SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("masterVolume", 1f) * 20;
            SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("bgmVolume", 1f) * 20;
            SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("enemyVolume", 1f) * 20;
            SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("playerVolume", 1f) * 20;
            ChangeBgmOffset();
            ChangeMasterVolume();
            ChangeBGMVolume();
            ChangeEnemyVolume();
            ChangePlayerVolume();
            
            prevState = MenuManager.Instance.currentState;
            MenuManager.Instance.currentState = MenuManager.MenuState.Settings;
            MenuManager.Instance.diskSwipeUI.gameObject.GetComponent<ScrollRect>().enabled = false;
            MenuManager.Instance.diskSwipeUI.StartPreviewSound(-1);
        }
        else
        {
            SettingPanel.SetActive(false);
            MenuManager.Instance.currentState = prevState;
            MenuManager.Instance.diskSwipeUI.gameObject.GetComponent<ScrollRect>().enabled = true;

            // 막스테이지는 에필로그, 노래 재생 안함 << 이거 하드코딩, 에필로그 조건 추가 필요
            if (MenuManager.Instance.currentState == MenuManager.MenuState.StageSelect || StageSelection.SelectedStageId != StageDBManager.Instance.stageNumbers - 1)
            {
                MenuManager.Instance.diskSwipeUI.StartPreviewSound(StageSelection.SelectedStageId);
            }
        }
    }

    public void InitUI()
    {
        // 세팅 판넬 프리팹 소환하기

        // 안보이게 하기
        // SettingCanvas.GetComponent<Canvas>().sortingOrder = 10;
        SettingPanel.SetActive(false);

        // 초기값 세팅
        // Debug.Log($"PPInit, {PlayerPrefs.GetInt("isPPInited", -1)}");
        if (PlayerPrefs.GetInt("isPPInited", 0) != 1)
        {
            PlayerPrefs.SetFloat("bgmOffset", 0f);
            PlayerPrefs.SetFloat("masterVolume", 1f);
            PlayerPrefs.SetFloat("bgmVolume", 1f);
            PlayerPrefs.SetFloat("enemyVolume", 1f);
            PlayerPrefs.SetFloat("playerVolume", 1f);
            PlayerPrefs.SetInt("isPPInited", 1);
        }
    }

    public void ChangeBgmOffset()
    {
        int sliderValue = (int)SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("bgmOffset", sliderValue / 100f);
        SettingPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{((sliderValue >= 0) ? "+" : "")}{sliderValue}";
    }

    public void ChangeMasterVolume()
    {
        // Debug.Log("ChangeMasterVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("masterVolume", sliderValue / 20f);
        // SettingPanel.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeBGMVolume()
    {
        // Debug.Log("ChangeBGMVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("bgmVolume", sliderValue / 20f);
        // SettingPanel.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeEnemyVolume()
    {
        // Debug.Log("ChangeEnemyVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("enemyVolume", sliderValue / 20f);
        // SettingPanel.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangePlayerVolume()
    {
        // Debug.Log("ChangePlayerVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("playerVolume", sliderValue / 20f);
        // SettingPanel.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }
}
