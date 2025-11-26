using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LipSyncBlendshape : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public SkinnedMeshRenderer faceRenderer;
    public int mouthBlendshapeIndex = 6;   

    [Header("Sensitivity Settings")]
    public float loudnessMultiplier = 800f;
    public float maxMouthOpen = 100f;
    public float smoothSpeed = 20f;

    private const int SAMPLE_SIZE = 256;
    private readonly float[] samples = new float[SAMPLE_SIZE];
    private float currentWeight;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (faceRenderer == null || audioSource == null)
            return;

        float targetWeight = 0f;

        if (audioSource.isPlaying)
        {
            audioSource.GetOutputData(samples, 0);
            
            float sum = 0f;
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }

            float loudness = sum / SAMPLE_SIZE;
            
            targetWeight = Mathf.Clamp(loudness * loudnessMultiplier, 0f, maxMouthOpen);
        }
        
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * smoothSpeed);

        faceRenderer.SetBlendShapeWeight(mouthBlendshapeIndex, currentWeight);
    }
}