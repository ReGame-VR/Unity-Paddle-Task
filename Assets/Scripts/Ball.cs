using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Unity.Labs.SuperScience;

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

    // Unity PhysicsTracker Configuration =======================================================
    [SerializeField]
    [Tooltip("The object to track in space and report physics data on.")]
    Transform m_ToTrack;

    PhysicsTracker m_MotionData = new PhysicsTracker();
    Vector3 m_LastPosition;
    // ==========================================================================================

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        sCollider = GetComponent<SphereCollider>();

        // Physics for ball is disabled until Space is pressed
        rigidBody.velocity = Vector3.zero;
        rigidBody.useGravity = false;
        rigidBody.detectCollisions = false;

        // Use Unity PhysicsTracker
        m_MotionData.Reset(m_ToTrack.position, m_ToTrack.rotation, Vector3.zero, Vector3.zero);
        m_LastPosition = m_ToTrack.position;
    }

    private void Update()
    {
        // send updated information to physicstracker
        m_MotionData.mUpdate(m_ToTrack.position, m_ToTrack.rotation, Time.smoothDeltaTime);
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

    void OnCollisionExit(Collision c)
    {
        //
    }

    private void BounceBall(Collision c)
    {
        //Vector3 paddleVelocity = c.gameObject.GetComponent<VelocityEstimator>().GetVelocityEstimate();
        Vector3 paddleVelocity = m_MotionData.Velocity;
        Vector3 paddleAccel = m_MotionData.Acceleration;
        
        ContactPoint cp = c.GetContact(0);

        // Get velocity of ball just before hitting paddle
        Vector3 iVelocity = GetComponent<Kinematics>().storedVelocity;

        // Get reflected bounce, with energy transfer
        Vector3 rVelocity = GetComponent<Kinematics>().GetReflectionDamped(iVelocity, cp.normal, 0.8f);

        // Apply reflection
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            rVelocity = LimitDeviationFromUp(rVelocity);
        }

        // Apply paddle velocity
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            rVelocity = new Vector3(0, rVelocity.y + paddleVelocity.y, 0);
        }
        else
        {
            rVelocity += new Vector3(0, paddleVelocity.y, 0);
        }

        // Apply loose approximation of paddle acceleration force
        if (paddleAccel.y > 1.0f)
        {
            rVelocity += new Vector3(0, paddleAccel.y / 8, 0); // accel fraction determined through playtesting 
        }
        rigidBody.velocity = rVelocity;
        

        /*
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            Vector3 projectedUp = paddleAccel * Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, paddleAccel));
            projectedUp = Vector3.up * projectedUp.magnitude; // make sure it only goes up
            rigidBody.AddForce(projectedUp);

        }
        else
        {
            Vector3 projectedNormal = paddleAccel * Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(cp.normal, paddleAccel));
            // TODO: sometimes paddle accel is negative even when the intended motion is to go up.
            // Makes re-bouncing the ball once it slows almost impossible. 
            rigidBody.AddForce(projectedNormal);
        }
         */

        // If physics are being changed mid game, change them!
        if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
        {
            rigidBody.velocity += currentBounceModification;
        }

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



        // DEBUGGING
        debugvelocitycollision(paddleVelocity, rigidBody.velocity, paddleAccel);
    }

    // for debugging only. remove later.
    void debugvelocitycollision(Vector3 paddlev, Vector3 outv, Vector3 paddlea)
    {
        DebuggerDisplay dd = GameObject.Find("Debugger Display").GetComponent<DebuggerDisplay>();

        dd.Display("Paddle vy: " + paddlev.y.ToString() + "\nPaddle ay: " + paddlea.y.ToString(), 1);
        dd.Display("Ball inv: " + GetComponent<Kinematics>().storedVelocity +
            "  mag: " + GetComponent<Kinematics>().storedVelocity.magnitude, 2);
        dd.Display("Ball outv: " + outv + "   mag: " + outv.magnitude, 3);
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
    public Vector3 LimitDeviationFromUp(Vector3 v)
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
