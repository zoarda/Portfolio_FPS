using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveAcceleration = 18f;
    [SerializeField] private float maxGroundSpeed = 5f;

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
        Move();

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
    private void Move()
    {
        Vector3 direction =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        direction.Normalize();

        Vector3 horizontalVelocity =
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude < maxGroundSpeed)
        {
            rb.AddForce(
                direction * moveAcceleration,
                ForceMode.Acceleration
            );
        }
    }

    private void Jump()
    {
        if (!IsGrounded)
            return;

        Vector3 velocity = rb.linearVelocity;

        velocity.y = jumpVelocity;

        rb.linearVelocity = velocity;
    }

    private void CheckGround()
    {
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