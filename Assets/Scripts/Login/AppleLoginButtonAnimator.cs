using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AppleLoginButtonAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button appleLoginButton;
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

    private Vector3 _originalScale;
    private bool _isRunning;

    private void Awake()
    {
        if (appleLoginButton == null)
            appleLoginButton = GetComponent<Button>();

        if (buttonRect == null && appleLoginButton != null)
            buttonRect = appleLoginButton.GetComponent<RectTransform>();

        if (buttonRect != null)
            _originalScale = buttonRect.localScale;

        appleLoginButton.onClick.AddListener(OnButtonPressed);
    }

    private void OnDestroy()
    {
        if (appleLoginButton != null)
            appleLoginButton.onClick.RemoveListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        if (_isRunning) return;

        // TODO: put your real login validation here if needed.
        bool loginSuccess = true;

        if (!loginSuccess)
        {
            // show "Invalid login" as you already do.
            return;
        }

        StartCoroutine(PlayPressAndTransition());
    }

    private IEnumerator PlayPressAndTransition()
    {
        _isRunning = true;
        appleLoginButton.interactable = false;

        // 1. Press – quick scale down
        yield return ScaleTo(pressScale, pressDuration);

        // 2. Bounce – slightly overshoot then back to 1
        yield return ScaleTo(bounceScale, bounceDuration * 0.6f);
        yield return ScaleTo(1f,          bounceDuration * 0.4f);

        // 3. Fade out login panel
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

        // 4. Go to loading scene → humanoid
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
            // smooth easing
            t = t * t * (3f - 2f * t); // SmoothStep
            buttonRect.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }

        buttonRect.localScale = end;
    }
}
