using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraIntro : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup instructionPanel;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private Button smileButton;

    private void Start()
    {
        if (smileButton != null)
            smileButton.interactable = false;

        if (instructionPanel != null)
            StartCoroutine(IntroRoutine());
        else if (smileButton != null)
            smileButton.interactable = true;
    }

    private IEnumerator IntroRoutine()
    {
        instructionPanel.alpha = 0f;
        instructionPanel.gameObject.SetActive(true);
        
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            instructionPanel.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        
        yield return new WaitForSeconds(displayDuration);
        
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            instructionPanel.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        instructionPanel.gameObject.SetActive(false);

        if (smileButton != null)
            smileButton.interactable = true;
    }
}