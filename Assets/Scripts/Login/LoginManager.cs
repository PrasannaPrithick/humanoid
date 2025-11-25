using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private Button appleLoginButton;
    [SerializeField] private GameObject invalidText;

    private void Start()
    {
        invalidText.SetActive(false);
        appleLoginButton.onClick.AddListener(OnAppleLoginPressed);
    }

    private void OnAppleLoginPressed()
    {
        bool loginSuccess = true;

        if (loginSuccess)
        {
            SceneLoader.LoadWithLoadingScreen("Scene_Humanoid");
        }
        else
        {
            invalidText.SetActive(true);
        }
    }
}