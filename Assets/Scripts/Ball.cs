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

    bool inTurnBallWhiteCR = false;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        sCollider = GetComponent<SphereCollider>();

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
            BounceBall(c);
            /*
            if (sCollider.enabled)
            {
                sCollider.enabled = false;
            }
            StartCoroutine(ReEnableCollider(3));
            */
        }
        else
        {
            // if ball collides with the floor or something random, it is no longer bouncing
            isBouncing = false;
        }
    }

    // Re-enable the ball collider after n frames
    bool test = false;
    IEnumerator ReEnableCollider(int n)
    {
        if (test) yield break;

        yield return new WaitForFixedUpdate();
        test = true;

        sCollider.enabled = true;
        test = false;

        /*

        if (n <= 1)
        {
            sCollider.enabled = true;
            test = false;
        }
        else
        {
            StartCoroutine(ReEnableCollider(--n));
        }
        */
    }

    void OnCollisionStay(Collision c)
    {
        return;
    }

    private void BounceBall(Collision c)
    {
        Vector3 paddleVelocity = c.gameObject.GetComponent<VelocityEstimator>().GetVelocityEstimate();
        
        // Get collision point
        ContactPoint cp = c.GetContact(0);
        //Debug.DrawRay(cp.point, cp.normal, Color.yellow, 3f);           // draw contact normal

        // Get velocity of ball just before hitting paddle
        Vector3 iVelocity = GetComponent<Kinematics>().storedVelocity;
        //Debug.DrawRay(transform.position, -iVelocity, Color.red, 3f);   // draw in vector

        // Get reflected bounce, with energy transfer
        Vector3 rVelocity = GetComponent<Kinematics>().GetReflectionDamped(iVelocity, cp.normal, 0.75f);
        //Debug.DrawRay(transform.position, rVelocity, Color.green, 3f);  // draw reflected vector

        // Account for paddle motion
        Vector3 fVelocity = (rVelocity + paddleVelocity); 

        // Adjust bounce velocity for reduced degree of freedom
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            fVelocity = ReduceBounceDeviation(fVelocity);
        }

        // If physics are being changed mid game, change them!
        if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
        {
            fVelocity += currentBounceModification;
        }

        // Apply new velocity to ball
        rigidBody.velocity = fVelocity;

        // Determine if collision should be counted as an active bounce
        if (paddleVelocity.magnitude < 0.05f)
        {
            isBouncing = false;
        }
        else
        {
            isBouncing = true;
            DeclareBounce(c);
            GetComponent<BounceSoundPlayer>().PlayBounceSound();

            //CheckApexSuccess(fVelocity, cp.point);
        }
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

    public void TurnBallWhite()
    {
        GetComponent<MeshRenderer>().material = ballMat;
    }

    public IEnumerator TurnBallWhiteCR(float time)
    {
        if (inTurnBallWhiteCR)
        {
            yield break;
        }
        yield return new WaitForSeconds(time);
        inTurnBallWhiteCR = true;

        TurnBallWhite();
        inTurnBallWhiteCR = false;
    }

    public IEnumerator TurnBallGreenCR(float time)
    {
        yield return new WaitForSeconds(time);
        TurnBallGreen();
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
        TurnBallWhite();
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
