using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 6f;
    public float rotationSpeed = 14f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;
        if (kb != null)
        {
            float x = 0f, y = 0f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            input = new Vector2(x, y);
            if (input.sqrMagnitude > 1f) input.Normalize();
        }

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (cameraTransform != null)
        {
            camForward = cameraTransform.forward;
            camRight = cameraTransform.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();
        }

        Vector3 moveDir = camForward * input.y + camRight * input.x;

        Vector3 vel = rb.linearVelocity;
        vel.x = moveDir.x * moveSpeed;
        vel.z = moveDir.z * moveSpeed;
        rb.linearVelocity = vel;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (kb != null && kb.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Vector3 jv = rb.linearVelocity;
            jv.y = jumpForce;
            rb.linearVelocity = jv;
        }
    }

    void OnCollisionStay(Collision c)
    {
        foreach (var contact in c.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit(Collision c)
    {
        isGrounded = false;
    }
}
