using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPivot;

    [SerializeField]
    private float sensitivity = 0.15f;

    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        cameraPivot.localRotation =
            Quaternion.Euler(pitch, 0f, 0f);
    }
}