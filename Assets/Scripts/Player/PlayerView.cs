using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private PlayerInputAction pia;
    private PlayerController playerController;
    private UIService uiService;
    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; } 
    public AudioSource audioSource { get; private set; }
    private Camera mainCamera;
    private float moveInput;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpWasPressedLastFrame;
    public float distanceTravelled { get; private set; }
    private float lastZPosition = 0f;
    public bool hasDied;
    private AudioClip runningAudioClip;
    private TerrainPool terrainPool;
    private ObstaclePool obstaclePool;

    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    private bool isVulnerable;
    private float vulnerableTimer;
    [SerializeField] private bool onMobile;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        distanceTravelled = 0f;
        isVulnerable = true;

        SpawnPlayer();
        SetInputPlatform();
        CreateCamera();
    }
    private void Start()
    {
        this.hasDied = true;
        this.isGrounded = true;
        this.uiService = GameService.Instance.UIService();

        lastZPosition = transform.position.z;

        runningAudioClip = GameService.Instance.SoundService.GetClip(SoundTypes.Running);
        audioSource.loop = true;
        audioSource.clip = runningAudioClip;

        terrainPool = FindAnyObjectByType<TerrainPool>();
        obstaclePool = FindAnyObjectByType<ObstaclePool>();
    }
    private void SetInputPlatform()
    {
        this.onMobile = GameService.Instance.onMobile;

        if(!onMobile)
        {
            SetupInput();
        }
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
        this.mainCamera.nearClipPlane = 0.3f;
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
        if (hasDied) return;

        if (isVulnerable)
        {
            vulnerableTimer = 0f;
        }
        else
        {
            vulnerableTimer += Time.deltaTime;

            if(vulnerableTimer >= 0.5)
            {
                isVulnerable = true;
            }
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("Grounded", isGrounded);

        if(onMobile)
        {
            CheckIfUIButtonPressed();
        }
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
        if (hasDied) return;

        MoveCamera();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isVulnerable && other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            isVulnerable = false;
            audioSource.PlayOneShot(GameService.Instance.SoundService.GetClip(SoundTypes.Ouch));

            GameService.Instance.SoundService.PlaySFX(SoundTypes.Pop);

            // Clear hit lane + next 2 lanes
            if (playerController.GetCurrentLives() - 1 > 0)
            {
                obstaclePool.ClearRowsFromHit(other.transform.position.z, 2);
            }


            playerController.SetCurrentLives(playerController.GetCurrentLives() - 1);

            uiService.LoseHeart();
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

        // --- Forward speed with smooth acceleration ---
        float minSpeed = runSpeed;        // starting speed
        float maxSpeed = 75f;   // max speed cap
        float distanceForMax = 2000f;     // distance at which max speed is reached

        // Smooth speed curve: accelerates quickly at first, then slows down
        float targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, distanceTravelled / distanceForMax);
        targetSpeed = Mathf.Min(targetSpeed, maxSpeed);

        velocity.z = targetSpeed;

        // --- Side movement ---
        velocity.x = moveInput * targetSpeed * 0.75f;

        // Apply velocity
        rb.linearVelocity = velocity;

        // Clamp X manually
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -20f, 20f);
        transform.position = pos;

        // --- Footstep audio ---
        if (!audioSource.isPlaying && isGrounded)
            audioSource.Play();
        else if (!isGrounded)
            audioSource.Pause();

        // --- Animator speed ---
        anim.SetFloat("Speed", 1);
    }

    private void Jump()
    {
        if(isGrounded)
        {
            audioSource.Pause();
            GameService.Instance.SoundService.PlaySFX(SoundTypes.Jump);
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
        // 1. Reset player first
        transform.position = Vector3.zero;
        MoveCamera();

        playerController.ResetCurrentLives();
        uiService.ResetHearts();
        lastZPosition = 0;
        distanceTravelled = 0;
        uiService.UpdateDistanceText(0);

        terrainPool.ResetTerrain();
        obstaclePool.ResetObstacles();

        anim.Rebind();
        hasDied = false;
    }

    public AudioSource PlayerAudio() => audioSource;

    public void SetController(PlayerController controller) => this.playerController = controller;
}
