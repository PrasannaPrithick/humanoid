using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    
    public static string NextSceneName { get; private set; }

    public static void LoadWithLoadingScreen(string nextSceneName)
    {
        NextSceneName = nextSceneName;
        SceneManager.LoadScene("Scene_Loading");
    }
}