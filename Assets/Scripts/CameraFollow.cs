using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 3.5f;
    public float height = 1.5f;
    public float mouseSensitivity = 0.08f;
    public float rotationSmooth = 12f;
    public float minPitch = -25f;
    public float maxPitch = 60f;
    public float positionSmooth = 14f;
    public bool lockCursor = true;

    private float yaw;
    private float pitch = 15f;
    private float currentYaw;
    private float currentPitch = 15f;

    void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            currentYaw = yaw;
        }
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float t = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
        currentYaw = Mathf.LerpAngle(currentYaw, yaw, t);
        currentPitch = Mathf.Lerp(currentPitch, pitch, t);

        Quaternion rot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 desiredPos = pivot - rot * Vector3.forward * distance;

        float posT = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPos, posT);
        transform.rotation = rot;
    }

    public float GetYaw() { return currentYaw; }
}
