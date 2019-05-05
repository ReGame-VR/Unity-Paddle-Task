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

    // For debugging purposes only
    [SerializeField]
    private Material redBallMat;
    [SerializeField]
    private Material blueBallMat;

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

    // A reference to this ball's rigidbody and collider
    private Rigidbody rigidBody;
    private SphereCollider sCollider;

    private float minVelocityThreshold = 0.15f;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Physics for ball is disabled until Space is pressed
        rigidBody.velocity = Vector3.zero;
        rigidBody.useGravity = false;
        rigidBody.detectCollisions = false;
    }

    void OnCollisionEnter(Collision c)
    {
        // On collision with paddle, ball should bounce
        if (c.gameObject.tag == "Paddle")
        {
            TurnBallBlue(); // debugging
            BounceBall(c);
        }
        else
        {
            // if ball collides with the floor or something random,
            // it is no longer bouncing
            isBouncing = false;
        }
    }

    // debugging
    private void Update()
    {
        Debug.DrawRay(transform.position, rigidBody.velocity, Color.blue);
    }


    // debugging
    private void OnCollisionExit(Collision c)
    {
        if (c.gameObject.tag == "Paddle")
        {
            TurnBallBlue(); // debugging
        }
    }

    IEnumerator ReEnableCollider()
    {
        yield return new WaitForFixedUpdate();
        sCollider.enabled = true;
    }

    void OnCollisionStay(Collision c)
    {

        // return; // EW seems to go very high sometimes.
        if (c.gameObject.tag == "Paddle")
        {
            TurnBallRed(); // debugging
            // BounceBall(c);
        }
    }

    private void BounceBall(Collision c)
    {
        Vector3 paddleVelocity = c.gameObject.GetComponent<VelocityEstimator>().GetVelocityEstimate();
        
        // Get collision point
        ContactPoint cp = c.GetContact(0);
        Debug.DrawRay(cp.point, cp.normal, Color.yellow, 3f);           // draw contact normal

        // Get velocity of ball just before hitting paddle
        Vector3 iVelocity = GetComponent<Kinematics>().storedVelocity;
        Debug.DrawRay(transform.position, -iVelocity, Color.red, 3f);   // draw in vector

        // Get reflected bounce, with energy transfer
        Vector3 rVelocity = GetComponent<Kinematics>().GetReflectionDamped(iVelocity, cp.normal);
        Debug.DrawRay(transform.position, rVelocity, Color.green, 3f);  // draw reflected vector

        isBouncing = true;
        DeclareBounce(c);

        // Account for paddle motion
        Vector3 fVelocity = (rVelocity + paddleVelocity);

        // Adjust bounce velocity for reduced degree of freedom
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            fVelocity = ReduceBounceDeviation(rVelocity);
        }

        // If physics are being changed mid game, change them!
        if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
        {
            fVelocity += currentBounceModification;
        }



        // Apply new velocity to ball
        rigidBody.velocity = fVelocity;



        /*

        // Get velocity of paddle
        float paddleVelocity = c.gameObject.GetComponent<VelocityEstimator>().GetVelocityEstimate().magnitude;
        // float paddleVelocity = c.gameObject.GetComponent<VelocityNoRigidBody>().GetVelocity().magnitude;

        // Debug.Log("paddle velovity: " + paddleVelocity.ToString("F4"));
        // Create a bounce velocity based on collision with paddle. Also include paddle velocity
        // so that this bounce is proportional to movement of paddle

        Vector3 bounceVelocity;

        if (paddleVelocity < minVelocityThreshold)
        {
            // If the paddle isn't moving very much, add a default bounce. This is 
            // a bugfix so that the ball keeps bouncing.
            bounceVelocity = c.contacts[0].normal * bounceForce * minVelocityThreshold;

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

        if (isBouncing)
        {
            GetComponent<BounceSoundPlayer>().PlayBounceSound();
        }

    */
    }

    public void TurnBallGreen()
    {
        GetComponent<MeshRenderer>().material = greenBallMat;
    }

    public void TurnBallRed()
    {
        GetComponent<MeshRenderer>().material = redBallMat;
    }

    public void TurnBallBlue()
    {
        GetComponent<MeshRenderer>().material = blueBallMat;
    }

    public IEnumerator TurnBallWhite()
    {
        yield return new WaitForSeconds(0.5f);
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
