using UnityEngine;
using System.Collections;

public class EnemyBlink : MonoBehaviour
{

    private Coroutine blinkRoutine;

    [SerializeField] Renderer _renderer;
    [SerializeField] MaterialPropertyBlock propertyBlock;

    private float currentBlink = 1f;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void StartBlinkRoutine(float target)
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine(target));
    }

    IEnumerator BlinkRoutine(float target)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float start = currentBlink;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentBlink = Mathf.Lerp(start, 1f, elapsed / duration);

            propertyBlock.SetFloat("_BlinkActive", currentBlink);
            _renderer.SetPropertyBlock(propertyBlock);

            yield return null;
        }
        currentBlink = target;

        duration = 0.5f;
        elapsed = 0f;
        start = currentBlink;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentBlink = Mathf.Lerp(start, 0f, elapsed / duration);

            propertyBlock.SetFloat("_BlinkActive", currentBlink);
            _renderer.SetPropertyBlock(propertyBlock);

            yield return null;
        }
        currentBlink = target;
    }
}
