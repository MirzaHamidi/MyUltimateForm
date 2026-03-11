using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerGroundCheck groundCheck;
    public NoiseMapGeneratorMesh map;

    [Header("Movement")]
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
        HandleMovement();
        HandleJump();

    }

    void HandleMovement()
    {
        Vector3 horizontalMove = moveInput * spd * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + horizontalMove;

        // Y eksenini map kontrolünde dikkate alma
        Vector3 checkPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);

        bool canMove = true;

        if (map != null && map.walkable != null)
        {
            Vector2Int cell = map.WorldToCell(checkPos);

            if (!map.walkable[cell.x, cell.y])
            {
                canMove = false;
            }
        }

        if (canMove)
        {
            rb.MovePosition(targetPos);
        }
    }

    void HandleJump()
    {
        if (requestJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            requestJump = false;
        }
    }
}