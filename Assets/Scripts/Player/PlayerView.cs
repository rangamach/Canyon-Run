using Unity.Android.Gradle;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private PlayerInputAction pia;
    private PlayerController playerController;
    private Animator anim;
    private Rigidbody rb;
    private Camera mainCamera;
    private float moveInput;

    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); 

        SpawnPlayer();
        SetupInput();
        CreateCamera();
    }
    private void SpawnPlayer()
    {
        SetTransform();
        anim.Rebind();
        anim.SetFloat("Speed", 0);
    }
    private void SetTransform()
    {
        transform.position = new Vector3(0, 0, -175f);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    private void SetupInput()
    {
        pia = new PlayerInputAction();

        pia.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        pia.Movement.Move.canceled += ctx => moveInput = 0f;

        pia.Movement.Jump.performed += ctx => Jump();

        pia.Enable();
    }
    private void CreateCamera()
    {
        GameObject camera = new GameObject("MainCamera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>();
        camera.AddComponent<AudioListener>();

        this.mainCamera = camera.GetComponent<Camera>();
        this.mainCamera.fieldOfView = 60f;
        this.mainCamera.nearClipPlane = 0.01f;
        this.mainCamera.farClipPlane = 1000f;
        this.mainCamera.transform.rotation = Quaternion.Euler(20, 0, 0);
        SetCameraTransform();
    }
    private void SetCameraTransform()
    {
        this.mainCamera.transform.position = new Vector3(0f, 15f, transform.position.z - 20f);
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void LateUpdate()
    {
        MoveCamera();
    }
    private void MoveCamera()
    {
        //Vector3 targetPosition = new Vector3(0f, 15f, transform.position.z - 20f);
        Vector3 targetPosition = new Vector3(0f, transform.position.y + 15f, transform.position.z - 20f);

        this.mainCamera.transform.position = targetPosition;
    }

    private void MovePlayer()
    {
        Vector3 position = rb.position;

        // Keep Y from physics (jump)
        float y = position.y;

        // Forward movement
        position.z += runSpeed * Time.fixedDeltaTime;

        // Side movement
        position.x += moveInput * runSpeed * 0.5f * Time.fixedDeltaTime;

        // Clamp X
        position.x = Mathf.Clamp(position.x, -20f, 20f);

        // PUT BACK physics-based Y
        position.y = y;

        rb.MovePosition(position);

        anim.SetFloat("Speed", 1);
    }

    private void Jump()
    {
        bool grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        if (grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetTrigger("Jump");
        }
    }

    public void SetController(PlayerController controller) => this.playerController = controller;
}
