using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Ball : MonoBehaviour {

    // 0.31f is a good value !
    [Tooltip("The force at which the ball will bounce upon collisions")]
    [SerializeField]
    private float bounceForce = 0.31f;

    [Tooltip("The normal ball color")]
    [SerializeField]
    private Material ballMat;

    [Tooltip("The ball color when it is in the target height range")]
    [SerializeField]
    private Material greenBallMat;

    [Tooltip("The script that handles the game logic")]
    [SerializeField]
    private PaddleGame gameScript;

    // The current bounce effect in a forced exploration condition
    private Vector3 currentBounceModification = new Vector3(0,0,0);

    // Store last position of ball for pause
    private Vector3 lastPosition = Vector3.up;

    // This is true when the player is currently paddling the ball. If the player stops paddling the ball,
    // set to false.
    public bool isBouncing = false;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    // A reference to this ball's rigidbody
    private Rigidbody rigidBody;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Physics for ball is disabled until Space is pressed
        rigidBody.velocity = Vector3.zero;
        rigidBody.useGravity = false;
        rigidBody.detectCollisions = false;
    }

    void Update()
    {
        // Handle pausing
        if (GlobalControl.Instance.paused)
        {
            // Hold ball still
            rigidBody.velocity = Vector3.zero;
            rigidBody.position = lastPosition;

            // Space == pause button
            if (Input.GetKeyDown("space"))
            {
                GlobalControl.Instance.paused = false;
                rigidBody.detectCollisions = true;
                rigidBody.useGravity = true;
            }
        }
        else
        {
            // Space == pause button
            if (Input.GetKeyDown("space"))
            {
                GlobalControl.Instance.paused = true;

                lastPosition = rigidBody.position;
                rigidBody.detectCollisions = false;
                rigidBody.useGravity = false;

                // try to avoid resetting ball constantly while paused
                if (lastPosition.y < 0.05)
                {
                    lastPosition = new Vector3(lastPosition.x, 0.5f, lastPosition.y);
                }
            }
        }

    }

    void OnCollisionEnter(Collision c)
    {
        // On collision with paddle, ball should bounce
        if (c.gameObject.tag == "Paddle")
        {
            BounceBall(c);
        }
        else
        {
            // if ball collides with the floor or something random,
            // it is no longer bouncing
            isBouncing = false;
        }
    }

    void OnCollisionStay(Collision c)
    {

        // return; // EW seems to go very high sometimes.
        if (c.gameObject.tag == "Paddle")
        {
            BounceBall(c);
        }
    }

    private void BounceBall(Collision c)
    {
        // Get velocity of paddle
        float paddleVelocity = c.gameObject.GetComponent<VelocityEstimator>().GetVelocityEstimate().magnitude;
        // float paddleVelocity = c.gameObject.GetComponent<VelocityNoRigidBody>().GetVelocity().magnitude;

        // Debug.Log("paddle velovity: " + paddleVelocity.ToString("F4"));
        // Create a bounce velocity based on collision with paddle. Also include paddle velocity
        // so that this bounce is proportional to movement of paddle

        Vector3 bounceVelocity;

        if (paddleVelocity < 0.3)
        {
            // If the paddle isn't moving very much, add a default bounce. This is 
            // a bugfix so that the ball keeps bouncing.
            bounceVelocity = c.contacts[0].normal * bounceForce * 0.3f;

            // The ball is not being actively paddled
            isBouncing = false;
        }
        else
        {
            bounceVelocity = c.contacts[0].normal * bounceForce * paddleVelocity;

            // Adjust bounce velocity for reduced degree of freedom
            if (GlobalControl.Instance.condition == Condition.REDUCED)
            {
                bounceVelocity = ReduceBounceDeviation(bounceVelocity);
            }

            isBouncing = true;
            DeclareBounce(c);

            // If physics are being changed mid game, change them!
            if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
            {
                bounceVelocity = bounceVelocity + currentBounceModification;
            }
        }

        // reset the velocity of the ball
        rigidBody.velocity = new Vector3(0, 0, 0);
        rigidBody.angularVelocity = Vector3.zero;

        // Exert the new velocity on the ball
        rigidBody.AddForce(bounceVelocity, ForceMode.Impulse);

        GetComponent<BounceSoundPlayer>().PlayBounceSound();

    }

    public void TurnBallGreen()
    {
        GetComponent<MeshRenderer>().material = greenBallMat;
    }

    public void TurnBallWhite()
    {
        GetComponent<MeshRenderer>().material = ballMat;
    }

    // Try to declare that the ball has been bounced. If the ball
    // was bounced too recently, then this declaration will fail.
    // This is to ensure that bounces are only counted once.
    public void DeclareBounce(Collision c)
    {
        if (justBounced)
        {
            // do nothing, this bounce has already been counted
            return;
        }
        else
        {
            justBounced = true;
            gameScript.BallBounced(c);
            StartCoroutine(FinishBounceDeclaration());
        }
    }

    // Wait a little bit before a bounce can be declared again.
    // This is to ensure that bounces are not counted multiple times.
    IEnumerator FinishBounceDeclaration()
    {
        yield return new WaitForSeconds(0.2f);
        justBounced = false;
    }

    // Ball has been reset. Reset the trial as well.
    public void ResetBall()
    {
        gameScript.ResetTrial();
    }

    // If in Reduced condition, returns the vector of the same original magnitude and same x-z direction
    // but with adjusted height so that the angle does not exceed the desired degrees of freedom
    public Vector3 ReduceBounceDeviation(Vector3 v)
    {
        if (Vector3.Angle(Vector3.up, v) <= GlobalControl.Instance.degreesOfFreedom)
        {
            return v;
        }

        float bounceMagnitude = v.magnitude;

        float yReduced = bounceMagnitude * Mathf.Cos(GlobalControl.Instance.degreesOfFreedom * Mathf.Deg2Rad);

        float xzReducedMagnitude = bounceMagnitude * Mathf.Sin(GlobalControl.Instance.degreesOfFreedom * Mathf.Deg2Rad);
        Vector3 xzReduced = new Vector3(v.x, 0, v.z).normalized * xzReducedMagnitude;

        Vector3 modifiedBounceVelocity = new Vector3(xzReduced.x, yReduced, xzReduced.z);

        return modifiedBounceVelocity;
    }

    // Modifies the bounce for this forced exploration game
    public void SetBounceModification(Vector3 modification)
    {
        currentBounceModification = modification;
    }

    // Modifies the bounce for this forced exploration game
    public Vector3 GetBounceModification()
    {
        return currentBounceModification;
    }
}
