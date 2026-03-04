using UnityEngine;
using System.Collections;

public class DodgeBlur : MonoBehaviour
{
    private Coroutine blurRoutine;

    [SerializeField] Renderer _renderer;
    [SerializeField] MaterialPropertyBlock propertyBlock;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void StartBlurRoutine()
    {
        if (blurRoutine != null)
            StopCoroutine(blurRoutine);

        blurRoutine = StartCoroutine(BlurRoutine());
    }

    IEnumerator BlurRoutine()
    {
            propertyBlock.SetFloat("_Blur", 2f);
            _renderer.SetPropertyBlock(propertyBlock);

            yield return new WaitForSeconds(0.5f); 

            propertyBlock.SetFloat("_Blur", 0f);
            _renderer.SetPropertyBlock(propertyBlock);

    }
}
