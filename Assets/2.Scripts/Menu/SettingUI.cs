using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private GameObject SettingCanvas;
    private GameObject SettingBackGround;
    private GameObject SettingPanel;
    private GameObject SettingIcon;
    private bool settingOn = false;
    

    public void Setting()
    {
        settingOn = settingOn ? false : true;
        if (settingOn)
        {
            SettingBackGround.SetActive(true);
            SettingPanel.SetActive(true);
            SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value = (PlayerPrefs.GetFloat("musicOffset", 2f) - 2f) * 100;
            SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("masterVolume", 1f) * 20;
            SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("bgmVolume", 1f) * 20;
            SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("enemyVolume", 1f) * 20;
            SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value = PlayerPrefs.GetFloat("playerVolume", 1f) * 20;
            ChangeMusicOffset();
            ChangeMasterVolume();
            ChangeBGMVolume();
            ChangeEnemyVolume();
            ChangePlayerVolume();
        }
        else
        {
            SettingBackGround.SetActive(false);
            SettingPanel.SetActive(false);
        }
    }

    public void InitUI()
    {
        // 세팅 판넬 프리팹 소환하기

        // 안보이게 하기
        SettingCanvas.GetComponent<Canvas>().sortingOrder = 10;
        SettingBackGround.SetActive(false);

        // 버튼만 표시
        if (!SettingPanel.activeSelf)
        {
            SettingIcon.SetActive(true);
            // if (!MenuManager.Instance.modeChgButtonEnable)
            // {
            //     MenuManager.Instance.modeChgButtonEnable = true;
            //     MenuManager.Instance.modeChgButtonAble = true;
            // }
        }

        // 초기값 세팅
        // Debug.Log($"PPInit, {PlayerPrefs.GetInt("isPPInited", -1)}");
        if (PlayerPrefs.GetInt("isPPInited", 0) != 1)
        {
            PlayerPrefs.SetFloat("musicOffset", 2f);
            PlayerPrefs.SetFloat("masterVolume", 1f);
            PlayerPrefs.SetFloat("bgmVolume", 1f);
            PlayerPrefs.SetFloat("enemyVolume", 1f);
            PlayerPrefs.SetFloat("playerVolume", 1f);
            PlayerPrefs.SetInt("isPPInited", 1);
        }
    }

    public void ChangeMusicOffset()
    {
        // Debug.Log("ChangeMusicOffset");
        int sliderValue = (int)SettingPanel.transform.GetChild(1).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("musicOffset", sliderValue / 100f + 2f);
        SettingPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{((sliderValue >= 0) ? "+" : "")}{sliderValue}";
    }

    public void ChangeMasterVolume()
    {
        // Debug.Log("ChangeMasterVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(2).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("masterVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeBGMVolume()
    {
        // Debug.Log("ChangeBGMVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(3).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("bgmVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangeEnemyVolume()
    {
        // Debug.Log("ChangeEnemyVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(4).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("enemyVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }

    public void ChangePlayerVolume()
    {
        // Debug.Log("ChangePlayerVolume");
        int sliderValue = (int)SettingPanel.transform.GetChild(5).GetChild(2).GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("playerVolume", sliderValue / 20f);
        SettingPanel.transform.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{sliderValue * 5}%";
    }
}
