using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float speed = 10.0f;         // Camera movement speed
    public float sensitivity = 2.0f;    // Mouse look sensitivity

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Locks cursor to center of screen
        Cursor.visible = false;                    // Hides cursor
    }

    void Update()
    {
        // Mouse look rotation
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += Input.GetAxis("Mouse Y") * sensitivity;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.Euler(-rotationY, rotationX, 0);

        // Camera translation (movement)
        Vector3 direction = new Vector3();
        
        if (Input.GetKey(KeyCode.W)) direction += transform.forward;
        if (Input.GetKey(KeyCode.S)) direction -= transform.forward;
        if (Input.GetKey(KeyCode.A)) direction -= transform.right;
        if (Input.GetKey(KeyCode.D)) direction += transform.right;
        if (Input.GetKey(KeyCode.E)) direction += transform.up;
        if (Input.GetKey(KeyCode.Q)) direction -= transform.up;

        transform.position += direction.normalized * speed * Time.deltaTime;

        // Unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
