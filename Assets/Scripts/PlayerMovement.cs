using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Cam;
    Rigidbody rb;
    public PlayerGroundCheck groundCheck;
    public bool RequestJump =  false;
    public float spd = 10f;
  
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame,

    private void Update()
    {
        if (groundCheck.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            RequestJump = true;     
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        rb.linearVelocity = move * spd + new Vector3(0,rb.linearVelocity.y,0);
        
        
        if (RequestJump) 
        {
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            RequestJump = false;
        }
    }
    private void LateUpdate()
    {
        
    }
}
