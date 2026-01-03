using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("Animation")] // TAMBAHAN ANIMATOR
    public Animator animator; // Drag Animator komponen ke sini di Inspector

    public float speed;
    public float groundDrag;

    public float jumpForce = 5f;
    public float jumpCooldown = 0f;
    bool readyToJump = true;

    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        orientation = transform.Find("Orientation");

        // Otomatis cari animator jika lupa assign
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        rb.linearDamping = grounded ? groundDrag : 0; // Note: di Unity lama ini namanya 'drag', di Unity 6 'linearDamping'
        
        if (!gameObject.CompareTag("Player")) return;

        myInput();
        speedControl();
        UpdateAnimations(); // TAMBAHAN ANIMATOR

        // handle drag
    }

    void FixedUpdate()
    {
        movePlayer();
    }

    void myInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded && gameObject.CompareTag("Player"))
        {
            readyToJump = false;
            Jump();

            // TAMBAHAN ANIMATOR: Trigger Jump
            if (animator != null) animator.SetTrigger("Jump");

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // TAMBAHAN ANIMATOR: Fungsi baru untuk update animasi
    void UpdateAnimations()
    {
        if (animator == null) return;

        // Kirim status Grounded
        animator.SetBool("IsGrounded", grounded);

        // Kirim Speed (gunakan magnitude dari input agar animasi jalan meski tertahan tembok, 
        // atau gunakan rb.linearVelocity.magnitude untuk kecepatan asli physics)

        // Opsi 1: Berdasarkan Input (lebih responsif)
        float currentSpeed = new Vector2(horizontalInput, verticalInput).magnitude;

        // Opsi 2: Berdasarkan Kecepatan Asli (lebih realistis)
        // float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        // Jika kamu pakai lari (sprint), kalikan currentSpeed agar mencapai threshold Run
        animator.SetFloat("Speed", currentSpeed);
    }

    void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
    }

    void Jump()
    {
        // Ganti rb.velocity (Unity lama) atau rb.linearVelocity (Unity 6)
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void speedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}