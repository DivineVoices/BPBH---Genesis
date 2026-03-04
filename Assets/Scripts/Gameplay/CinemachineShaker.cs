using System.Collections;
using TreeEditor;
using Unity.Cinemachine;
using UnityEngine;


public class CinemachineShaker : MonoBehaviour
{
    [SerializeField] private Coroutine shakeRoutine;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;

    public void StartShakeRoutine(float duration, float intensity)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, intensity));
    }

    IEnumerator ShakeRoutine(float duration, float intensity)
    {
        perlin.AmplitudeGain = intensity;
        Debug.Log("Started");
        yield return new WaitForSeconds(duration);
        perlin.AmplitudeGain = 0f;
        Debug.Log("Stopped");
    }
}