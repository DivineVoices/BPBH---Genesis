using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Transform camTransform;
    private CharacterController controller;

    private Animator animator;

    [SerializeField] private SphereDetection sphereDetectorRef;
    [SerializeField] private DodgeBlur dodgeBlurRef;
    [SerializeField] private CinemachineShaker cinemachineShakerRef;

    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] float interactDetectionRange = 5f;


    // -- Movement States --

    [Header("Player Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float Width, Height;


    private Vector2 moveDirection = Vector2.zero;
    private bool isRunning = false;
    [SerializeField] private float verticalVelocity = 0f;

    

    [Header("Checks")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded = false;

    // Input System
    [HideInInspector] public InputSystem_Actions inputActions;

    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI respawnText;

    private Coroutine SetAlphaRoutine;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        inputActions = new InputSystem_Actions();
        camTransform = Camera.main.transform;

    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        inputActions.Player.Enable();
        respawnButton.gameObject.SetActive(false);
        respawnText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        inputActions.Disable();

    }

    private void Update()
    {
        UpdateGroundedState();
        HandleMovement();
        HandleLooking();
        HandleInteract();
    }

    // ---- Movement Handling ----

    private void HandleMovement()
    {

        moveDirection = inputActions.Player.Move.ReadValue<Vector2>();

        // Camera-relative movement
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // Flatten camera vectors (no vertical influence)
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * moveDirection.y + camRight * moveDirection.x;
        move.Normalize();

        isRunning = inputActions.Player.Sprint.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        if (isRunning && move != Vector3.zero)
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);
        if (move != Vector3.zero)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
            
        CalculateVerticalVelocity();

        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // Rotation towards movement direction
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
            );
        }
    }

    private void CalculateVerticalVelocity()
    {
        if (isGrounded)
        {
            verticalVelocity = -0.5f; // small negative value to keep grounded
        }

        // appliquer gravité
        verticalVelocity = Physics.gravity.y * mass * Time.deltaTime;

    }

    private void UpdateGroundedState()
    {
        // Le CharacterController capsule démarre à controller.center
        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.9f;
        float distance = groundCheckDistance;

        // SphereCast pour detecter le sol
        isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, groundMask);
    }

    // ---- Looking Handling ----
    private void HandleLooking()
    {
    }

    private void HandleInteract()
    {
        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            GameObject closestInteractable = sphereDetectorRef.SphereDetectClosest(interactLayerMask, interactDetectionRange);
            if (closestInteractable != null)
            {
                IInteractable interactable = null;
                switch (closestInteractable.tag)
                {
                    case "ShaderPedestal":
                        interactable = closestInteractable.GetComponent<IInteractable>();
                        interactable?.OnInteract();
                        Debug.Log("Pedestal");
                        break;
                    case "Untagged":
                        Debug.Log("Nothing");
                        break;
                }
            }
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        // Le CharacterController capsule démarre à controller.center
        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.9f;
        float distance = groundCheckDistance;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * distance, radius);
    }


    public void FreezeDialogueControls()
    {
        inputActions.Disable();
        inputActions.Player.Interact.Enable();
    }

    public void UnfreezeControls()
    {
        inputActions.Player.Enable();
    }

    private void SetAlpha(float targetAlpha, Image image, float duration)
    {
        if (image == null)
            return;

        // Stop any previous fade on this image
        if (SetAlphaRoutine != null)
            StopCoroutine(SetAlphaRoutine);

        SetAlphaRoutine = StartCoroutine(SetAlphaOverTime(targetAlpha, image, duration));
    }

    private IEnumerator SetAlphaOverTime(float targetAlpha, Image image, float duration)
    {
        float startAlpha = image.color.a;
        float time = 0f;

        Color c = image.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = c;

            yield return null;
        }

        // Ensure exact final value
        c.a = targetAlpha;
        image.color = c;
    }

}
