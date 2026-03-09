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

    [Header("Player Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float Width, Height;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private bool doubleJumped = false;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallJumpForce = 6f;
    [SerializeField] private float wallPushForce = 6f;
    [SerializeField] private LayerMask pipeMask;
    [SerializeField] private float pipeDetectionRange = 2f;
    [SerializeField] private MovementMode currentMode = MovementMode.Free3D;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpLockDuration = 0.2f;

    private bool isTouchingWall;
    private Vector3 wallNormal;
    private Vector3 sideScrollAxis = Vector3.right;
    private Vector3 sideScrollCameraForward;

    private Vector3 wallHorizontalVelocity = Vector3.zero;
    private float wallJumpLockTimer = 0f;

    [SerializeField] private GameObject LastCheckPoint;

    private Vector2 moveDirection = Vector2.zero;
    private bool isRunning = false;
    [SerializeField] private float verticalVelocity = 0f;

    [Header("Checks")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded = true;

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

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.deltaTime;
    }

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

        animator.SetBool("IsRunning", isRunning && move != Vector3.zero);
        animator.SetBool("IsWalking", move != Vector3.zero);

        CalculateVerticalVelocity();
        HandleJump();

        if (wallJumpLockTimer <= 0f)
            wallHorizontalVelocity = Vector3.Lerp(wallHorizontalVelocity, Vector3.zero, Time.deltaTime * 5f);

        Vector3 finalMovement;

        finalMovement = move * currentSpeed + wallHorizontalVelocity + Vector3.up * verticalVelocity;

        controller.Move(finalMovement * Time.deltaTime);

        Vector3 lookDir = (wallHorizontalVelocity.sqrMagnitude > 0.5f) ? wallHorizontalVelocity : move;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleJump()
    {
        if (!inputActions.Player.Jump.WasPressedThisFrame())
            return;

        if (isGrounded)
        {
            verticalVelocity = jumpForce;
            doubleJumped = false;
            wallHorizontalVelocity = Vector3.zero;
            animator.SetTrigger("Jump");
        }
        else if (isTouchingWall && wallJumpLockTimer <= 0f)
        {
            verticalVelocity = wallJumpForce;
            wallHorizontalVelocity = wallNormal * wallPushForce;
            doubleJumped = false;
            wallJumpLockTimer = wallJumpLockDuration;
            animator.SetTrigger("Jump");
        }
        else if (!doubleJumped)
        {
            verticalVelocity = jumpForce * 1.5f;
            doubleJumped = true;
            animator.SetTrigger("Jump");
        }
    }

    private void CalculateVerticalVelocity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Keeps the player grounded
            wallHorizontalVelocity = Vector3.zero;
        }

        // Apply gravity over time
        verticalVelocity += Physics.gravity.y * mass * Time.deltaTime;
    }

    private void UpdateGroundedState()
    {
        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.9f;
        float distance = groundCheckDistance;

        isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, distance, groundMask);
        if (isGrounded)
        {
            doubleJumped = false;
        }
    }

    void CheckWall()
    {
        //Prevents instantly re-grabbing
        if (wallJumpLockTimer > 0f)
        {
            isTouchingWall = false;
            return;
        }

        Vector3[] directions = new Vector3[]
        {
            transform.forward,
            -transform.forward,
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, wallCheckDistance)
                && hit.collider.CompareTag("WallJump"))
            {
                isTouchingWall = true;
                wallNormal = hit.normal;
                return;
            }
        }

        isTouchingWall = false;
        wallNormal = Vector3.zero;
    }

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
                    case "Pipe":
                        interactable = closestInteractable.GetComponent<IInteractable>();
                        interactable?.OnInteract();
                        Debug.Log("Pipe");
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

        c.a = targetAlpha;
        image.color = c;
    }

    public void Bounce(float force)
    {
        verticalVelocity = force;
        animator.SetTrigger("Jump");
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