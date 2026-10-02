using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform groundCheck;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sphereRadius = 0.35f;
    [SerializeField] private float castDistance = 0.2f;

    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 50f;

    public bool IsGrounded { get; private set; }
    public bool IsWalkableGround { get; private set; }

    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    public float GroundAngle { get; private set; }

    public RaycastHit GroundHit { get; private set; }

    private void FixedUpdate()
    {
        DetectGround();
    }

    private void DetectGround()
    {
        IsGrounded = false;
        IsWalkableGround = false;
        GroundNormal = Vector3.up;
        GroundAngle = 0f;

        if (groundCheck == null)
            return;

        Vector3 origin =
            groundCheck.position + Vector3.up * sphereRadius;

        bool hit = Physics.SphereCast(
            origin,
            sphereRadius,
            Vector3.down,
            out RaycastHit hitInfo,
            castDistance + sphereRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hit)
            return;

        GroundHit = hitInfo;

        GroundNormal = hitInfo.normal;

        GroundAngle = Vector3.Angle(
            Vector3.up,
            GroundNormal
        );

        IsGrounded = true;

        IsWalkableGround =
            GroundAngle <= maxSlopeAngle;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Vector3 origin =
            groundCheck.position + Vector3.up * sphereRadius;

        Gizmos.DrawWireSphere(
            origin,
            sphereRadius
        );

        Vector3 end =
            origin +
            Vector3.down * (castDistance + sphereRadius);

        Gizmos.DrawLine(origin, end);

        Gizmos.DrawWireSphere(
            end,
            sphereRadius
        );

        if (Application.isPlaying && IsGrounded)
        {
            Gizmos.DrawLine(
                GroundHit.point,
                GroundHit.point + GroundNormal
            );
        }
    }
}