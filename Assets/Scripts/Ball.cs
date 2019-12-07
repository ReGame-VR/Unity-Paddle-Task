using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Unity.Labs.SuperScience;

public class Ball : MonoBehaviour
{    
    [Tooltip("The normal ball color")]
    [SerializeField]
    private Material ballMat;

    [Tooltip("The ball color when it is in the target height range")]
    [SerializeField]
    private Material greenBallMat;

    [Tooltip("Auxilliary color materials")]
    [SerializeField]
    private Material redBallMat;
    [SerializeField]
    private Material blueBallMat;

    [Tooltip("The script that handles the game logic")]
    [SerializeField]
    private PaddleGame gameScript;


    // The current bounce effect in a forced exploration condition
    public Vector3 currentBounceModification;

    // Store last position of ball for pause
    private Vector3 lastPosition = Vector3.up;

    // This is true when the player is currently paddling the ball. If the 
    // player stops paddling the ball, set to false.
    public bool isBouncing = false;

    // If the ball just bounced, this will be true (momentarily)
    private bool justBounced = false;

    // A reference to this ball's rigidbody and collider
    private Rigidbody rigidBody;

    // For Green/White IEnumerator coroutine 
    bool inTurnBallWhiteCR = false;

    // Unity PhysicsTracker Configuration ======================================
    [SerializeField]
    [Tooltip("The object to track in space and report physics data on.")]
    Transform m_ToTrack;
    public PhysicsTracker m_MotionData = new PhysicsTracker();
    // =========================================================================

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Physics for ball is disabled until Space is pressed
        rigidBody.velocity = Vector3.zero;
        rigidBody.useGravity = false;
        rigidBody.detectCollisions = false;

        // Use UnityLabs PhysicsTracker
        m_MotionData.Reset(m_ToTrack.position, m_ToTrack.rotation, Vector3.zero, Vector3.zero);
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
        }
        // if ball collides with the floor or something random, it is no longer bouncing
        else
        {
            isBouncing = false;
        }
    }

    // For every frame that the ball is still in collision with the paddle, 
    // apply a fraction of the paddle velocity to the ball. This is a fix for
    // the "sticky paddle" effect seen at low ball speeds. Crude approximation 
    // of acceleration. 
    void OnCollisionStay(Collision c)
    {
        float pVySlice = m_MotionData.Velocity.y / 8.0f;    // 8 is a good divisor
        rigidBody.velocity += new Vector3(0, pVySlice, 0);
    }

    // Called from ExplorationMode.cs --> start()
    public void InitBounceMod()
    {
        List<Vector3> bounceModList = GameObject.Find("[SteamVR]").GetComponent<ExplorationMode>().GetBounceModList();
        Debug.Log("Init bounce mod. ec:" + GlobalControl.Instance.expCondition);
        switch (GlobalControl.Instance.expCondition)
        {
            case ExpCondition.NORMAL:
                currentBounceModification = bounceModList[0];
                break;
            case ExpCondition.LIGHTEST:
                currentBounceModification = bounceModList[1];
                break;
            case ExpCondition.LIGHTER:
                currentBounceModification = bounceModList[2];
                break;
            case ExpCondition.HEAVIER:
                currentBounceModification = bounceModList[3];
                break;
            case ExpCondition.HEAVIEST:
                currentBounceModification = bounceModList[4];
                break;
            default:
                currentBounceModification = new Vector3(0, 0, 0);
                break;
        }

        Debug.Log("Initializing ball bounce mod to " + currentBounceModification.y);
    }

    // Returns the default spawn position of the ball (10cm above the target line) 
    public static Vector3 spawnPosition(GameObject targetLine)
    {
        return new Vector3(0.0f, targetLine.transform.position.y + 0.1f, 0.5f);
    }

    private void BounceBall(Collision c)
    {
        ContactPoint cp = c.GetContact(0);

        Vector3 paddleVelocity = m_MotionData.Velocity;
        Vector3 paddleAccel = m_MotionData.Acceleration;
        Vector3 Vin = GetComponent<Kinematics>().storedVelocity;
        
        ApplyBouncePhysics(paddleVelocity, paddleAccel, cp, Vin);

        // Determine if collision should be counted as an active bounce
        if (paddleVelocity.magnitude < 0.05f)
        {
            isBouncing = false;
        }
        else
        {
            isBouncing = true;

            CheckApexSuccess();
            DeclareBounce(c);
            GetComponent<BounceSoundPlayer>().PlayBounceSound();
        }

        // DEBUGGING
        debugvelocitycollision(cp.normal);
    }

    void CheckApexSuccess()
    {
        StartCoroutine(CheckForApex());
    }

    IEnumerator CheckForApex()
    {
        yield return new WaitWhile( () => !GetComponent<Kinematics>().ReachedApex());

        float apexHeight = rigidBody.position.y;
        bool successfulBounce = gameScript.HeightInsideTargetWindow(apexHeight);

        if (successfulBounce) { 
            gameScript.IndicateSuccessBall();       // Flash ball green 
        }
    
        if (GlobalControl.Instance.expCondition == ExpCondition.RANDOM)
        {
            gameScript.ModifyPhysicsOnSuccess(successfulBounce);    // Check if 3 bounces were successful in the last 10
        }
    }

    // Perform physics calculations to bounce ball. Includes ExplorationMode modifications.
    void ApplyBouncePhysics(Vector3 paddleVelocity, Vector3 paddleAccel, ContactPoint cp, Vector3 Vin)
    {
        // Reduce the effects of the paddle tilt so the ball doesn't bounce everywhere
        Vector3 reducedNormal = ProvideLeewayFromUp(cp.normal);

        // Get reflected bounce, with energy transfer
        Vector3 Vreflected = GetComponent<Kinematics>().GetReflectionDamped(Vin, reducedNormal, 0.8f);
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            Vreflected = LimitDeviationFromUp(Vreflected, GlobalControl.Instance.degreesOfFreedom);
        }

        // Apply paddle velocity
        if (GlobalControl.Instance.condition == Condition.REDUCED)
        {
            Vreflected = new Vector3(0, Vreflected.y + (2.0f * paddleVelocity.y), 0);
        }
        else
        {
            Vreflected += new Vector3(0, paddleVelocity.y, 0);
        }
        rigidBody.velocity = Vreflected;

        // If physics are being changed mid game, change them!
        if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
        {
            rigidBody.velocity += currentBounceModification;
        }
    }

    Vector3 ProvideLeewayFromUp(Vector3 n)
    {
        float degreesOfTilt = Vector3.Angle(Vector3.up, n);
        float limit = 2.0f; // feels pretty realistic through testing
        if (degreesOfTilt < limit)
        {
            degreesOfTilt /= limit;
        }
        else
        {
            degreesOfTilt -= limit;
        }
        return LimitDeviationFromUp(n, degreesOfTilt);
    }

    // for debugging only. remove later.
    void debugvelocitycollision(Vector3 n)
    {
        DebuggerDisplay dd = GameObject.Find("Debugger Display").GetComponent<DebuggerDisplay>();

        dd.Display("paddle tilt deg: " + (Vector3.Angle(Vector3.up, n)), 1);

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
    public Vector3 LimitDeviationFromUp(Vector3 v, float degreesOfFreedom)
    {
        if (Vector3.Angle(Vector3.up, v) <= degreesOfFreedom)
        {
            return v;
        }

        float bounceMagnitude = v.magnitude;
        float yReduced = bounceMagnitude * Mathf.Cos(degreesOfFreedom * Mathf.Deg2Rad);
        float xzReducedMagnitude = bounceMagnitude * Mathf.Sin(degreesOfFreedom * Mathf.Deg2Rad);
        Vector3 xzReduced = new Vector3(v.x, 0, v.z).normalized * xzReducedMagnitude;

        Vector3 modifiedBounceVelocity = new Vector3(xzReduced.x, yReduced, xzReduced.z);
        return modifiedBounceVelocity;
    }

    // Modifies the bounce for this forced exploration game
    public void SetBounceModification(Vector3 modification)
    {
        currentBounceModification = modification;
    }

    // Returns the bounce for this forced exploration game
    public Vector3 GetBounceModification()
    {
        return currentBounceModification;
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

    public IEnumerator TurnBallWhiteCR(float time = 0.0f)
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

    public IEnumerator TurnBallGreenCR(float time = 0.0f)
    {
        yield return new WaitForSeconds(time);
        TurnBallGreen();
    }
}
