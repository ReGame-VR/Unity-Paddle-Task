using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleGame : MonoBehaviour {

    [Tooltip("The head mounted display")]
    [SerializeField]
    private GameObject hmd;

    [Tooltip("The left paddle in the game")]
    [SerializeField]
    private GameObject leftPaddle;

    [Tooltip("The right paddle in the game")]
    [SerializeField]
    private GameObject rightPaddle;

    [Tooltip("The ball being bounced")]
    [SerializeField]
    private GameObject ball;

    [Tooltip("The line that denotes where the ball should be bounced ideally")]
    [SerializeField]
    private GameObject targetLine;

    [Tooltip("The canvas that displays score information to the user")]
    [SerializeField]
    private FeedbackCanvas feedbackCanvas;

    [Tooltip("The radius of the target line area. Example: If this is 0.05, the target line will be 0.10 thick")]
    [SerializeField]
    private float targetRadius = 0.05f;

    // Current number of bounces that the player has acheieved in this trial
    private int numBounces = 0;
    private int numAccurateBounces = 0;
    // Current score during this trial
    private float curScore = 0f;

    // The current trial number. This is increased by one every time the ball is reset.
    public int trialNum = 0;

    // A group of trials. The current group number will tick up every trial.
    // When curGroupNum reaches trialGroupSize, it will reset back to 1.
    private int bounceGroupSize = 10;
    private int curGroupNum = 1;

    // The paddle bounce height, velocity, and acceleration to be recorded on each bounce.
    // These are the values on the *paddle*, NOT the ball
    private float paddleBounceHeight;
    private float paddleBounceVelocity;
    private float paddleBounceAccel;

    // Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
    // 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
    public float degreesOfFreedom;

    // Trial Condition and Visit type
    private Condition condition;
    private Session session;

    // Variables to keep track of resetting the ball after dropping to the ground
    GameObject paddle;
    private bool inHoverMode = false;
    private bool inHoverResetCoroutine = false;
    private bool inPlayDropSoundRoutine = false;
    private int ballResetHoverSeconds = 3;

    // Keep track of max number of trials allowed for this instance
    private int maxTrials = 0;

    private List<float> bounceHeightList = new List<float>();

    void Start()
    {
        // Get reference to Paddle
        paddle = GetActivePaddle();

        // Initialize Condition and Visit types
        condition = GlobalControl.Instance.condition;
        session = GlobalControl.Instance.session;
        degreesOfFreedom = GlobalControl.Instance.degreesOfFreedom;
        ballResetHoverSeconds = GlobalControl.Instance.ballResetHoverSeconds;

        // Calibrate the target line to be at the player's eye level
        Vector3 targetPos = targetLine.transform.position;
        targetLine.transform.position = new Vector3(
            targetPos.x, 
            AdjustTargetHeightPreference(hmd.transform.position.y), 
            targetPos.z
        );
        GetComponent<ExplorationMode>().CalibrateEyeLevel(targetLine.transform.position.y);

        maxTrials = GlobalControl.Instance.maxTrialCount;

        if (GlobalControl.Instance.numPaddles > 1)
        {
            rightPaddle.GetComponent<Paddle>().EnablePaddle();
            rightPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.RIGHT);

            leftPaddle.GetComponent<Paddle>().EnablePaddle();
            leftPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.LEFT);
        }
    }

    private float AdjustTargetHeightPreference(float y)
    {
        switch(GlobalControl.Instance.targetHeightPreference)
        {
            case TargetHeight.DEFAULT:
                return y;
            case TargetHeight.RAISED:
                return 1.1f * y;
            case TargetHeight.LOWERED:
                return 0.9f * y;
            default:
                Debug.Log("Error: Invalid Target Height Preference");
                return y;
        }
    }

    void FixedUpdate()
    {        
        // Record continuous ball & paddle info
        GatherContinuousData();

        // Turn ball green if it is within target area
        if (HeightInsideTargetWindow(ball.transform.position.y) && ball.GetComponent<Ball>().isBouncing)
        {
            ball.GetComponent<Ball>().TurnBallGreen();
        }
        else
        {
            ball.GetComponent<Ball>().TurnBallWhite();
        }

        // Update Canvas display
        feedbackCanvas.UpdateScoreText(curScore, numBounces);

        // Record list of heights for bounce data analysis
        if (ball.GetComponent<Ball>().isBouncing)
        {
            bounceHeightList.Add(ball.transform.position.y);
        }

        // Reset ball if it drops 
        HoverOnReset();

        // Check if game should end
        if ((maxTrials > 0) && (trialNum > maxTrials))
        {
            Debug.Log("Max trials exceeded, quitting");
            Application.Quit();
        }
    }

    // Holds the ball over the paddle at Target Height for 0.5 seconds, then releases
    public void HoverOnReset()
    {
        if (!inHoverMode)
        {
            // Check if ball is on ground
            if (ball.transform.position.y < ball.transform.localScale.y)
            {
                inHoverMode = true;
            }
        }
        else // if hovering
        {
            Vector3 paddlePosition = new Vector3(
                paddle.transform.position.x, 
                targetLine.transform.position.y, 
                paddle.transform.position.z
            );


            // Hover ball at target line for a second
            StartCoroutine(PlayDropSound(0.9f));
            StartCoroutine(ReleaseHoverOnReset(ballResetHoverSeconds));

            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.transform.position = paddlePosition;
        }
    }

    // Drops ball after reset
    IEnumerator ReleaseHoverOnReset(float time)
    {
        if (inHoverResetCoroutine)
        {
            yield break;
        }
        inHoverResetCoroutine = true;

        yield return new WaitForSeconds(time);
        
        // Stop hovering
        inHoverMode = false;
        inHoverResetCoroutine = false;
        inPlayDropSoundRoutine = false;

        // Reset trial
        ResetTrial();
    }

    // Play drop sound
    IEnumerator PlayDropSound(float time)
    {
        if (inPlayDropSoundRoutine)
        {
            yield break;
        }
        inPlayDropSoundRoutine = true;
        yield return new WaitForSeconds(time);

        GetComponent<DropSoundPlayer>().PlayDropSound();
    }

    // Returns true if the ball is within the target line boundaries.
    private bool HeightInsideTargetWindow(float height)
    {
        float targetHeight = targetLine.transform.position.y;
        float lowerLimit = targetHeight - targetRadius;
        float upperLimit = targetHeight + targetRadius;

        return (height > lowerLimit) && (height < upperLimit);
    }

    // This will be called when the ball successfully bounces on the paddle.
    public void BallBounced(Collision c)
    {
        if (numBounces < 1)
        {
            SetUpPaddleData();
        }
        else
        {
            GatherBounceData();
            SetUpPaddleData();
        }
        numBounces++;

        // If the user bounced enough times, kick in an exploration
        // effect (if turned on).
        curGroupNum++;
        if (curGroupNum > bounceGroupSize)
        {
            ResetBounceGroup();
            curGroupNum = 1;
        }

        // If there are two paddles, switch the active one
        if (GlobalControl.Instance.numPaddles > 1)
        {
            StartCoroutine(WaitToSwitchPaddles(c));
        }
    }

    // The ball was reset after hitting the ground. Reset bounce and score.
    public void ResetTrial()
    {
        // Don't run this code the first time the ball is reset or when there are 0 bounces
        if (trialNum < 1 || numBounces < 1)
        {
            trialNum++;
            return;
        }

        // Record data for final bounce in trial
        GatherBounceData();

        // Record Trial Data from last trial
        GetComponent<DataHandler>().recordTrial(condition, session, degreesOfFreedom, Time.time, trialNum, numBounces, numAccurateBounces);

        trialNum++;
        numBounces = 0;
        numAccurateBounces = 0;
        curScore = 0f;
    }

    // Determine data for recording a bounce and finally, record it.
    private void GatherBounceData()
    {
        float apexHeight = Mathf.Max(bounceHeightList.ToArray());
        float apexTargetError = Mathf.Abs(targetLine.transform.position.y - apexHeight);

        bool apexSuccess = HeightInsideTargetWindow(apexHeight);

        // If the apex of the bounce was inside the target window, increase the score
        if (apexSuccess)
        {
            curScore = curScore + 10;
            numAccurateBounces++;
            ball.GetComponent<BallParticleSpawner>().SpawnSuccessParticles();
        }

        //Record Data from last bounce
        GetComponent<DataHandler>().recordBounce(condition, session, degreesOfFreedom, trialNum, numBounces, apexTargetError,
            paddleBounceVelocity, paddleBounceAccel);

        bounceHeightList = new List<float>();
    }

    // Grab ball and paddle info and record it. Should be called once per frame
    private void GatherContinuousData()
    {        
        float paddleVelocity = paddle.GetComponent<Paddle>().GetVelocity().magnitude;
        float paddleAccel = paddle.GetComponent<Paddle>().GetAcceleration();

        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;

        GetComponent<DataHandler>().recordContinuous(condition, session, degreesOfFreedom,
            Time.time, ballVelocity.x, ballVelocity.y, ballVelocity.z, paddleVelocity, paddleAccel);
    }

    // Initialize paddle information to be recorded upon next bounce
    private void SetUpPaddleData()
    {
        GameObject paddle = GetActivePaddle();

        paddleBounceHeight = paddle.transform.position.y;
        paddleBounceVelocity = paddle.GetComponent<Paddle>().GetVelocity().magnitude;
        paddleBounceAccel = paddle.GetComponent<Paddle>().GetAcceleration();
    }

    // If 5 trials or so have passed, make a change to the game and reset the group.
    private void ResetBounceGroup()
    {
        if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.TASK)
        {
            // Move the target to a different location
            GetComponent<ExplorationMode>().MoveTargetLine();
        }
        else if (GlobalControl.Instance.explorationMode == GlobalControl.ExplorationMode.FORCED)
        {
            // Change game physics
            GetComponent<ExplorationMode>().ModifyBouncePhysics();
        }
        else
        {
            // Don't do anything, there is no exploration mode set
        }
    }

    // In order to prevent bugs, wait a little bit for the paddles to switch
    IEnumerator WaitToSwitchPaddles(Collision c)
    {
        yield return new WaitForSeconds(0.1f);
        // We need the paddle identifier. This is the second parent of the collider in the heirarchy.
        SwitchPaddles(c.gameObject.transform.parent.transform.parent.GetComponent<Paddle>().GetPaddleIdentifier());
    }

    // Switch the active paddles
    private void SwitchPaddles(Paddle.PaddleIdentifier paddleId)
    {
        if (paddleId == Paddle.PaddleIdentifier.LEFT)
        {
            leftPaddle.GetComponent<Paddle>().DisablePaddle();
            rightPaddle.GetComponent<Paddle>().EnablePaddle();
        }
        else
        {
            leftPaddle.GetComponent<Paddle>().EnablePaddle();
            rightPaddle.GetComponent<Paddle>().DisablePaddle();
        }
     }

    // Finds the currently active paddle (in the case of two paddles)
    private GameObject GetActivePaddle()
    {
        if (leftPaddle.GetComponent<Paddle>().ColliderIsActive())
        {
            return leftPaddle;
        }
        else
        {
            return rightPaddle;
        }
    }

    void OnApplicationQuit()
    {
        // This is to ensure that the final trial is recorded.
        ResetTrial();
    }
}
