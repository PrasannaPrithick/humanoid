using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Timing")]
    [Tooltip("Minimum time (in seconds) to keep the loading screen visible.")]
    [SerializeField] private float minDisplayTime = 1f;

    [Header("UI Smoothing")]
    [Tooltip("How fast the slider value moves towards the real progress.")]
    [SerializeField] private float fillSpeed = 1.5f;

    private float displayedProgress = 0f;

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading... 0%";
        }

        string nextScene = SceneLoader.NextSceneName;
        if (string.IsNullOrEmpty(nextScene))
        {
            nextScene = "Scene_Humanoid";
        }

        StartCoroutine(LoadSceneAsync(nextScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while (!operation.isDone)
        {
            elapsed += Time.deltaTime;

            // Real target progress from async operation (0..0.9)
            float target = Mathf.Clamp01(operation.progress / 0.9f);

            
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                target,
                fillSpeed * Time.deltaTime
            );

            if (progressSlider != null)
            {
                progressSlider.value = displayedProgress;
            }

            if (loadingText != null)
            {
                int percent = Mathf.RoundToInt(displayedProgress * 100f);
                loadingText.text = $"Loading... {percent}%";
            }

            bool loadReady = operation.progress >= 0.9f;
            bool uiReady   = displayedProgress >= 0.99f;
            bool timeReady = elapsed >= minDisplayTime;

            if (loadReady && uiReady && timeReady)
            {
                yield return new WaitForSeconds(0.1f);

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
