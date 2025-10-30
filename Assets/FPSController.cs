using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpHeight = 1.0f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public float mouseSensitivity = 100f;
    private Transform playerBody;
    private float xRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerBody = transform;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // --- �ٴ� üũ ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // �ٴڿ� �ٰ� ����
        }

        // --- �̵� �Է� ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = playerBody.right * x + playerBody.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // --- ���� ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- �߷� ���� ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- ���콺 ȸ�� ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
