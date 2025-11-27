using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginButtonAnimator  : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button loginButton;
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private CanvasGroup loginCanvasGroup;

    [Header("Animation")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float pressDuration = 0.06f;
    [SerializeField] private float bounceScale = 1.04f;
    [SerializeField] private float bounceDuration = 0.10f;
    [SerializeField] private float fadeDuration = 0.20f;

    [Header("Flow")]
    [SerializeField] private string nextSceneName = "Scene_Humanoid";

    [Header("Login Logic")]
    [SerializeField] private GoogleLoginManager googleLoginManager;
    [SerializeField] private GameObject errorTextObject;

    private Vector3 _originalScale;
    private bool _isRunning;

    private void Awake()
    {
        if (loginButton == null)
            loginButton = GetComponent<Button>();

        if (buttonRect == null && loginButton != null)
            buttonRect = loginButton.GetComponent<RectTransform>();

        if (buttonRect != null)
            _originalScale = buttonRect.localScale;

        if (loginButton != null)
            loginButton.onClick.AddListener(OnButtonPressed);

        if (errorTextObject != null)
            errorTextObject.SetActive(false);
    }
    
    

    private void OnDestroy()
    {
        if (loginButton != null)
            loginButton.onClick.RemoveListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        if (_isRunning) return;

        if (googleLoginManager == null)
        {
            Debug.LogWarning("LoginButtonAnimator: GoogleLoginManager not assigned.");
            return;
        }

        loginButton.interactable = false; 
        googleLoginManager.StartGoogleSignIn(OnLoginCompleted);
    }




    private void OnLoginCompleted(bool success, string message)
    {
        Debug.Log("Login completed: success=" + success + " message=" + message);

        if (success)
        {
            _isRunning = true;
            StartCoroutine(PlayPressAndTransition());
        }
        else
        {
            loginButton.interactable = true;

            if (errorTextObject != null)
                errorTextObject.SetActive(true);
        }
    }



    private IEnumerator PlayPressAndTransition()
    {
        yield return ScaleTo(pressScale, pressDuration);
        yield return ScaleTo(bounceScale, bounceDuration * 0.6f);
        yield return ScaleTo(1f,          bounceDuration * 0.4f);

        if (loginCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = loginCanvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                loginCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            loginCanvasGroup.interactable = false;
            loginCanvasGroup.blocksRaycasts = false;
        }

        SceneLoader.LoadWithLoadingScreen(nextSceneName);
    }

    private IEnumerator ScaleTo(float targetScale, float duration)
    {
        if (buttonRect == null || duration <= 0f)
            yield break;

        Vector3 start = buttonRect.localScale;
        Vector3 end = _originalScale * targetScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            buttonRect.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        buttonRect.localScale = end;
    }
}
