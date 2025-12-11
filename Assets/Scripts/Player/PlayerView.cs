using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private PlayerInputAction pia;
    private PlayerController playerController;
    private UIService uiService;
    private Animator anim;
    private Rigidbody rb;
    private Camera mainCamera;
    private float moveInput;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpWasPressedLastFrame;
    private float distanceTravelled = 0f;
    private float lastZPosition = 0f;
    public bool hasDied;

    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); 

        SpawnPlayer();
        SetupInput();
        CreateCamera();
    }
    private void Start()
    {
        this.hasDied = true;
        this.isGrounded = true;
        this.uiService = GameService.Instance.UIService();

        lastZPosition = transform.position.z;
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
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("Grounded", isGrounded);

        CheckIfUIButtonPressed();
    }
    private void CheckIfUIButtonPressed()
    {
        if (hasDied) return;

        if ((uiService.IsLeftPressed() && uiService.IsRightPressed()) || (!uiService.IsLeftPressed() && !uiService.IsRightPressed())) moveInput = 0;
        else if (uiService.IsLeftPressed()) moveInput = -1;
        else if (uiService.IsRightPressed()) moveInput = 1;

        // Edge detection for jump
        jumpPressed = uiService.IsJumpPressed();
        bool jumpJustPressed = jumpPressed && !jumpWasPressedLastFrame;

        if (jumpJustPressed)
            Jump();

        jumpWasPressedLastFrame = jumpPressed;

        // Horizontal input (example)
        if (uiService.IsLeftPressed()) moveInput = -1f;
        else if (uiService.IsRightPressed()) moveInput = 1f;
        else moveInput = 0f;
    }

    private void FixedUpdate()
    {
        if (hasDied) return;

        MovePlayer();
        CalculateDistance();
    }
    private void LateUpdate()
    {
        MoveCamera();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            //game over...
            hasDied = true;
            if (distanceTravelled > PlayerPrefs.GetFloat("Best"))
            {
                PlayerPrefs.SetFloat("Best", distanceTravelled);
                PlayerPrefs.Save();
            }
            anim.SetTrigger("Death");

            uiService.GameOverUI();
        }
    }
    private void MoveCamera()
    {
        Vector3 targetPosition = new Vector3(0f, transform.position.y + 15f, transform.position.z - 20f);

        this.mainCamera.transform.position = targetPosition;
    }

    private void MovePlayer()
    {
        Vector3 velocity = rb.linearVelocity;

        // Forward speed
        velocity.z = runSpeed;

        // Side movement
        velocity.x = moveInput * runSpeed * 0.5f;

        // Apply
        rb.linearVelocity = velocity;

        // Clamp X manually
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -20f, 20f);
        transform.position = pos;

        anim.SetFloat("Speed", 1);
    }

    private void Jump()
    {
        if(isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetTrigger("Jump");
        }
    }

    private void CalculateDistance()
    {
        float deltaZ = transform.position.z - lastZPosition;

        if(deltaZ > 0)
        {
            distanceTravelled += deltaZ;
        }

        lastZPosition = transform.position.z;

        if(uiService)
        {
            uiService.UpdateDistanceText(distanceTravelled);
        }
    }

    public void RestartPlayer()
    {
        transform.position = Vector3.zero;
        MoveCamera();

        lastZPosition = transform.position.z;
        distanceTravelled = 0;

        uiService.UpdateDistanceText(distanceTravelled);

        anim.SetTrigger("Restart");

        hasDied = false;
    }

    public void SetController(PlayerController controller) => this.playerController = controller;
}
