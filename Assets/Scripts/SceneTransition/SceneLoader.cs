using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // The scene we want to load after the loading screen
    public static string NextSceneName { get; private set; }

    /// <summary>
    /// Call this from any scene to go through the loading screen.
    /// </summary>
    public static void LoadWithLoadingScreen(string nextSceneName)
    {
        NextSceneName = nextSceneName;
        SceneManager.LoadScene("Scene_Loading");
    }
}