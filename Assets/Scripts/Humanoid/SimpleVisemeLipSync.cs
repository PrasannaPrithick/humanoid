using System.Collections;
using UnityEngine;

public class SimpleVisemeLipSync : MonoBehaviour
{
    
    [Header("Smile")]
    public int smileIndex1 = 0;
    public int smileIndex2 = 1;
    public float smileMax = 60f;
    float currentSmile1, currentSmile2;
    
    
    [Header("References")]
    public AudioSource audioSource;
    public SkinnedMeshRenderer faceRenderer;

    [Header("Blendshape Indices (Unity-chan MTH_DEF)")]
    public int indexA = 6;  
    public int indexI = 7;  
    public int indexE = 9;  
    public int indexO = 10; 

    [Header("Mouth Settings")]
    public float maxOpen = 90f;
    public float blendSpeed = 20f;
    [Header("Timing")]
    public float timeScale = 1f;
    Coroutine routine;
    [Header("Word timing (0–1 over clip length)")]
    [Range(0f, 1f)] public float haiEndRatio = 0.25f;
    [Range(0f, 1f)] public float howEndRatio = 0.50f;
    [Range(0f, 1f)] public float areEndRatio = 0.75f;
    [Range(0f, 1f)] public float youEndRatio = 0.90f;

    public void PlayLipSync()
    {
        if (audioSource == null || audioSource.clip == null || faceRenderer == null)
        {
            Debug.LogWarning("SimpleVisemeLipSync: Missing references.");
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(LipSyncRoutine());
    }

    IEnumerator LipSyncRoutine()
{
    float smileTarget = smileMax; 
    audioSource.Play();

    float clipLen = audioSource.clip.length;
    if (clipLen <= 0f) yield break;

    
    float tHaiEnd = clipLen * haiEndRatio;
    float tHowEnd = clipLen * howEndRatio;
    float tAreEnd = clipLen * areEndRatio;
    float tYouEnd = clipLen * youEndRatio; 
    float tEnd    = clipLen;

    float elapsed = 0f;

    
    SetAll(0f, 0f, 0f, 0f);

    while (elapsed < tEnd)
    {
        elapsed += Time.deltaTime * timeScale;
        float t = Mathf.Clamp(elapsed, 0f, tEnd);

        float a = 0f, i = 0f, e = 0f, o = 0f;

        if (t <= tHaiEnd)
        {
            // Hai: A + I
            float phase = Mathf.InverseLerp(0f, tHaiEnd, t);
            a = maxOpen * phase;
            i = maxOpen * phase * 0.7f;
        }
        else if (t <= tHowEnd)
        {
            // How: O
            float phase = Mathf.InverseLerp(tHaiEnd, tHowEnd, t);
            o = maxOpen * phase;
        }
        else if (t <= tAreEnd)
        {
            // are: E
            float phase = Mathf.InverseLerp(tHowEnd, tAreEnd, t);
            e = maxOpen * phase;
        }
        else
        {
            // you: HOLD O strongly until end of clip
            o = maxOpen;
        }

        ApplySmooth(a, i, e, o, smileTarget);
        yield return null;
    }

    float closeTime = 0.15f;
    float timer = 0f;

    float startA = currentA, startI = currentI, startE = currentE, startO = currentO;

    while (timer < closeTime)
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / closeTime);

        float a = Mathf.Lerp(startA, 0f, t);
        float i = Mathf.Lerp(startI, 0f, t);
        float e = Mathf.Lerp(startE, 0f, t);
        float o = Mathf.Lerp(startO, 0f, t);

        SetAll(a, i, e, o);
        yield return null;
    }

    SetAll(0f, 0f, 0f, 0f);
    routine = null;
}
    

    float currentA, currentI, currentE, currentO;

    void ApplySmooth(float targetA, float targetI, float targetE, float targetO, float targetSmile)
    {
        float s = blendSpeed * Time.deltaTime;

        currentA = Mathf.Lerp(currentA, targetA, s);
        currentI = Mathf.Lerp(currentI, targetI, s);
        currentE = Mathf.Lerp(currentE, targetE, s);
        currentO = Mathf.Lerp(currentO, targetO, s);
        currentSmile1 = Mathf.Lerp(currentSmile1, targetSmile, s);
        currentSmile2 = Mathf.Lerp(currentSmile2, targetSmile * 0.6f, s);

        faceRenderer.SetBlendShapeWeight(indexA, currentA);
        faceRenderer.SetBlendShapeWeight(indexI, currentI);
        faceRenderer.SetBlendShapeWeight(indexE, currentE);
        faceRenderer.SetBlendShapeWeight(indexO, currentO);
        faceRenderer.SetBlendShapeWeight(smileIndex1, currentSmile1);
        faceRenderer.SetBlendShapeWeight(smileIndex2, currentSmile2);
    }


    void SetAll(float a, float i, float e, float o)
    {
        currentA = a; currentI = i; currentE = e; currentO = o;
        faceRenderer.SetBlendShapeWeight(indexA, a);
        faceRenderer.SetBlendShapeWeight(indexI, i);
        faceRenderer.SetBlendShapeWeight(indexE, e);
        faceRenderer.SetBlendShapeWeight(indexO, o);
    }
}
