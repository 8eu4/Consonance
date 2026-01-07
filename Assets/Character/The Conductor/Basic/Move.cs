using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    [Header("Movement")]
    public float speed = 7f; 
    public float groundDrag = 10f; 

    [Header("Jumping & Air Control")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public float airDrag = 1f; // Rem angin 

    [Header("Ground Detection (Automatic)")]
    public LayerMask whatIsGround;
    public bool grounded;

    bool readyToJump = true;

    Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;
    Collider playerCollider;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        orientation = transform.Find("Orientation");

        // Otomatis cari animator jika lupa assign
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        playerCollider = transform.GetChild(0).GetComponent<Collider>();
        if (playerCollider == null)
        {
            Debug.LogError("Waduh! Karakter ini tidak punya Capsule Collider atau Box Collider!");
        }
    }

    void Update()
    {
        Vector3 rayOrigin = playerCollider.bounds.center;
        float rayDistance = playerCollider.bounds.extents.y + 0.1f;
        grounded = Physics.Raycast(rayOrigin, Vector3.down, rayDistance, whatIsGround);
        Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, grounded ? Color.green : Color.red);

        // ground check
        rb.linearDamping = grounded ? groundDrag : airDrag;

        if (!gameObject.CompareTag("Player")) return;

        myInput();
        speedControl();
        UpdateAnimations(); // TAMBAHAN ANIMATOR

        // handle drag
    }

    void FixedUpdate()
    {
        if (!gameObject.CompareTag("Player")) return;
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

    public void ResetMovementState()
    {
        // 1. Reset Input ke 0
        horizontalInput = 0;
        verticalInput = 0;

        // 2. Paksa Animasi berhenti (Speed 0)
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        // 3. (Opsional) Hentikan sisa momentum fisika biar gak meluncur
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsGrounded", grounded);

        float currentSpeed = new Vector2(horizontalInput, verticalInput).magnitude;

        animator.SetFloat("Speed", currentSpeed);
    }

    void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
        {
            // Gerak di tanah (Full Power)
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        }
        else
        {
            // Gerak di udara (Dikalikan airMultiplier biar ga terlalu ngebut/licin)
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }
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