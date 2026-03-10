using Mono.Cecil;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public PlayerGroundCheck groundCheck;
    public float spd = 10f;
    public float jumpForce = 5f;
    

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool requestJump;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        moveInput = (transform.right * h + transform.forward * v).normalized;

        if (groundCheck != null && groundCheck.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            requestJump = true;
        }
    }
    

    void FixedUpdate()
    {
        Vector3 velocity = moveInput * spd;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (requestJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            requestJump = false;
        }
    }
}