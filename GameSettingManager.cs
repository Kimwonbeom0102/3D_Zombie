using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameSettingManager : MonoBehaviour
{
    //UI 텍스트
    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    
    public CanvasScaler canvasScaler;

    private int resolutionIndex = 0;  // 해상도
    private int graphicsQualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "800X600", "1280X720", "1920X1080" };
    private string[] graphicqualityOptions = { "Low", "Normal", "High" };

    public GameObject option;

    
    void Start()
    {
        LoadSettings();
        UpdateResolutionText();
        UpdateGraphicsQualityText();
        UpdateScreenText();
    }

    public void OnResolutionLeftClick()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex -1);
        UpdateResolutionText();
    }

    public void OnResolutionRightClick()  
    {
        resolutionIndex = Mathf.Min(resolutionIndex - 1, resolutionIndex + 1);  //최대치와 최소치 구분
        UpdateResolutionText();
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }

    public void onGraphicsLeftClick()
    {
        graphicsQualityIndex = Mathf.Max(0, graphicsQualityIndex - 1);
        UpdateGraphicsQualityText();
    }

    public void onGraphicsRightClick()
    {
        graphicsQualityIndex = Mathf.Min(graphicqualityOptions.Length -1 , graphicsQualityIndex + 1);
        UpdateGraphicsQualityText();
    }

    public void UpdateGraphicsQualityText()
    {
        graphicsQualityText.text = graphicqualityOptions[graphicsQualityIndex];
    }

    public void onFullScreenToggleClick()
    {
        isFullScreen = !isFullScreen;
        UpdateScreenText();
    }

    public void UpdateScreenText()
    {
        fullScreenText.text = "전체화면 : " + (isFullScreen ? "켜짐" : "꺼짐");
    }

    public void ApplySetting()
    {
        string[] res = resolutions[resolutionIndex].Split("x");
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);

        QualitySettings.SetQualityLevel(graphicsQualityIndex);
        
        // 변경하시겠습니까? 문구 저장
        SaveSetting();

    }

    private void SaveSetting()
    {
        PlayerPrefs.SetInt("ResoultionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", graphicsQualityIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResoultionIndex", resolutionIndex);
        graphicsQualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", graphicsQualityIndex);
        isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;  // bool 값이기에 비교연산자 true or false; 1 : 0

    }
    
    public void OptionExitButton()
    {
        option.SetActive(false);    
    }
    
}
