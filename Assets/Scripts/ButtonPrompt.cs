using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ButtonPrompt : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool playerInRange = false;
    private bool hasInteractedThisSession = false;

    public InputAction interactAction;
    public string[] dialogueLines;

    private Text legacyText;
    private Coroutine clearTextCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Look for legacy Text component in any grandchild
        foreach (Transform child in transform)
        {
            foreach (Transform grandchild in child)
            {
                legacyText = grandchild.GetComponent<Text>();
                if (legacyText != null)
                    break;
            }
            if (legacyText != null)
                break;
        }
    }

    void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        interactAction.performed -= OnInteract;
        interactAction.Disable();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= 2f)
        {
            if (!playerInRange)
            {
                spriteRenderer.enabled = true;
                playerInRange = true;
                hasInteractedThisSession = false; // Reset on new proximity entry
            }
        }
        else
        {
            if (playerInRange)
            {
                spriteRenderer.enabled = false;
                playerInRange = false;
            }
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInRange && !hasInteractedThisSession && legacyText != null && dialogueLines.Length > 0)
        {
            hasInteractedThisSession = true;
            spriteRenderer.enabled = false;

            int index = Random.Range(0, dialogueLines.Length);
            legacyText.text = dialogueLines[index];

            if (clearTextCoroutine != null)
                StopCoroutine(clearTextCoroutine);

            clearTextCoroutine = StartCoroutine(ClearTextAfterDelay(1f));
        }
    }

    IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        legacyText.text = "";
        clearTextCoroutine = null;
    }
}
