using System.Collections;
using UnityEngine;

public class SimpleVisemeLipSync : MonoBehaviour
{
    [Header("Smile")]
    public int smileIndex1 = 0;
    public int smileIndex2 = 1;
    public float smileMax = 60f;
    public float baseSmileWeight = 30f;           // idle smile amount

    [Header("References")]
    public AudioSource audioSource;
    public SkinnedMeshRenderer faceRenderer;

    [Header("Blendshape Indices (mouth shapes)")]
    public int indexA = 6;  // “A” shape
    public int indexI = 7;  // “I” shape
    public int indexE = 9;  // “E” shape
    public int indexO = 10; // “O” shape

    [Header("Mouth Settings")]
    public float maxOpen = 90f;
    public float blendSpeed = 20f;

    [Header("Timing")]
    public float timeScale = 1f;

    [Header("Word timing (0–1 over clip length)")]
    [Range(0f, 1f)] public float haiEndRatio = 0.25f;
    [Range(0f, 1f)] public float howEndRatio = 0.50f;
    [Range(0f, 1f)] public float areEndRatio = 0.75f;
    [Range(0f, 1f)] public float youEndRatio = 0.90f;

    [Header("Idle Smile Intro")]
    public bool autoIdleSmile = true;
    public float idleSmileDelay = 2f;

    Coroutine routine;

    float currentSmile1, currentSmile2;
    float currentA, currentI, currentE, currentO;

    void Start()
    {
        if (autoIdleSmile)
        {
            StartCoroutine(IdleSmileRoutine());
        }
    }

    IEnumerator IdleSmileRoutine()
    {
        ApplyAll(0f, 0f, 0f, 0f, 0f, 0f);

        yield return new WaitForSeconds(idleSmileDelay);
        
        ApplyAll(0f, 0f, 0f, 0f, baseSmileWeight, baseSmileWeight * 0.6f);
    }

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

        
        audioSource.Stop();
        audioSource.Play();

        float clipLen = audioSource.clip.length;
        if (clipLen <= 0f) yield break;

        float tHaiEnd = clipLen * haiEndRatio;
        float tHowEnd = clipLen * howEndRatio;
        float tAreEnd = clipLen * areEndRatio;
        float tYouEnd = clipLen * youEndRatio;
        float tEnd    = clipLen;

        float elapsed = 0f;
        
        ApplyAll(0f, 0f, 0f, 0f, baseSmileWeight, baseSmileWeight * 0.6f);

        while (elapsed < tEnd)
        {
            elapsed += Time.deltaTime * timeScale;
            float t = Mathf.Clamp(elapsed, 0f, tEnd);

            float targetA = 0f, targetI = 0f, targetE = 0f, targetO = 0f;

            if (t <= tHaiEnd)
            {
                // "Hai" → A + I
                float phase = Mathf.InverseLerp(0f, tHaiEnd, t);
                targetA = maxOpen * phase;
                targetI = maxOpen * phase * 0.7f;
            }
            else if (t <= tHowEnd)
            {
                // "How" → O
                float phase = Mathf.InverseLerp(tHaiEnd, tHowEnd, t);
                targetO = maxOpen * phase;
            }
            else if (t <= tAreEnd)
            {
                // "are" → E
                float phase = Mathf.InverseLerp(tHowEnd, tAreEnd, t);
                targetE = maxOpen * phase;
            }
            else
            {
                // "you" → hold O till near end
                float phase = Mathf.InverseLerp(tAreEnd, tEnd, t);
                targetO = maxOpen * Mathf.Lerp(1f, 0.3f, phase); // start to soften
            }

            
            float s = blendSpeed * Time.deltaTime;

            currentA = Mathf.Lerp(currentA, targetA, s);
            currentI = Mathf.Lerp(currentI, targetI, s);
            currentE = Mathf.Lerp(currentE, targetE, s);
            currentO = Mathf.Lerp(currentO, targetO, s);

            currentSmile1 = Mathf.Lerp(currentSmile1, smileTarget, s);
            currentSmile2 = Mathf.Lerp(currentSmile2, smileTarget * 0.6f, s);

            ApplyCurrent();
            yield return null;
        }

        
        float closeTime = 0.2f;
        float timer = 0f;

        float startA = currentA, startI = currentI, startE = currentE, startO = currentO;
        float startSmile1 = currentSmile1, startSmile2 = currentSmile2;

        while (timer < closeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / closeTime);

            float a = Mathf.Lerp(startA, 0f, t);
            float i = Mathf.Lerp(startI, 0f, t);
            float e = Mathf.Lerp(startE, 0f, t);
            float o = Mathf.Lerp(startO, 0f, t);

            float s1 = Mathf.Lerp(startSmile1, baseSmileWeight, t);
            float s2 = Mathf.Lerp(startSmile2, baseSmileWeight * 0.6f, t);

            ApplyAll(a, i, e, o, s1, s2);
            yield return null;
        }

        
        ApplyAll(0f, 0f, 0f, 0f, baseSmileWeight, baseSmileWeight * 0.6f);

        routine = null;
    }

    void ApplyCurrent()
    {
        ApplyAll(currentA, currentI, currentE, currentO, currentSmile1, currentSmile2);
    }

    void ApplyAll(float a, float i, float e, float o, float smile1, float smile2)
    {
        if (faceRenderer == null || faceRenderer.sharedMesh == null) return;

        if (indexA >= 0) faceRenderer.SetBlendShapeWeight(indexA, a);
        if (indexI >= 0) faceRenderer.SetBlendShapeWeight(indexI, i);
        if (indexE >= 0) faceRenderer.SetBlendShapeWeight(indexE, e);
        if (indexO >= 0) faceRenderer.SetBlendShapeWeight(indexO, o);

        if (smileIndex1 >= 0) faceRenderer.SetBlendShapeWeight(smileIndex1, smile1);
        if (smileIndex2 >= 0) faceRenderer.SetBlendShapeWeight(smileIndex2, smile2);

        currentA = a; currentI = i; currentE = e; currentO = o;
        currentSmile1 = smile1; currentSmile2 = smile2;
    }
}
