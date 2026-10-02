using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float maxGroundSpeed = 5f;
    [SerializeField] private float groundAcceleration = 12f;
    [SerializeField] private float groundDeceleration = 5f;

    [Header("Air Movement")]
    [SerializeField] private float airAcceleration = 1.5f;
    [SerializeField] private float maxAirSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 3.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpRequested;

    public bool IsGrounded { get; private set; }

    public float HorizontalSpeed
    {
        get
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }

    public float VerticalVelocity => rb.linearVelocity.y;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReadMovementInput();
        ReadJumpInput();
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (IsGrounded)
        {
            GroundMovement();
        }
        else
        {
            AirMovement();
        }

        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            moveInput.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            moveInput.y -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput.x += 1f;

        if (Keyboard.current.aKey.isPressed)
            moveInput.x -= 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void ReadJumpInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    // =========================
    // Ground Movement
    // =========================

    private void GroundMovement()
    {
        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        inputDirection.Normalize();

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        // 有輸入：逐漸加速到目標速度
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity =
                inputDirection * maxGroundSpeed;

            Vector3 newVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                groundAcceleration * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector3(
                newVelocity.x,
                rb.linearVelocity.y,
                newVelocity.z
            );
        }
        // 沒輸入：逐漸減速，而不是瞬間停止
        else
        {
            Vector3 newVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                groundDeceleration * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector3(
                newVelocity.x,
                rb.linearVelocity.y,
                newVelocity.z
            );
        }
    }

    // =========================
    // Air Movement
    // =========================

    private void AirMovement()
    {
        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        inputDirection.Normalize();

        if (inputDirection.sqrMagnitude <= 0.01f)
            return;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        // 空中只能輕微修正方向
        rb.AddForce(
            inputDirection * airAcceleration,
            ForceMode.Acceleration
        );

        // 避免 Air Control 無限加速
        horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (horizontalVelocity.magnitude > maxAirSpeed)
        {
            Vector3 limitedVelocity =
                horizontalVelocity.normalized * maxAirSpeed;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }
    }

    // =========================
    // Jump
    // =========================

    private void Jump()
    {
        if (!IsGrounded)
            return;

        Vector3 velocity = rb.linearVelocity;

        velocity.y = jumpVelocity;

        rb.linearVelocity = velocity;
    }

    // =========================
    // Ground Detection
    // =========================

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}