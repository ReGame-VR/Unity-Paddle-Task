using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Labs.SuperScience;
using Valve.VR;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

public class PaddleGame : MonoBehaviour
{

	[Tooltip("The head mounted display")]
	[SerializeField]
	private GameObject hmd;

	[Tooltip("The left paddle in the game")]
	[SerializeField]
	private GameObject leftPaddle;

	[Tooltip("The right paddle in the game")]
	[SerializeField]
	private GameObject rightPaddle;

	[Tooltip("The right paddle in the game")]
	[SerializeField]
	private bool useLeft;

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
	private float targetRadius = 0.05f; // get from GlobalControl

	[Tooltip("A reference to the Time to Drop countdown display quad")]
	[SerializeField]
	private GameObject timeToDropQuad;

	[Tooltip("A reference to the Time to Drop countdown display text")]
	[SerializeField]
	private Text timeToDropText;

	[SerializeField, Tooltip("Handles the ball sound effects")]
	private BallSoundPlayer ballSoundPlayer;

	// Current number of bounces that the player has acheieved in this trial
	private int numBounces = 0;
	private int numAccurateBounces = 0;
	// Current score during this trial
	private float curScore = 0f;

	// Running total number of bounces this instance
	private int numTotalBounces = 0;

	// The current trial number. This is increased by one every time the ball is reset.
	public int trialNum = 0;

	public EffectController effectController;

	// If 3 of the last 10 bounces were successful, update the exploration mode physics 
	private const int EXPLORATION_MAX_BOUNCES = 10;
	private const int EXPLORATION_SUCCESS_THRESHOLD = 6;
	private int explorationModeBounces;
	private int explorationModeSuccesses;
	private CircularBuffer<bool> explorationModeBuffer = new CircularBuffer<bool>(EXPLORATION_MAX_BOUNCES);

	// The paddle bounce height, velocity, and acceleration to be recorded on each bounce.
	// These are the values on the *paddle*, NOT the ball
	private float paddleBounceHeight;
	private Vector3 paddleBounceVelocity;
	private Vector3 paddleBounceAccel;

	// Degrees of freedom, how many degrees in x-z directions ball can bounce after hitting paddle
	// 0 degrees: ball can only bounce in y direction, 90 degrees: no reduction in range
	public float degreesOfFreedom;

	// This session information
	private Condition condition;
	private ExpCondition expCondition;
	private Session session;
	private DifficultyEvaluation difficultyEvaluation;
	private int maxTrialTime;
	private float hoverTime;

	// Variables to keep track of resetting the ball after dropping to the ground
	GameObject paddle;
	private bool inHoverMode = false;
	private bool inHoverResetCoroutine = false;
	private bool inPlayDropSoundRoutine = false;
	private int ballResetHoverSeconds = 3;
	private bool inRespawnMode = false;
	private int ballRespawnSeconds = 1;

	// Variables for countdown timer display
	public int countdown;
	private bool inCoutdownCoroutine = false;

	// Timescale
	public bool slowtime = false;

	// Reference to Paddle PhysicsTracker via Ball script
	PhysicsTracker m_MotionData;

	private List<float> bounceHeightList = new List<float>();

	int difficultyEvaluationTrials = GlobalControl.Instance.difficultyEvaluationTrials;
	int difficultyChangedSuspension = GlobalControl.Instance.difficultyChangedSuspension;
	int trialDifficultyChanged = 0;
	int successLastTrials = 10;

	private Dictionary<DifficultyEvaluation, List<TrialSetData>> trialSetDatas = new Dictionary<DifficultyEvaluation, List<TrialSetData>>()
	{
		{ DifficultyEvaluation.BASE, new List<TrialSetData>() },
		{ DifficultyEvaluation.MODERATE, new List<TrialSetData>() },
		{ DifficultyEvaluation.MAXIMAL, new List<TrialSetData>() },
		{ DifficultyEvaluation.CUSTOM, new List<TrialSetData>() }
	};
	private List<ScoreEffects> scoreEffects = new List<ScoreEffects>();
	private List<DifficultyEvaluation> difficultyEvaluationOrder = new List<DifficultyEvaluation>();
	private int difficultyEvaluationIndex = 0;

	private Dictionary<DifficultyEvaluation, int> evaluatedDifficulties = new Dictionary<DifficultyEvaluation, int>() 
	{ 
		{ DifficultyEvaluation.BASE, - 1 },
		{ DifficultyEvaluation.MODERATE, - 1 },
		{ DifficultyEvaluation.MAXIMAL, - 1 }
	};

	int scoreEffectTarget = 0;
	bool maxScoreEffectReached = false;

	GlobalControl globalControl;
	DataHandler dataHandler;

	void Start()
	{
		globalControl = GlobalControl.Instance;
		dataHandler = GetComponent<DataHandler>();

		Instantiate(globalControl.environments[globalControl.environmentOption]);

		// Get reference to Paddle
		paddle = GetActivePaddle();
		useLeft = false;

		m_MotionData = ball.GetComponent<Ball>().m_MotionData;

		// Calibrate the target line to be at the player's eye level
		SetTargetLineHeight();
		targetRadius = globalControl.targetHeightEnabled ? globalControl.targetRadius : 0f;

		if (globalControl.numPaddles > 1)
		{
			rightPaddle.GetComponent<Paddle>().EnablePaddle();
			rightPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.RIGHT);

			leftPaddle.GetComponent<Paddle>().EnablePaddle();
			leftPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.LEFT);
		}

		Initialize();
	}

	private void Initialize()
	{
		// Initialize Condition and Visit types
		condition = globalControl.condition;
		expCondition = globalControl.expCondition;
		session = globalControl.session;
		maxTrialTime = globalControl.maxTrialTime;
		hoverTime = globalControl.ballResetHoverSeconds;
		degreesOfFreedom = globalControl.degreesOfFreedom;
		ballResetHoverSeconds = globalControl.ballResetHoverSeconds;
		difficultyEvaluation = globalControl.difficultyEvaluation;

		if (globalControl.recordingData)
		{
			StartRecording();
		}

		int difficulty = 0;
		if (globalControl.difficultyEvaluation == DifficultyEvaluation.BASE)
		{
			// difficulty = GetDifficulty()
		}
		else if (globalControl.difficultyEvaluation == DifficultyEvaluation.CUSTOM)
		{

		}

		ChangeDifficulty(globalControl.difficulty);

		if (globalControl.targetHeightEnabled == false) targetLine.SetActive(false);

		effectController.dissolve.effectTime = ballRespawnSeconds;
		effectController.respawn.effectTime = hoverTime;

		PopulateScoreEffects();

		if (globalControl.session == Session.ACQUISITION)
		{
			difficultyEvaluationOrder = new List<DifficultyEvaluation>() { DifficultyEvaluation.BASE, DifficultyEvaluation.MODERATE, DifficultyEvaluation.MAXIMAL, DifficultyEvaluation.MODERATE };

			difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
		}
	}

	void Update()
	{
		// Data handler. Record continuous ball & paddle info
		GatherContinuousData();

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
		CheckEndCondition();

		// add time adjust jl controls for demonstrations
		if (Input.GetKeyDown(KeyCode.N))
		{
			globalControl.timescale = Mathf.Clamp(globalControl.timescale - .05f, .05f, 3f);
			Time.timeScale = globalControl.timescale;
			Debug.Log("reduced timescale to " + globalControl.timescale);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			globalControl.timescale = Mathf.Clamp(globalControl.timescale + .05f, .05f, 3f);
			Time.timeScale = globalControl.timescale;
			Debug.Log("increased timescale to " + globalControl.timescale);

		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			curScore -= 25f;
			BallBounced(null);
			Debug.Log("Score decreased");
		}
		if (Input.GetKeyDown(KeyCode.I))
		{
			curScore += 25f;
			BallBounced(null);
			Debug.Log("Score increased");
		}
	}

	void OnApplicationQuit()
	{
		// This is to ensure that the final trial is recorded.
		ResetTrial();
	}

	public void StartRecording()
	{
		// Record session data
		dataHandler.recordHeaderInfo(condition, expCondition, session, maxTrialTime, hoverTime, targetRadius);

	}

	public void SwapActivePaddle()
	{
		SteamVR_Behaviour_Pose paddlePose = GameObject.Find("Paddle").GetComponent<SteamVR_Behaviour_Pose>();
		useLeft = !useLeft;

		if (useLeft)
		{
			paddlePose.inputSource = SteamVR_Input_Sources.LeftHand;
		}
		else
		{
			paddlePose.inputSource = SteamVR_Input_Sources.RightHand;
		}
	}

	// Sets Target Line height based on HMD eye level and target position preference
	public void SetTargetLineHeight()
	{
		Vector3 tlPosn = targetLine.transform.position;

		float x = tlPosn.x;
		float z = tlPosn.z;
		float y = ApplyInstanceTargetHeightPref(GetHmdHeight());

		targetLine.transform.position = new Vector3(x, y, z);

		// Update Exploration Mode height calibration
		GetComponent<ExplorationMode>().CalibrateEyeLevel(targetLine.transform.position.y);
	}

	private float ApplyInstanceTargetHeightPref(float y)
	{
		switch (globalControl.targetHeightPreference)
		{
			case TargetHeight.RAISED:
				y *= 1.1f;
				break;
			case TargetHeight.LOWERED:
				y *= 0.9f;
				break;
			case TargetHeight.DEFAULT:
				break;
			default:
				Debug.Log("Error: Invalid Target Height Preference");
				break;
		}
		return y;
	}

	// Toggles the timescale to make the game slower 
	public void ToggleTimescale()
	{
		slowtime = !slowtime;

		if (slowtime)
		{
			Time.timeScale = globalControl.timescale; // 0.7f;
		}
		else
		{
			Time.timeScale = 1.0f;
		}
	}

	// Holds the ball over the paddle at Target Height for 0.5 seconds, then releases
	public void HoverOnReset()
	{
		if (!inHoverMode)
		{
			if (!inRespawnMode)
			{
				Time.timeScale = globalControl.timescale;
			}

			timeToDropQuad.SetActive(false);

			// Check if ball is on ground
			if (!inRespawnMode && ball.transform.position.y < ball.transform.localScale.y)
			{
				// inHoverMode = true;
				inRespawnMode = true;
				StartCoroutine(Respawning());
			}
		}
		else // if hovering
		{
			timeToDropQuad.SetActive(true);

			ball.GetComponent<SphereCollider>().enabled = false;

			// Hover ball at target line for a second
			StartCoroutine(PlayDropSound(ballResetHoverSeconds - 0.15f));
			StartCoroutine(ReleaseHoverOnReset(ballResetHoverSeconds));

			// Start countdown timer 
			StartCoroutine(UpdateTimeToDropDisplay());

			ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
			ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			ball.transform.position = Ball.spawnPosition(targetLine);
			ball.transform.rotation = Quaternion.identity;

			Time.timeScale = 1f;
		}
	}

	IEnumerator Respawning()
	{
		Time.timeScale = 1f;
		effectController.StopAllParticleEffects();
		effectController.StartEffect(effectController.dissolve);
		yield return new WaitForSeconds(ballRespawnSeconds);
		inRespawnMode = false;
		inHoverMode = true;
		yield return new WaitForEndOfFrame();
		effectController.StopParticleEffect(effectController.dissolve);
		effectController.StartEffect(effectController.respawn);
		yield return new WaitForSeconds(ballRespawnSeconds);
		ball.GetComponent<Ball>().TurnBallWhite();
		//yield return new WaitForSeconds(ballResetHoverSeconds);
		// Debug.Log("4");

		// yield return null;
		// Debug.Log("5");

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

		ball.GetComponent<SphereCollider>().enabled = true;

		// Reset trial
		ResetTrial();
	}

	// Update time to drop
	IEnumerator UpdateTimeToDropDisplay()
	{
		if (inCoutdownCoroutine)
		{
			yield break;
		}
		inCoutdownCoroutine = true;

		countdown = (int)ballResetHoverSeconds;

		while (countdown >= 1.0f)
		{
			timeToDropText.text = countdown.ToString();
			countdown--;
			yield return new WaitForSeconds(1.0f);
		}

		inCoutdownCoroutine = false;
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

		ballSoundPlayer.PlayDropSound();
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
		numTotalBounces++;

		// If there are two paddles, switch the active one
		if (globalControl.numPaddles > 1)
		{
			StartCoroutine(WaitToSwitchPaddles(c));
		}

		if (!maxScoreEffectReached && curScore >= scoreEffects[scoreEffectTarget].score)
		{
			Debug.LogFormat("max score for effects not reached. current score {0} is greater than the target {1} for effect {2}. startign effects, increasing score target...", curScore, scoreEffectTarget, scoreEffects[scoreEffectTarget].score);

			foreach (var disableEffect in scoreEffects[scoreEffectTarget].disableEffects)
			{
				effectController.StopParticleEffect(disableEffect);
			}
			effectController.StartEffect(scoreEffects[scoreEffectTarget].effect);
			ballSoundPlayer.PlayEffectSound(scoreEffects[scoreEffectTarget].audioClip);

			if (scoreEffectTarget + 1 >= scoreEffects.Count)
			{
				maxScoreEffectReached = true;
				Debug.LogFormat("max effect score reached, score is {0} and score target was {1}", curScore, scoreEffects[scoreEffectTarget].score);
			}
			else
			{
				scoreEffectTarget++;
			}
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

		if (globalControl.recordingData)
		{
			// Record Trial Data from last trial
			dataHandler.recordTrial(degreesOfFreedom, Time.time, trialNum, numBounces, numAccurateBounces, difficultyEvaluation);
			CheckDifficulty();
		}

		trialNum++;
		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;
	}

	// Determine data for recording a bounce and finally, record it.
	private void GatherBounceData()
	{
		float apexHeight = Mathf.Max(bounceHeightList.ToArray());
		float apexTargetError = globalControl.targetHeightEnabled ? (apexHeight - targetLine.transform.position.y) : 0;

		bool apexSuccess = globalControl.targetHeightEnabled ? GetHeightInsideTargetWindow(apexHeight) : false;

		// If the apex of the bounce was inside the target window, increase the score
		if (apexSuccess)
		{
			curScore += 10;
			numAccurateBounces++;

			// IndicateSuccessBall(); // temporariliy disabled while testing apex coroutines in Ball
		}

		//Record Data from last bounce
		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

		if (globalControl.recordingData)
		{
			dataHandler.recordBounce(degreesOfFreedom, Time.time, cbm, trialNum, numBounces, numTotalBounces, apexTargetError, apexSuccess, paddleBounceVelocity, paddleBounceAccel, difficultyEvaluation);
		}

		bounceHeightList = new List<float>();
	}

	// Turns ball green briefly and plays success sound.
	public void IndicateSuccessBall()
	{
		Ball b = ball.GetComponent<Ball>();

		ballSoundPlayer.PlaySuccessSound();

		b.TurnBallGreen();
		StartCoroutine(b.TurnBallWhiteCR(0.3f));
	}

	// Grab ball and paddle info and record it. Should be called once per frame
	private void GatherContinuousData()
	{
		//Vector3 paddleVelocity = paddle.GetComponent<Paddle>().GetVelocity();
		//Vector3 paddleAccel = paddle.GetComponent<Paddle>().GetAcceleration();
		Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
		Vector3 paddleVelocity = m_MotionData.Velocity;
		Vector3 paddleAccel = m_MotionData.Acceleration;

		Vector3 cbm = ball.GetComponent<Ball>().GetBounceModification();

		if (globalControl.recordingData)
		{
			dataHandler.recordContinuous(degreesOfFreedom, Time.time, cbm, globalControl.paused, ballVelocity, paddleVelocity, paddleAccel, difficultyEvaluation);
		}
	}

	// Initialize paddle information to be recorded upon next bounce
	private void SetUpPaddleData()
	{
		GameObject paddle = GetActivePaddle();

		paddleBounceHeight = paddle.transform.position.y;
		//paddleBounceVelocity = paddle.GetComponent<Paddle>().GetVelocity();
		//paddleBounceAccel = paddle.GetComponent<Paddle>().GetAcceleration();
		paddleBounceVelocity = m_MotionData.Velocity;
		paddleBounceAccel = m_MotionData.Acceleration;
	}

	// If 6 of the last 10 bounces were successful, update ExplorationMode physics 
	// bool parameter is whether last bounce was success 
	public void ModifyPhysicsOnSuccess(bool bounceSuccess)
	{
		if (globalControl.explorationMode != GlobalControl.ExplorationMode.FORCED)
		{
			return;
		}

		explorationModeBuffer.Add(bounceSuccess);

		int successes = 0;

		bool[] temp = explorationModeBuffer.GetArray();
		for (int i = 0; i < explorationModeBuffer.length(); i++)
		{
			if (temp[i])
			{
				successes++;
			}
		}

		if (successes >= EXPLORATION_SUCCESS_THRESHOLD)
		{
			// Change game physics
			GetComponent<ExplorationMode>().ModifyBouncePhysics();
			GetComponent<ExplorationMode>().IndicatePhysicsChange();

			// Reset counter
			explorationModeBuffer = new CircularBuffer<bool>(EXPLORATION_MAX_BOUNCES);
			return;
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

	private void CheckDifficulty()
	{
		var trialData = dataHandler.GetTrialData(difficultyEvaluation);
		if (trialData.Count >= trialDifficultyChanged + difficultyChangedSuspension + difficultyEvaluationTrials)
		{
			int bounces = 0, accurateBounces = 0;
			float slopeDenominator = 0f;
			float bounceSlope;

			for (int i = trialData.Count - difficultyEvaluationTrials; i < trialData.Count; i++)
			{
				bounces += trialData[i].numBounces;
				accurateBounces += trialData[i].numAccurateBounces;

				slopeDenominator += i + 1;
			}

			float averageBounces = (float)bounces / (float)difficultyEvaluationTrials;
			float averageAccurateBounces = (float)accurateBounces / (float)difficultyEvaluationTrials;
			bounceSlope = bounces / slopeDenominator;
			Debug.LogFormat("evaluated slope to be {0}", bounceSlope.ToString("F4"));

			trialSetDatas[difficultyEvaluation].Add(new TrialSetData(bounces, averageBounces, accurateBounces, averageAccurateBounces, bounceSlope));


			// increase if trials are progressvely positive, decrease otherwise 
			// ChangeDifficulty(GetDifficultyChange(bounceSlope >= 1) + GetOverallDifficultyModifier());

		}
	}

	public void ChangeDifficulty(float difficulty)
	{
		// TODO reimplement
		return; 
		if (difficulty == globalControl.difficulty)
		{
			// no chnage, returning
			return;
		}

		trialDifficultyChanged = dataHandler.GetTrialData().Count;
	}

	private void EvaluationCompleted()
	{
		difficultyEvaluationIndex++;
		if (difficultyEvaluationIndex >= difficultyEvaluationOrder.Count)
		{
			// evaluations completed
		}
		else
		{
			difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
		}
	}

	private float GetDifficultyChange(bool higher)
	{
		float change = higher ? globalControl.difficultyInterval : globalControl.difficultyInterval * -1;
		return Mathf.Clamp(globalControl.difficulty + change, globalControl.difficultyMin, globalControl.difficultyMax);
	}

	private float GetDifficulty(DifficultyEvaluation evaluation)
	{
		if (evaluation == DifficultyEvaluation.CUSTOM)
		{
			return globalControl.difficulty;
		}
		else if (evaluation == DifficultyEvaluation.BASE)
		{
			return 1;
		}
		else if (evaluation == DifficultyEvaluation.MODERATE)
		{

		}

		return -1;
	}



	/// <summary>
	/// may return a value to additionally increase or decrease current difficulty step based on overall performance. 
	/// </summary>
	/// <returns></returns>
	private float GetOverallDifficultyModifier()
	{
		foreach(var trialSetData in trialSetDatas)
		{
			// evaluate the set
		}

		// create modifier
		return 0;
	}

	void CheckEndCondition()
	{

		if (CheckScoreCondition())
		{
			EvaluateDifficultyResult(true);
		}
		
		if (globalControl.GetTimeLimitSeconds() == 0)
		{
			return;
		}

		if (globalControl.GetTimeElapsed() > globalControl.GetTimeLimitSeconds())
		{
#if !UNITY_EDITOR
			Debug.Log("Time limit of " + globalControl.GetTimeLimitSeconds() + " seconds has passed. Quitting");
#endif
			// Application.Quit();
			EvaluateDifficultyResult(false);
		}
	}

	void EvaluateDifficultyResult(bool successfulCompletion)
	{
		var difficulty = GetPerformanceBasedDifficulty(successfulCompletion);

		if (difficultyEvaluation == DifficultyEvaluation.BASE)
		{

		}
		else if (difficultyEvaluation == DifficultyEvaluation.MODERATE)
		{

		}
		else if (difficultyEvaluation == DifficultyEvaluation.MAXIMAL)
		{

		}
	}

	private float GetPerformanceBasedDifficulty(bool successfulCompletion)
	{
		if (difficultyEvaluationIndex > 0 && difficultyEvaluationIndex < difficultyEvaluationOrder.Count)
		{
			// var difficulty = difficultyEvaluationOrder[difficultyEvaluationIndex - 1];
			var trialSetData = trialSetDatas[difficultyEvaluation];
			// var trialData = dataHandler.GetTrialData(difficultyEvaluation);
			//var trialSetData = 
			TrialSetData bestTrialSet = null;

			if (successfulCompletion)
			{
				bestTrialSet = trialSetData[trialSetData.Count - 1];
			}
			else
			{
				int bestRated = 0;
				foreach (var trialSet in trialSetData)
				{
					bestTrialSet = trialSet;
				}
			}
		}

		return -1;
	}

	float GetEvaluatedDifficultyPercentage()
	{
		// get generic percentage, later changed into relative difficulty

		// primarily check the set that completed the session, or the best set if they timed out. 


		// the earlier data will also be used but to a lesser extent
		// primarily looking at how close they were to the target on average, and how many trials it took them to achieve success or trial end. 

		return -1;
	}

	bool CheckScoreCondition()
	{
		int successTrials = 0, targetConsecutiveBounces = 0, targetAccurateBounces = 0;
		if (difficultyEvaluation == DifficultyEvaluation.BASE)
		{
			successTrials = 7;
			targetConsecutiveBounces = 5;
		}
		else if (difficultyEvaluation == DifficultyEvaluation.MODERATE)
		{
			successTrials = 7;
			targetConsecutiveBounces = 5;
			targetAccurateBounces = 5;

		}
		else if (difficultyEvaluation == DifficultyEvaluation.MAXIMAL)
		{
			successTrials = 7;
			targetConsecutiveBounces = 5;
		}

		if (GetTrialEvaluationTrue(successTrials, successLastTrials, targetConsecutiveBounces, targetAccurateBounces))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	bool GetTrialEvaluationTrue(int successTrials, int lastTrials, int targetConsecutiveBounces, int targetAccurateBounces)
	{
		var data = trialSetDatas[difficultyEvaluation];
		if(data.Count >= lastTrials)
		{
			int succeededTrials = 0;
			for (int i = data.Count - lastTrials; i < data.Count; i++)
			{
				if(data[i].bounces >= targetConsecutiveBounces && data[i].accurateBounces >= targetAccurateBounces)
				{
					succeededTrials++;
				}
			}

			if (succeededTrials >= successTrials)
			{
				return true;
			}
		}

		return false;
	}

	private void PopulateScoreEffects()
	{
		// enter score effects in order of the score needed to trigger them
		scoreEffects.Add(new ScoreEffects(25, effectController.embers, null));
		scoreEffects.Add(new ScoreEffects(50, effectController.fire, null));
		scoreEffects.Add(new ScoreEffects(75, effectController.blueEmbers, null, new List<Effect>() { effectController.embers }));
		scoreEffects.Add(new ScoreEffects(100, effectController.blueFire, null, new List<Effect>() { effectController.fire }));


		int highestScore = 0;
		for(int i = 0; i < scoreEffects.Count; i++)
		{
			if (scoreEffects[i].score > highestScore)
			{
				highestScore = scoreEffects[i].score;
			}
			else
			{
				// could create a sorting algorithm but it's a bit more work to deal with the custom class. user input in the correct order not that hard though. 
				Debug.LogErrorFormat("ERROR! Invalid Score order entered, must be in ascending order. Entry {0} had score {1}, lower than the minimum {2}", i, scoreEffects[i], highestScore);
			}
		}

		scoreEffectTarget = 0;
	}

	private float GetHmdHeight()
	{
		return hmd.transform.position.y;
	}

	// Returns true if the ball is within the target line boundaries.
	public bool GetHeightInsideTargetWindow(float height)
	{
		if (!globalControl.targetHeightEnabled) return false;

		float targetHeight = targetLine.transform.position.y;
		float lowerLimit = targetHeight - targetRadius;
		float upperLimit = targetHeight + targetRadius;

		return (height > lowerLimit) && (height < upperLimit);
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
}
