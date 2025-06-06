using UnityEngine;

public class NPC : MonoBehaviour
{
    public Sprite[] idleSprites; // Array for idle sprites
    public Sprite[] walkSprites; // Array for walk sprites

    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentSpriteIndex = 0;
    private bool isWalking = false;
    private float moveTimer = 0f;
    private Vector3 moveDirection;
    private float animationSpeed = 12f;

    private Vector3 startPosition;
    private int walkLoops = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        ChooseNextAction();
    }

    void Update()
    {
        if (isWalking)
        {
            MoveAndAnimate();
        }
        else
        {
            AnimateIdle();
        }
    }

    void AnimateIdle()
    {
        animationTimer += Time.deltaTime * animationSpeed;
        if (animationTimer >= 1f)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[currentSpriteIndex];
            animationTimer = 0f;
        }
    }

    void MoveAndAnimate()
    {
        animationTimer += Time.deltaTime * animationSpeed;
        if (animationTimer >= 1f)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % walkSprites.Length;
            spriteRenderer.sprite = walkSprites[currentSpriteIndex];
            animationTimer = 0f;
        }

        transform.Translate(moveDirection * Time.deltaTime);
        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0f)
        {
            isWalking = false;
            walkLoops++;

            if (walkLoops < 3)
                ChooseNextAction();
            else
                ReturnToStart();
        }
    }

    void ChooseNextAction()
    {
        float waitTime = Random.Range(2f, 14f);
        Invoke(nameof(StartRandomWalk), waitTime);
    }

    void StartRandomWalk()
    {
        int dir = Random.Range(0, 2); // 0 = left, 1 = right
        SetMoveDirection(dir == 0 ? Vector3.left : Vector3.right);

        moveTimer = Random.Range(4f, 10f);
        isWalking = true;
    }

    void ReturnToStart()
    {
        Vector3 toStart = startPosition - transform.position;

        // Determine direction (left/right) for flipping
        SetMoveDirection(toStart.normalized);

        moveTimer = toStart.magnitude / 2f;
        isWalking = true;
        walkLoops = 0;
    }

    void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        // Flip sprite based on X direction
        spriteRenderer.flipX = moveDirection.x < 0;
    }
}
