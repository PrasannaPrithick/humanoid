using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SmileController : MonoBehaviour
{
    

    
    [SerializeField] private Animator animator;
    [SerializeField] private Button smileButton;
    [SerializeField] private string smileTriggerName = "SmileTrigger";

    // NEW: reference to the viseme lip sync
    [SerializeField] private SimpleVisemeLipSync lipSync;

    private bool isPlaying;

    private void Start()
    {
        smileButton.onClick.AddListener(OnSmilePressed);
    }

    private void OnSmilePressed()
    {
        if (isPlaying) return;
        isPlaying = true;

        // trigger body animation
        if (animator != null)
        {
            animator.SetTrigger(smileTriggerName);
        }

        // start audio + mouth visemes
        if (lipSync != null)
        {
            lipSync.PlayLipSync();
        }

        StartCoroutine(WaitForEnd());
    }

    private IEnumerator WaitForEnd()
    {
        if (lipSync != null && lipSync.audioSource != null)
        {
            while (lipSync.audioSource.isPlaying)
                yield return null;
        }

        isPlaying = false;
    }
}