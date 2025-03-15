using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform cameraTransform; 
    public float moveSpeed = 5f;   
    public float rotateSpeed = 100f; 
   
    public Vector3 cameraOffset = new Vector3(0, 2, -5); 

    public float jumpForce = 5f;      
    private bool isGrounded = true;   
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        // Move the rover forward and backward 
        float moveInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        // Rotate the roverleft and right
        float rotateInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * rotateInput * rotateSpeed * Time.deltaTime);

        // Check for jump input (Space key)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

     
        if (cameraTransform != null)
        {
            cameraTransform.position = transform.position + cameraOffset;
            cameraTransform.LookAt(transform); 
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }
}
