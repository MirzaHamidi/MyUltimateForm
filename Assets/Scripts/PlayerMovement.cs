using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Rigidbody rb;
    public float spd = 5f;
  
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame,

    
    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0,v);
        rb.linearVelocity = move * spd + new Vector3(0,rb.linearVelocity.y,0);
    }
}
