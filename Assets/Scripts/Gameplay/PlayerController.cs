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
    public enum MovementMode { Free3D, SideScroller }

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
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private bool doubleJumped = false;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float wallJumpForce = 6f;
    [SerializeField] private float wallPushForce = 6f;
    [SerializeField] private LayerMask pipeMask;
    [SerializeField] private float pipeDetectionRange = 2f;
    [SerializeField] private MovementMode currentMode = MovementMode.Free3D;

    private bool isTouchingWall;
    private Vector3 wallNormal;
    private Vector3 sideScrollAxis = Vector3.right;
    private Vector3 sideScrollCameraForward;

    [SerializeField] private GameObject LastCheckPoint;

    private Vector2 moveDirection = Vector2.zero;
    private bool isRunning = false;
    [SerializeField] private float verticalVelocity = 0f;

    [Header("Checks")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded = true;

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
        CheckWall();
        HandlePipe();
    }

    // ---- Movement Handling ----

    private void HandleMovement()
    {

        moveDirection = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move;

        if (currentMode == MovementMode.SideScroller)
        {
            move = -sideScrollAxis * moveDirection.x;
        }
        else
        {
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            move = camForward * moveDirection.y + camRight * moveDirection.x;
        }

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

        HandleJump();
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

    private void HandleJump()
    {
        if (inputActions.Player.Jump.WasPressedThisFrame())
        {
            if (isGrounded)
            {
                verticalVelocity = jumpForce;
                doubleJumped = false;
                animator.SetTrigger("Jump");
            }
            else if (isTouchingWall)
            {
                verticalVelocity = wallJumpForce;

                Vector3 pushDir = wallNormal;
                controller.Move(pushDir * wallPushForce * Time.deltaTime);

                animator.SetTrigger("Jump");
            }
            else if (!doubleJumped)
            {
                verticalVelocity = jumpForce * 1.5f;
                doubleJumped = true;
                animator.SetTrigger("Jump");
            }
        }
    }

    private void CalculateVerticalVelocity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // keeps the player grounded
        }

        // apply gravity over time
        verticalVelocity += Physics.gravity.y * mass * Time.deltaTime;
    }
    private void UpdateGroundedState()
    {
        // Le CharacterController capsule démarre à controller.center
        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.9f;
        float distance = groundCheckDistance;

        // SphereCast pour detecter le sol
        isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, groundMask);
        if (isGrounded)
        {
            doubleJumped = false;
        }
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

    public void Bounce(float force)
    {
        verticalVelocity = force;
        animator.SetTrigger("Jump");
    }

    void CheckWall()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, wallCheckDistance, wallMask))
        {
            isTouchingWall = true;
            wallNormal = hit.normal;
        }
        else
        {
            isTouchingWall = false;
        }
    }

    void HandlePipe()
    {
        if (!inputActions.Player.Crouch.WasPressedThisFrame())
            return;

        Debug.Log("Enterred");
        Collider[] hits = Physics.OverlapSphere(transform.position, pipeDetectionRange, pipeMask);

        foreach (Collider hit in hits)
        {
            Pipe pipe = hit.GetComponent<Pipe>();

            if (pipe != null && pipe.CanEnter(transform))
            {
                Transform exit = pipe.GetExit();

                controller.enabled = false;
                transform.position = exit.position + new Vector3(0, 2, 0);
                controller.enabled = true;

                break;
            }
        }
    }

    public void RespawnPlayer()
    {
        controller.enabled = false;
        transform.position = LastCheckPoint.gameObject.transform.position;
        controller.enabled = true;
    }

    public void ChangeCheckPoint(GameObject newCheckPoint)
    {
        LastCheckPoint = newCheckPoint;
    }

    public void SetMovementMode(MovementMode newMode, Vector3? axis = null)
    {
        currentMode = newMode;

        if (newMode == MovementMode.SideScroller && axis.HasValue)
        {
            sideScrollAxis = axis.Value.normalized;
        }
    }
}
