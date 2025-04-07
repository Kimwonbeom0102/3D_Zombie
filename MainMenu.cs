using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void StartGame()
    {
        // ù ��° ��(���� ��)���� �̵�
        SceneManager.LoadScene("GameScene"); // "GameScene"�� ���� ���� �̸����� ����
    }

    public void Option()
    {
        SceneManager.LoadScene("Option");
    }
    // Exit ��ư Ŭ�� �� ȣ��
    public void ExitGame()
    {
        // ���� ����
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �����Ϳ��� ���� ����
        #endif
    }
}
