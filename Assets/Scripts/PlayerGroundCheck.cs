using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    public bool isGrounded;
    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position, groundDistance, groundLayer);

        Debug.Log(isGrounded);
    }
}
