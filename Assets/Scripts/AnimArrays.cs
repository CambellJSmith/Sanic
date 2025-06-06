using System.Collections.Generic;
using UnityEngine;

public class AnimArrays : MonoBehaviour
{
    [System.Serializable]
    public class SpriteArray
    {
        public string arrayName;
        public Sprite[] sprites;
        public float timeBetweenFrames = 1f / 12f; // Default time between frames, editable in the inspector
    }

    [SerializeField]
    private List<SpriteArray> spriteArrays;

    public int activeArrayIndex = -1;

    private float timer = 0f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Frame tracking for playback
    private int currentFrameIndex = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        SetActiveArrayByName("Idle");
    }

    void Update()
    {
        if (activeArrayIndex < 0 || activeArrayIndex >= spriteArrays.Count)
            return;

        SpriteArray activeArray = spriteArrays[activeArrayIndex];
        if (activeArray.sprites == null || activeArray.sprites.Length == 0)
            return;

        // If the active array is "Run", adjust the time between frames based on velocity
        if (activeArray.arrayName == "Run")
        {
            AdjustRunAnimationSpeed(activeArray);
        }

        timer += Time.deltaTime;
        if (timer >= activeArray.timeBetweenFrames) // Use the time set in the array
        {
            timer = 0f;

            spriteRenderer.sprite = activeArray.sprites[currentFrameIndex];
            currentFrameIndex++;

            // Check if we reached the end of the current array
            if (currentFrameIndex >= activeArray.sprites.Length)
            {
                if (activeArray.arrayName == "Jump")
                {
                    SetActiveArrayByName("Flip"); // Automatically switch to Flip after Jump
                }
                else if (activeArray.arrayName == "Land")
                {
                    SetActiveArrayByName("Idle"); // Switch to Idle after Land finishes
                }
                else
                {
                    currentFrameIndex = 0; // Loop normally for other animations
                }
            }
        }
    }

    private void AdjustRunAnimationSpeed(SpriteArray activeArray)
    {
        // Check if the object's velocity on the x-axis is greater than 5
        float velocity = Mathf.Abs(rb.linearVelocity.x);

        if (velocity >= 5f)
        {
            // Calculate the new frame time based on the velocity, linearly between 5 and 9
            // 0.1 is the frame time at velocity 5, and 0.004 is the frame time at velocity 9
            float frameTime = Mathf.Lerp(0.1f, 0.004f, Mathf.InverseLerp(5f, 9f, velocity));
            activeArray.timeBetweenFrames = frameTime;
        }
        else
        {
            // If velocity is less than 5, reset to the default time between frames (0.8)
            activeArray.timeBetweenFrames = 0.8f;
        }
    }

    public void SetActiveArray(int arrayIndex)
    {
        if (arrayIndex >= 0 && arrayIndex < spriteArrays.Count)
        {
            activeArrayIndex = arrayIndex;
            currentFrameIndex = 0;
            // Log the newly active array name
            Debug.Log("Switched to animation array: " + spriteArrays[activeArrayIndex].arrayName);
        }
        else
        {
            Debug.LogWarning("Invalid array index!");
        }
    }

    public void SetActiveArrayByName(string arrayName)
    {
        for (int i = 0; i < spriteArrays.Count; i++)
        {
            if (spriteArrays[i].arrayName == arrayName)
            {
                SetActiveArray(i);
                return;
            }
        }

        Debug.LogWarning("Array with name \"" + arrayName + "\" not found.");
    }

    public string GetActiveArrayName()
    {
        if (activeArrayIndex >= 0 && activeArrayIndex < spriteArrays.Count)
        {
            return spriteArrays[activeArrayIndex].arrayName;
        }
        return "No Active Array";
    }
}
