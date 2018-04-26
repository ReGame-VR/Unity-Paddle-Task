using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    // The force at which the ball will bounce upon collisions
    [SerializeField]
    private float bounceForce = 0.26f;

    // A reference to this ball's rigidbody
    private Rigidbody rigidBody;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        // On collision with paddle, ball should bounce
        if (c.gameObject.tag == "Paddle")
        {

            // Get velocity of paddle
            float paddleVelocity = c.gameObject.GetComponent<VelocityNoRigidBody>().GetVelocity().magnitude;
            
            // Create a bounce velocity based on collision with paddle. Also include paddle velocity
            // so that this bounce is proportional to movement of paddle
            Vector3 bounceVelocity = c.contacts[0].normal * bounceForce * paddleVelocity;

            // reset the velocity of the ball
            rigidBody.velocity = new Vector3(0, 0, 0);
           
            // Exert the new velocity on the ball
            rigidBody.AddForce(bounceVelocity, ForceMode.Impulse);
        }
        else
        {
            // On collision with everything else, ball should behave normally (roll, not bounce)
        }
    }
}
