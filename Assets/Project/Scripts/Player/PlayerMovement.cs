using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundDetector))]
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

    [Header("Ground Adhesion")]
    [SerializeField] private float groundStickForce = 3f;

    [Header("Steep Slope")]
    [SerializeField] private float slopeSlideAcceleration = 4f;
    [SerializeField] private float slopeControlAcceleration = 0.5f;

    private Rigidbody rb;
    private GroundDetector groundDetector;

    private Vector2 moveInput;
    private bool jumpRequested;

    public bool IsGrounded =>
        groundDetector != null &&
        groundDetector.IsGrounded;

    public bool IsWalkableGround =>
        groundDetector != null &&
        groundDetector.IsWalkableGround;

    public float HorizontalSpeed
    {
        get
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;

            return velocity.magnitude;
        }
    }

    public float VerticalVelocity =>
        rb.linearVelocity.y;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();
    }

    private void Update()
    {
        ReadMovementInput();
        ReadJumpInput();
    }

    private void FixedUpdate()
    {
        if (IsGrounded)
        {
            if (IsWalkableGround)
            {
                GroundMovement();
                StickToGround();
            }
            else
            {
                SteepSlopeMovement();
            }
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

        moveInput =
            Vector2.ClampMagnitude(moveInput, 1f);
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


    private void GroundMovement()
    {
        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        inputDirection.Normalize();

        // 將玩家輸入方向投影到斜坡表面
        Vector3 slopeDirection =
            Vector3.ProjectOnPlane(
                inputDirection,
                groundDetector.GroundNormal
            ).normalized;

        Vector3 groundVelocity =
            Vector3.ProjectOnPlane(
                rb.linearVelocity,
                groundDetector.GroundNormal
            );

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Vector3 targetVelocity =
                slopeDirection * maxGroundSpeed;

            Vector3 newGroundVelocity =
                Vector3.MoveTowards(
                    groundVelocity,
                    targetVelocity,
                    groundAcceleration *
                    Time.fixedDeltaTime
                );

            // 保留垂直於地面的速度
            Vector3 normalVelocity =
                Vector3.Project(
                    rb.linearVelocity,
                    groundDetector.GroundNormal
                );

            rb.linearVelocity =
                newGroundVelocity +
                normalVelocity;
        }
        else
        {
            Vector3 newGroundVelocity =
                Vector3.MoveTowards(
                    groundVelocity,
                    Vector3.zero,
                    groundDeceleration *
                    Time.fixedDeltaTime
                );

            Vector3 normalVelocity =
                Vector3.Project(
                    rb.linearVelocity,
                    groundDetector.GroundNormal
                );

            rb.linearVelocity =
                newGroundVelocity +
                normalVelocity;
        }
    }
    private void AirMovement()
    {
        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        inputDirection.Normalize();

        if (inputDirection.sqrMagnitude <= 0.01f)
            return;

        rb.AddForce(
            inputDirection * airAcceleration,
            ForceMode.Acceleration
        );

        Vector3 horizontalVelocity =
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

        if (horizontalVelocity.magnitude > maxAirSpeed)
        {
            Vector3 limitedVelocity =
                horizontalVelocity.normalized *
                maxAirSpeed;

            rb.linearVelocity =
                new Vector3(
                    limitedVelocity.x,
                    rb.linearVelocity.y,
                    limitedVelocity.z
                );
        }
    }


    private void StickToGround()
    {
        // 正在往上移動時不要吸回地面
        if (rb.linearVelocity.y > 0.1f)
            return;

        rb.AddForce(
            -groundDetector.GroundNormal *
            groundStickForce,
            ForceMode.Acceleration
        );
    }

    private void Jump()
    {
        if (!IsGrounded || !IsWalkableGround)
            return;

        Vector3 velocity = rb.linearVelocity;

        velocity.y = jumpVelocity;

        rb.linearVelocity = velocity;
    }
    private void SteepSlopeMovement()
    {
        Vector3 normal = groundDetector.GroundNormal;

        // 計算沿坡面的下坡方向
        Vector3 downhillDirection =
            Vector3.ProjectOnPlane(
                Vector3.down,
                normal
            ).normalized;

        // 強制產生滑坡
        rb.AddForce(
            downhillDirection * slopeSlideAcceleration,
            ForceMode.Acceleration
        );

        // 沒有輸入就單純滑下去
        if (moveInput.sqrMagnitude <= 0.01f)
            return;

        Vector3 inputDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        inputDirection.Normalize();

        // 將輸入投影到坡面
        Vector3 slopeInput =
            Vector3.ProjectOnPlane(
                inputDirection,
                normal
            ).normalized;

        /*
         * 判斷玩家輸入是否正在嘗試「往上坡」。
         *
         * downhillDirection = 下坡
         *
         * Dot < 0
         * 代表輸入方向與下坡方向相反
         * = 正在嘗試往上爬
         */
        float downhillDot =
            Vector3.Dot(
                slopeInput,
                downhillDirection
            );

        // 不允許在不可行走坡面產生上坡推力
        if (downhillDot < 0f)
        {
            // 移除輸入中「上坡」的分量
            slopeInput -=
                downhillDirection * downhillDot;

            if (slopeInput.sqrMagnitude > 0.001f)
            {
                slopeInput.Normalize();
            }
        }

        // 只保留左右控制或往下坡控制
        rb.AddForce(
            slopeInput * slopeControlAcceleration,
            ForceMode.Acceleration
        );
    }
}