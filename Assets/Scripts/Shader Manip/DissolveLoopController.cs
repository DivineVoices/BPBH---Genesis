using UnityEngine;

public class DissolveLoopController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetObject;

    [Header("Shader properties")]
    [SerializeField] private string cutoffHeightPropertyName = "_CutoffHeight";
    [SerializeField] private string noiseStrengthPropertyName = "_NoiseStrength";

    [Header("Cutoff values")]
    [SerializeField] private float cutoffHeightMinimum = 0f;
    [SerializeField] private float cutoffHeightMaximum = 10f;

    [Tooltip("Durée d'une phase (apparition ou disparition)")]
    [SerializeField] private float phaseDurationSeconds = 1f;

    [Header("Noise")]
    [SerializeField] private float noiseStrengthValue = 1f;

    [Header("Loop")]
    [Tooltip("Pause entre chaque phase (fin apparition / fin disparition)")]
    [SerializeField] private float pauseBetweenPhasesSeconds = 1f;

    private Renderer targetRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    private int cutoffHeightPropertyId;
    private int noiseStrengthPropertyId;

    // Progression de la phase : 0 => début, 1 => fin
    private float phaseProgress01;

    // Temps de pause restant avant de relancer une phase
    private float remainingPauseSeconds;

    // true : on va de min -> max (apparition)
    // false : on va de max -> min (disparition)
    private bool isAppearingPhase;

    private void Awake()
    {
        targetRenderer = targetObject.GetComponentInChildren<Renderer>();

        materialPropertyBlock = new MaterialPropertyBlock();

        cutoffHeightPropertyId = Shader.PropertyToID(cutoffHeightPropertyName);
        noiseStrengthPropertyId = Shader.PropertyToID(noiseStrengthPropertyName);

        // État initial : l'objet est caché, et on se prépare à le faire apparaître
        phaseProgress01 = 0f;
        remainingPauseSeconds = 0f;
        isAppearingPhase = true;

        targetObject.SetActive(false);

        // On applique une valeur initiale
        ApplyShaderValues(cutoffHeightMinimum);
    }

    private void Update()
    {
        HandlePauseTimer();

        if (remainingPauseSeconds > 0f)
        {
            return;
        }

        EnsureObjectIsActiveWhenAppearing();

        AdvancePhaseProgress();

        float currentCutoffHeight = Mathf.Lerp(cutoffHeightMinimum, cutoffHeightMaximum, phaseProgress01);
        ApplyShaderValues(currentCutoffHeight);

        CheckEndOfPhaseAndSwitchIfNeeded();
    }

    private void HandlePauseTimer()
    {
        if (remainingPauseSeconds <= 0f)
        {
            return;
        }

        remainingPauseSeconds -= Time.deltaTime;
        if (remainingPauseSeconds < 0f)
        {
            remainingPauseSeconds = 0f;
        }
    }

    private void EnsureObjectIsActiveWhenAppearing()
    {
        if (isAppearingPhase && !targetObject.activeSelf)
        {
            targetObject.SetActive(true);
            phaseProgress01 = 0f;
            ApplyShaderValues(cutoffHeightMinimum);
        }
    }

    private void AdvancePhaseProgress()
    {
        float safePhaseDuration = Mathf.Max(0.0001f, phaseDurationSeconds);

        float progressDelta = Time.deltaTime / safePhaseDuration;

        if (isAppearingPhase)
        {
            phaseProgress01 += progressDelta; // 0 -> 1
        }
        else
        {
            phaseProgress01 -= progressDelta; // 1 -> 0
        }

        phaseProgress01 = Mathf.Clamp01(phaseProgress01);
    }

    private void CheckEndOfPhaseAndSwitchIfNeeded()
    {
        // Fin de phase d'apparition : on passe en phase de disparition + pause
        if (isAppearingPhase && phaseProgress01 >= 1f)
        {
            isAppearingPhase = false;
            remainingPauseSeconds = pauseBetweenPhasesSeconds;
            return;
        }

        // Fin de phase de disparition : on désactive l'objet + on repasse en apparition + pause
        if (!isAppearingPhase && phaseProgress01 <= 0f)
        {
            targetObject.SetActive(false);
            isAppearingPhase = true;
            remainingPauseSeconds = pauseBetweenPhasesSeconds;
        }
    }

    private void ApplyShaderValues(float cutoffHeight)
    {
        // On récupère les valeurs actuelles du PropertyBlock
        targetRenderer.GetPropertyBlock(materialPropertyBlock);

        // On met à jour les propriétés du shader
        materialPropertyBlock.SetFloat(cutoffHeightPropertyId, cutoffHeight);
        materialPropertyBlock.SetFloat(noiseStrengthPropertyId, noiseStrengthValue);

        // On ré-applique au Renderer
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
