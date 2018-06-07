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
    // Current score during this trial
    private float curScore = 0f;

    // The current trial number. This is increased by one every time the ball is reset.
    private int trialNum = 0;

    // A group of trials. The current group number will tick up every trial.
    // When curGroupNum reaches trialGroupSize, it will reset back to 1.
    private int trialGroupSize = 5;
    private int curGroupNum = 1;

    // The paddle bounce height, velocity, and acceleration to be recorded on each bounce.
    // These are the values on the *paddle*, NOT the ball
    private float paddleBounceHeight;
    private float paddleBounceVelocity;
    private float paddleBounceAccel;

    private List<float> bounceHeightList = new List<float>();

    void Start()
    {
        // Calibrate the target line to be at the player's eye level
        Vector3 targetPos = targetLine.transform.position;
        targetLine.transform.position = new Vector3(targetPos.x, hmd.transform.position.y, targetPos.z);
        GetComponent<ExplorationMode>().CalibrateEyeLevel(targetLine.transform.position.y);

        if (GlobalControl.Instance.numPaddles > 1)
        {
            rightPaddle.GetComponent<Paddle>().EnablePaddle();
            leftPaddle.GetComponent<Paddle>().DisablePaddle();
        }
    }

    void Update()
    {
        // Turn ball green if it is within target area
        if (HeightInsideTargetWindow(ball.transform.position.y) && ball.GetComponent<Ball>().isBouncing)
        {
            ball.GetComponent<Ball>().TurnBallGreen();
        }
        else
        {
            ball.GetComponent<Ball>().TurnBallWhite();
        }

        feedbackCanvas.UpdateScoreText(curScore, numBounces);

        // Record list of heights for bounce data analysis
        if (ball.GetComponent<Ball>().isBouncing)
        {
            bounceHeightList.Add(ball.transform.position.y);
        }
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
    public void BallBounced()
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

        // If there are two paddles, switch the active one
        if (GlobalControl.Instance.numPaddles > 1)
        {
            StartCoroutine(WaitToSwitchPaddles());
        }
    }

    // The ball was picked up and reset by the controller. Reset bounce and score.
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
        GetComponent<DataHandler>().recordTrial(Time.time, trialNum, numBounces, curScore,
            targetLine.transform.position.y, targetRadius);

        trialNum++;
        numBounces = 0;
        curScore = 0f;

        curGroupNum++;
        if (curGroupNum > trialGroupSize)
        {
            ResetTrialGroup();
            curGroupNum = 1;
        }
    }

    // Determine data for recording a bounce and finally, record it.
    private void GatherBounceData()
    {
        float apexHeight = Mathf.Max(bounceHeightList.ToArray());
        float apexTargetDistance = Mathf.Abs(targetLine.transform.position.y - apexHeight);

        bool apexSuccess = HeightInsideTargetWindow(apexHeight);

        // If the apex of the bounce was inside the target window, increase the score
        if (apexSuccess)
        {
            curScore = curScore + 10;
        }

        //Record Data from last bounce
        GetComponent<DataHandler>().recordBounce(Time.time, trialNum, numBounces, apexHeight, apexTargetDistance,
            apexSuccess, paddleBounceHeight, paddleBounceVelocity, paddleBounceAccel);

        bounceHeightList = new List<float>();
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
    private void ResetTrialGroup()
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
    IEnumerator WaitToSwitchPaddles()
    {
        yield return new WaitForSeconds(0.1f);
        SwitchPaddles();
    }

    // Switch the active paddles
    private void SwitchPaddles()
    {
        if (leftPaddle.GetComponent<Paddle>().ColliderIsActive())
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
