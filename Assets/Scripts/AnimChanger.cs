using UnityEngine;
using UnityEngine.InputSystem; // For new Input System

public class AnimChanger : MonoBehaviour
{
    private AnimArrays animArrays;
    public GameObject targetObject;

    private Rigidbody2D rb;
    private string currentAnimation = "";
    private bool isTouchingFloor = false;
    private bool isTouchingWall = false;

    private bool suppressFloorAnimations = false;
    private int suppressUntilFrame = 0;
    private int frameJumped = -1; // Track when jump occurred
    private const int SuppressionFrameCount = 30; // ~0.5s at 60fps
    private const int MinSuppressionFrames = 10;

    private SpriteRenderer spriteRenderer; // To flip sprite
    private bool isFlipped = false; // To track current flip state

    private InputSystem_Actions inputActions; // Reference to Input Actions class
    private Vector2 moveInput; // Store the move input from the new Input System

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody2D found on AnimChanger object!");
        }

        if (targetObject != null)
        {
            animArrays = targetObject.GetComponent<AnimArrays>();
            if (animArrays == null)
            {
                Debug.LogWarning("AnimArrays component not found on targetObject!");
            }
        }
        else
        {
            Debug.LogWarning("targetObject is not assigned in the Inspector!");
        }

        spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("No SpriteRenderer found on targetObject!");
        }

        // Initialize Input Actions
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Subscribe to Move and Jump actions
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => QueueJump();
    }

    private void QueueJump()
    {
        if (isTouchingFloor)
        {
            suppressFloorAnimations = true;
            suppressUntilFrame = Time.frameCount + SuppressionFrameCount;
            frameJumped = Time.frameCount;
            currentAnimation = "Jump"; // Set jump animation immediately
            animArrays.SetActiveArrayByName("Jump");
        }
    }

    [System.Obsolete]
    void Update()
    {
        if (rb == null || animArrays == null) return;

        string desiredAnimation = currentAnimation;

        // End suppression when time is up
        if (suppressFloorAnimations && Time.frameCount >= suppressUntilFrame)
        {
            suppressFloorAnimations = false;
        }

        // Handle floor and wall interactions
        if (!suppressFloorAnimations)
        {
            if (!isTouchingFloor && isTouchingWall && Mathf.Abs(rb.velocity.y) < 0.1f)
            {
                desiredAnimation = "Wall";
            }
            else if (isTouchingFloor)
            {
                float speed = rb.velocity.magnitude;

                if (speed < 0.1f)
                    desiredAnimation = "Idle";
                else if (speed <= 5f)
                    desiredAnimation = "Walk";
                else
                    desiredAnimation = "Run";

                // Check if x velocity is below 0.1 and move input is down (left stick pulled down), set Crouch animation
                if (Mathf.Abs(rb.velocity.x) < 0.1f && moveInput.y < 0) // Left stick pulled down (y < 0)
                {
                    desiredAnimation = "Crouch";
                }
            }
            else
            {
                desiredAnimation = "Flip";
            }

            // Sprite flipping based on horizontal velocity
            if (Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                if (rb.velocity.x < 0 && !isFlipped)
                {
                    spriteRenderer.flipX = true;
                    isFlipped = true;
                }
                else if (rb.velocity.x > 0 && isFlipped)
                {
                    spriteRenderer.flipX = false;
                    isFlipped = false;
                }
            }
        }

        if (desiredAnimation != currentAnimation)
        {
            animArrays.SetActiveArrayByName(desiredAnimation);
            currentAnimation = desiredAnimation;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Floor"))
        {
            if (!isTouchingFloor)
            {
                Debug.Log("Player has landed, triggering 'Land' animation.");
                animArrays.SetActiveArrayByName("Land");
                currentAnimation = "Land";

                suppressFloorAnimations = false;
                suppressUntilFrame = 0;
                frameJumped = -1;
            }

            isTouchingFloor = true;

            if (suppressFloorAnimations && frameJumped > 0 && Time.frameCount - frameJumped >= MinSuppressionFrames)
            {
                suppressFloorAnimations = false;
                suppressUntilFrame = 0;
                frameJumped = -1;
            }
        }

        if (collision.collider.CompareTag("Wall"))
        {
            isTouchingWall = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Floor"))
        {
            isTouchingFloor = true;
        }

        if (collision.collider.CompareTag("Wall"))
        {
            isTouchingWall = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Floor"))
        {
            isTouchingFloor = false;
        }

        if (collision.collider.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }
}
