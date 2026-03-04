using UnityEngine;

public class Mask_Controller : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material vignetteMaterial;

    [Header("Name parameter")]
    [SerializeField] private string vignettePowerPropertyName = "_VignettePower";

    [Header("Value")]
    [SerializeField] private float vignettePowerMinimum = 0.0f;
    [SerializeField] private float vignettePowerMaximum = 1.0f;

    [Header("Timer")]
    [SerializeField] private float timeToReachTarget = 1.0f;

    [Header("Activation")]
    [SerializeField] private bool isVignetteEnabled = false;

    private float currentVignettePower = 0.0f;

    private void Start()
    {
        currentVignettePower = isVignetteEnabled ? vignettePowerMinimum : vignettePowerMaximum;
        ApplyVignettePowerToMaterial(currentVignettePower);
    }

    private void Update()
    {
        float targetVignettePower = isVignetteEnabled ? vignettePowerMaximum : vignettePowerMinimum;

        if (timeToReachTarget <= 0.0f)
        {
            currentVignettePower = targetVignettePower;
            ApplyVignettePowerToMaterial(currentVignettePower);
            return;
        }

        float step = Time.deltaTime / timeToReachTarget;
        currentVignettePower = Mathf.Lerp(currentVignettePower, targetVignettePower, step);

        ApplyVignettePowerToMaterial(currentVignettePower);
    }

    private void ApplyVignettePowerToMaterial(float vignettePowerValue)
    {
        if (vignetteMaterial.HasProperty(vignettePowerPropertyName))
        {
            vignetteMaterial.SetFloat(vignettePowerPropertyName, vignettePowerValue);
        }
    }

    private void OnValidate()
    {
        float previewValue = isVignetteEnabled ? vignettePowerMaximum : vignettePowerMinimum;
        vignetteMaterial.SetFloat(vignettePowerPropertyName, previewValue);
    }

    public bool GetIsVignetteEnabled()
    {
        return isVignetteEnabled;
    }

    public void SetIsVignetteEnabled(bool newValue)
    {
        isVignetteEnabled = newValue;
    }
}
