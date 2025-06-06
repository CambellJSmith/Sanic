using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputActions;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentFloorNormal = Vector2.down;
    private bool isGrounded = false;
    private Collider2D currentFloor;

    [Header("Movement Settings")]
    public float maxSpeed = 10f;
    public float groundSpeedLimitWhenAirborne = 5f;
    public float acceleration = 3f;
    public float deceleration = 15f;
    public float jumpForce = 7f;

    [Header("Jump Buffer Settings")]
    public float jumpBufferTime = 0.25f;
    private float jumpBufferCounter = 0f;

    private Vector2 velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => QueueJump();
    }

    private void FixedUpdate()
    {
        Vector2 right = transform.right;
        float inputX = moveInput.x;

        float targetX = inputX * maxSpeed;

        // Restrict speed increase above 5 when airborne
        if (!isGrounded && Mathf.Abs(rb.linearVelocity.x) >= groundSpeedLimitWhenAirborne && Mathf.Sign(targetX) == Mathf.Sign(rb.linearVelocity.x))
        {
            targetX = rb.linearVelocity.x; // maintain current velocity, no further increase
        }

        float accelRate;
        float currentX = velocity.x;

        if (Mathf.Sign(currentX) != Mathf.Sign(targetX) && Mathf.Abs(currentX) > 0.01f)
        {
            accelRate = deceleration;
        }
        else
        {
            accelRate = (moveInput.magnitude > 0) ? acceleration : deceleration;
        }

        velocity.x = Mathf.MoveTowards(currentX, targetX, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        rb.AddForce(Vector2.down * 9.81f, ForceMode2D.Force);

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;

            if (isGrounded)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
            }
        }
    }

    private void QueueJump()
    {
        jumpBufferCounter = jumpBufferTime;
    }

    private void ExecuteJump()
    {
        Vector2 jumpDirection = transform.up;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        isGrounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            currentFloor = collision.collider;
            currentFloorNormal = collision.GetContact(0).normal;
            isGrounded = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider == currentFloor)
        {
            currentFloorNormal = collision.GetContact(0).normal;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == currentFloor)
        {
            isGrounded = false;
            currentFloor = null;
        }
    }

    private void LateUpdate()
    {
        if (isGrounded)
        {
            AlignWithNormal(currentFloorNormal);
        }
        else
        {
            rb.MoveRotation(0f); // Reset to upright when airborne
        }
    }

    private void AlignWithNormal(Vector2 normal)
    {
        // Calculate the angle needed to align transform.up with the floor normal
        float angle = Vector2.SignedAngle(Vector2.up, normal);

        // Apply that rotation immediately using Rigidbody2D
        rb.MoveRotation(angle);
    }
}
