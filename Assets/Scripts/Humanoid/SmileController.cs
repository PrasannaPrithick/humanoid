using UnityEngine;
using UnityEngine.UI;

public class SmileController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button smileButton;
    [SerializeField] private string smileTriggerName = "SmileTrigger";

    private bool isPlaying;

    private void Start()
    {
        smileButton.onClick.AddListener(OnSmilePressed);
    }

    private void OnSmilePressed()
    {
        if (isPlaying) return;

        isPlaying = true;

        if (animator != null && !string.IsNullOrEmpty(smileTriggerName))
        {
            animator.SetTrigger(smileTriggerName);
        }

        if (audioSource != null)
        {
            audioSource.Play();
            // LipSyncBlendshape listens to this automatically
        }

        // Optional: reset lock after audio ends
        StartCoroutine(ResetAfterAudio());
    }

    private System.Collections.IEnumerator ResetAfterAudio()
    {
        if (audioSource != null)
        {
            while (audioSource.isPlaying)
                yield return null;
        }

        isPlaying = false;
    }
}
