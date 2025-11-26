using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraIntro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;   
    [SerializeField] private Button haiButton;            

    [Header("Intro Motion")]
    [Tooltip("How long the camera move takes (seconds).")]
    [SerializeField] private float moveDuration = 1.5f;

    [Tooltip("Offset from the final camera position for the intro start.")]
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0f, -0.7f, -2.5f);

    [Tooltip("Starting rotation (in degrees) for the intro.")]
    [SerializeField] private Vector3 startEulerAngles = new Vector3(12f, 0f, 0f);

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        
        targetPosition = cameraTransform.position;
        targetRotation = cameraTransform.rotation;

        
        cameraTransform.position = targetPosition + startPositionOffset;
        cameraTransform.rotation = Quaternion.Euler(startEulerAngles);

        
        if (haiButton != null)
        {
            haiButton.interactable = false;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        float elapsed = 0f;

        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            
            t = t * t * (3f - 2f * t);

            cameraTransform.position = Vector3.Lerp(startPos, targetPosition, t);
            cameraTransform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            yield return null;
        }

        cameraTransform.position = targetPosition;
        cameraTransform.rotation = targetRotation;

        if (haiButton != null)
        {
            haiButton.interactable = true;
        }
    }
}
