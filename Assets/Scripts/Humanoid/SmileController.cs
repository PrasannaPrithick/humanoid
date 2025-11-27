using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SmileController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string smileTriggerName = "Smile";
    [SerializeField] private string idleStateName = "Idle";

    [Header("Audio")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip greetingClip; 

    [Header("UI")]
    [SerializeField] private Button smileButton;

    [Header("Lip Sync")]
    [SerializeField] private SimpleVisemeLipSync lipSync;

    private bool isPlaying;

    private void Awake()
    {
        if (smileButton != null)
        {
            smileButton.onClick.RemoveListener(OnSmileButtonPressed);
            smileButton.onClick.AddListener(OnSmileButtonPressed);
        }
    }

    private void OnDestroy()
    {
        if (smileButton != null)
            smileButton.onClick.RemoveListener(OnSmileButtonPressed);
    }

    private void OnSmileButtonPressed()
    {
        if (isPlaying) return;

        if (greetingClip == null || voiceSource == null || animator == null)
        {
            Debug.LogWarning("SmileController: Missing references.");
            return;
        }

        StartCoroutine(PlayGreetingRoutine());
    }

    private IEnumerator PlayGreetingRoutine()
    {
        isPlaying = true;
        if (smileButton != null) smileButton.interactable = false;

        animator.ResetTrigger(smileTriggerName);
        animator.SetTrigger(smileTriggerName);

        voiceSource.Stop();
        voiceSource.clip = greetingClip;

        if (lipSync != null)
        {
            lipSync.audioSource = voiceSource;
            lipSync.PlayLipSync();
        }
        else
        {
            voiceSource.Play();
        }

        yield return new WaitUntil(() => !voiceSource.isPlaying);

        if (!string.IsNullOrEmpty(idleStateName))
        {
            animator.CrossFade(idleStateName, 0.1f);
        }

        if (smileButton != null) smileButton.interactable = true;
        isPlaying = false;
    }
}
