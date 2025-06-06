using UnityEngine;

public class ParticleController : MonoBehaviour
{
    // Reference to the Rigidbody2D (or Rigidbody if using 3D)
    private Rigidbody2D rb;

    // Reference to the ParticleSystem
    private new ParticleSystem particleSystem;
    private ParticleSystem.EmissionModule emission;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the references
        rb = GetComponent<Rigidbody2D>();
        particleSystem = GetComponent<ParticleSystem>();
        emission = particleSystem.emission;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player's velocity on the x-axis is greater than 8
        if (Mathf.Abs(rb.linearVelocity.x) > 8f)
        {
            // Enable particle emission if player is moving fast enough
            emission.enabled = true;
        }
        else
        {
            // Disable particle emission if player is not moving fast enough
            emission.enabled = false;
        }
    }
}
