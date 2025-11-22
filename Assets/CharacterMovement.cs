using UnityEngine;
using UnityEngine.InputSystem;

public class characterMovement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float acceleration = 5f;
    public Animator animator;

    private CharacterController controller;
    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // baca input pakai InputSystem
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude > 0.1f)
        {
            bool isRunning = Keyboard.current.leftShiftKey.isPressed;
            float targetSpeed = isRunning ? runSpeed : walkSpeed;

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

            controller.Move(direction * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            if (animator != null)
                animator.SetFloat("Speed", currentSpeed);
        }
        else
        {
            currentSpeed = 0f;
            if (animator != null)
                animator.SetFloat("Speed", 0f);
        }
    }
}
