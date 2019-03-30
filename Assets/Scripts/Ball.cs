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

    // This is true when the player is currently paddling the ball. If the player stops paddling the ball,
    // set to false.
    public bool isBouncing = false;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    // Degrees of freedom (in radians), how many rads in x-z directions ball can bounce after hitting paddle
    // 0 rad: ball can only bounce in y direction, pi/2 rad: no reduction in range
    public float radiansOfFreedom; 

    // A reference to this ball's rigidbody
    private Rigidbody rigidBody;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        radiansOfFreedom = GlobalControl.Instance.degreesOfFreedom * Mathf.Deg2Rad;
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

        Debug.Log("paddle velovity: " + paddleVelocity.ToString("F4"));
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
            isBouncing = true;
            DeclareBounce(c);

            // If physics are being changed mid game, change them!
            if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
            {
                bounceVelocity = bounceVelocity + currentBounceModification;
            }

            // Restrict bounce velocity in the X and Z directions based on DoF 

        }

        // Adjust bounce velocity for reduced degree of freedom
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            bounceVelocity = ReduceBounceDeviation(bounceVelocity);
        }

        // reset the velocity of the ball
        rigidBody.velocity = new Vector3(0, 0, 0);
        rigidBody.angularVelocity = Vector3.zero;

        // Exert the new velocity on the ball
        rigidBody.AddForce(bounceVelocity, ForceMode.Impulse);

        GetComponent<BounceSoundPlayer>().PlayBounceSound();

    }

    public Vector3 ReduceBounceDeviation(Vector3 v)
    {
        if (Vector3.Angle(Vector3.up, v) <= GlobalControl.Instance.degreesOfFreedom)
        {
            return v;
        }

        float bounceMagnitude = v.magnitude;
        
        float yReduced = v.y * Mathf.Cos( GlobalControl.Instance.degreesOfFreedom * Mathf.Deg2Rad);

        float xzReducedMagnitude = bounceMagnitude * Mathf.Sin( GlobalControl.Instance.degreesOfFreedom * Mathf.Deg2Rad);
        Vector3 xzReduced = new Vector3(v.x, 0, v.z) * xzReducedMagnitude;
        
        Vector3 modifiedBounceVelocity = new Vector3(xzReduced.x, yReduced, xzReduced.z) * bounceMagnitude;

        return modifiedBounceVelocity;
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
            GetComponent<BallParticleSpawner>().SpawnBounceParticles();
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
