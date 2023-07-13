using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private PlayerState playerState;

    private CharacterController controller;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float slidingSpeed;
    [SerializeField] private float gravityMultiplier = 1f;
    private float currentSpeed;
    private float rotationSpeed;
    private float velocity;
    private float gravity = -9.81f;

    [Header("Feet and body")]
    [SerializeField] private float checkSphereRadius;
    [SerializeField] private Transform feet;
    [SerializeField] private Transform body;

    [Header("Layers and Inputs")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameInputs gameInputs;

    private bool isWalking = false;
    private bool isGrounded;
    private bool isSliding;
    private Vector3 slidingDirection;
    private RaycastHit hitInfo;

    private void Start() {
        controller = GetComponent<CharacterController>();
    }

    private void Update() {
        HandleMovement();
        HandleGravity();
        HandleSlopeSliding();

        playerState = GetComponent<PlayerStateManager>().playerState;
        if (playerState == PlayerState.Walking) {
            currentSpeed = moveSpeed;
        }
        if (playerState == PlayerState.Crouching || playerState == PlayerState.CarryingEnemy) {
            currentSpeed = crouchSpeed;
        }
        if (playerState == PlayerState.Running) {
            currentSpeed = runningSpeed;
        }
    }

    private void FixedUpdate() {
        isGrounded = Physics.CheckSphere(feet.position, checkSphereRadius, groundLayer);
        isSliding = OnSlope();
    }

    private void HandleMovement() {
        if (GetComponent<PlayerAnimations>().isAttacking) return;

        Vector2 inputVector = gameInputs.GetMovementVectorNormalized();

        float moveDistance = currentSpeed * Time.deltaTime;
        if (inputVector != Vector2.zero) {
            float targetRotation = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            // Rotate character
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationSpeed, 0.1f);
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;

            moveDir = AdjustVelocityToSlope(moveDir);

            controller.Move(moveDir * moveDistance);
        }


        isWalking = inputVector != Vector2.zero;
    }

    private Vector3 AdjustVelocityToSlope(Vector3 velocity) {
        // align move vector with the ramp
        Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        Vector3 adjustedVeclocity = slopeRotation * velocity;
        if (adjustedVeclocity.y < 0) return adjustedVeclocity;

        return velocity;
    }

    private void HandleGravity() {
        if (isGrounded && velocity < 0f) {
            velocity = 0f;
        } else {
            velocity += gravity;
        }

        controller.Move(new Vector3(0f, velocity, 0f) * gravityMultiplier * Time.deltaTime);
    }

    private void HandleSlopeSliding() {
        if (!isSliding) return;

        slidingDirection = Vector3.ProjectOnPlane(Vector3.down, hitInfo.normal);
        controller.Move(slidingDirection * slidingSpeed * Time.deltaTime);
    }

    private bool OnSlope() {
        if (Physics.SphereCast(body.position, checkSphereRadius, Vector3.down, out hitInfo, 1f, groundLayer)) {
            // Slope Angle
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
            if (angle > controller.slopeLimit) {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(feet.position, checkSphereRadius);
    }

    public void ChangeSpeed(float speed) {
        currentSpeed = speed;
    }

    public bool IsWalking { get { return isWalking; } }
    public bool IsGrounded { get { return isGrounded; } }
    public bool IsSliding { get { return isSliding; } }
}