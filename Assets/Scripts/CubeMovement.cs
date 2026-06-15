using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        // Правильный способ читать Input через Input System
        Vector2 moveInput = Keyboard.current != null ?
            new Vector2(
                (Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f) +
                (Keyboard.current.upArrowKey.isPressed ? 1f : Keyboard.current.downArrowKey.isPressed ? -1f : 0f),
                (Keyboard.current.dKey.isPressed ? -1f : Keyboard.current.aKey.isPressed ? 1f : 0f) +
                (Keyboard.current.rightArrowKey.isPressed ? 1f : Keyboard.current.leftArrowKey.isPressed ? -1f : 0f)
            ) : Vector2.zero;

        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 vel = rb.linearVelocity;
        vel.x = input.x * moveSpeed;
        vel.z = input.z * moveSpeed;
        rb.linearVelocity = vel;

        // Прыжок через Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
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