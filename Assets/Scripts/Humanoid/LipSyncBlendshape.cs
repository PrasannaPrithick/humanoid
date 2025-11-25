using UnityEngine;

/// <summary>
/// Simple runtime lip sync:
/// - Samples audio output from an AudioSource
/// - Maps loudness to a mouth-open blendshape
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class LipSyncBlendshape : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [Tooltip("Blendshape index for mouth open / jaw open.")]
    [SerializeField] private int mouthBlendshapeIndex = 0;

    [Header("Sensitivity Settings")]
    [Tooltip("Multiplier for how strongly loudness opens the mouth.")]
    [SerializeField] private float loudnessMultiplier = 800f;

    [Tooltip("Maximum blendshape weight.")]
    [SerializeField] private float maxMouthOpen = 100f;

    [Tooltip("How quickly the mouth follows the loudness (higher = snappier).")]
    [SerializeField] private float smoothSpeed = 20f;

    // Internal buffer for audio samples
    private const int SAMPLE_SIZE = 256;
    private float[] samples = new float[SAMPLE_SIZE];
    private float currentWeight = 0f;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (audioSource == null || faceRenderer == null) return;

        if (!audioSource.isPlaying)
        {
            // Blend back to closed mouth when not speaking
            currentWeight = Mathf.Lerp(currentWeight, 0f, Time.deltaTime * smoothSpeed);
            faceRenderer.SetBlendShapeWeight(mouthBlendshapeIndex, currentWeight);
            return;
        }

        // Get raw audio samples from this source
        audioSource.GetOutputData(samples, 0);

        // Compute average absolute loudness
        float sum = 0f;
        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }

        float loudness = sum / SAMPLE_SIZE;

        // Map loudness to a blendshape weight
        float targetWeight = Mathf.Clamp(loudness * loudnessMultiplier, 0f, maxMouthOpen);

        // Smooth interpolation for less jitter
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * smoothSpeed);

        // Apply to blendshape
        faceRenderer.SetBlendShapeWeight(mouthBlendshapeIndex, currentWeight);
    }
}
