using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [SerializeField]
    private float gravity = 1.62f;

    private Rigidbody rb;

    public float Gravity => gravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    public void SetGravity(float value)
    {
        gravity = Mathf.Max(0f, value);
    }
}