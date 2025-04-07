using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void StartGame()
    {
        // 첫 번째 씬(게임 씬)으로 이동
        SceneManager.LoadScene("GameScene"); // "GameScene"은 게임 씬의 이름으로 변경
    }

    public void Option()
    {
        SceneManager.LoadScene("Option");
    }
    // Exit 버튼 클릭 시 호출
    public void ExitGame()
    {
        // 게임 종료
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중지
        #endif
    }
}
