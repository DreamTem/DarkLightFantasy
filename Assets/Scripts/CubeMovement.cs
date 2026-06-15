using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 6f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;
        Vector3 vel = rb.linearVelocity;
        vel.x = input.x * moveSpeed;
        vel.z = input.z * moveSpeed;
        rb.linearVelocity = vel;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
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
