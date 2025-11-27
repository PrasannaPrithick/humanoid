using System.Collections;
using UnityEngine;

public class EyebrowExpression : MonoBehaviour
{
    [Header("Eye / Brow Meshes")]
    [SerializeField] private SkinnedMeshRenderer eyeRenderer;   
    [SerializeField] private SkinnedMeshRenderer browRenderer; 

    [Header("Eye BlendShape Indices")]
    [SerializeField] private int eyeIndex1  = 0;  
    [SerializeField] private int eyeIndex2  = 1;   

    [Header("Brow BlendShape Indices")]
    [SerializeField] private int browIndex1 = 0;  
    [SerializeField] private int browIndex2 = 1;  

    [Header("Audio (for talking expression)")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Talking Expression")]
    [SerializeField] private float talkMaxWeight = 70f;  
    [SerializeField] private float smooth = 6f;          

    [Header("Blink Settings (quick squint blink)")]
    [SerializeField] private float blinkMaxWeight = 100f;
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 5f;
    [SerializeField] private float blinkDuration = 0.12f;

    private float talkWeight;
    private float blinkWeight;
    private Coroutine blinkRoutine;

    private void OnEnable()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        ApplyWeights(0f);
    }

    private void Update()
    {
        float targetTalk = (voiceSource != null && voiceSource.isPlaying) ? talkMaxWeight : 0f;
        talkWeight = Mathf.Lerp(talkWeight, targetTalk, smooth * Time.deltaTime);
        
        float combined = Mathf.Clamp(talkWeight + blinkWeight, 0f, 100f);
        ApplyWeights(combined);
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float wait = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(wait);

            float half = blinkDuration * 0.5f;
            float t = 0f;

            // close / squint
            while (t < half)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / half);
                blinkWeight = Mathf.Lerp(0f, blinkMaxWeight, k);
                yield return null;
            }

            // open / relax
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / half);
                blinkWeight = Mathf.Lerp(blinkMaxWeight, 0f, k);
                yield return null;
            }

            blinkWeight = 0f;
        }
    }

    private void ApplyWeights(float w)
    {
        SetWeight(eyeRenderer, eyeIndex1, w);
        SetWeight(eyeRenderer, eyeIndex2, w * 0.7f);
        
        SetWeight(browRenderer, browIndex1, w);
        SetWeight(browRenderer, browIndex2, w * 0.7f);
    }

    private void SetWeight(SkinnedMeshRenderer renderer, int index, float weight)
    {
        if (renderer == null || renderer.sharedMesh == null) return;
        if (index < 0 || index >= renderer.sharedMesh.blendShapeCount) return;

        renderer.SetBlendShapeWeight(index, weight);
    }
}
