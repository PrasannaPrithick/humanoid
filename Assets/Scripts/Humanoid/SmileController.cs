using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SmileController : MonoBehaviour
{
    

    
    [SerializeField] private Animator animator;
    [SerializeField] private Button smileButton;
    [SerializeField] private string smileTriggerName = "SmileTrigger";

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

        if (animator != null)
        {
            animator.SetTrigger(smileTriggerName);
        }

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