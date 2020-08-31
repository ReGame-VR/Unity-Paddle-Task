using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Labs.SuperScience;
using Valve.VR;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine.SceneManagement;

public class PaddleGame : MonoBehaviour
{	
	private const int difficultyMin = 1, difficultyMax = 10;

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

	[SerializeField]
	AudioClip feedbackExample;

	[SerializeField]
	AudioSource feedbackSource, difficultySource;

	[SerializeField]
	TextMeshPro difficultyDisplay;

	[SerializeField]
	TextMeshPro highestBouncesDisplay;

	[SerializeField]
	TextMeshPro highestAccurateBouncesDisplay;

	/// <summary>
	/// list of the audio clips played at the beginning of difficulties in some cases
	/// </summary>
	[SerializeField]
	List<DifficultyAudioClip> difficultyAudioClips;

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

	[SerializeField]
	private GlobalPauseHandler pauseHandler;

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

	int difficultyEvaluationTrials;
	int trialDifficultyChanged = 0;
	private List<ScoreEffect> scoreEffects = new List<ScoreEffect>();
	private List<DifficultyEvaluation> difficultyEvaluationOrder = new List<DifficultyEvaluation>() 
	{ 
		DifficultyEvaluation.BASE, 
		DifficultyEvaluation.MODERATE, 
		DifficultyEvaluation.MAXIMAL, 
		DifficultyEvaluation.MODERATE 
	};
	private int difficultyEvaluationIndex = 0;

	int scoreEffectTarget = 0;
	bool maxScoreEffectReached = false;

	int difficulty;
	List<TrialCondition> trialConditions = new List<TrialCondition>();
	TrialCondition baseTrialCondition, moderateTrialCondition, maximaltrialCondition;
	private List<DifficultyEvaluationData<TrialData>> trialData = new List<DifficultyEvaluationData<TrialData>>();
	// {
		//{ DifficultyEvaluation.BASE, new List<TrialData>() },
		//{ DifficultyEvaluation.MODERATE, new List<TrialData>() },
		//{ DifficultyEvaluation.MAXIMAL, new List<TrialData>() },
		//{ DifficultyEvaluation.CUSTOM, new List<TrialData>() }
	// };

	private Dictionary<DifficultyEvaluation, int> targetConditionBounces = new Dictionary<DifficultyEvaluation, int>()
	{
		{ DifficultyEvaluation.BASE, 5 },
		{ DifficultyEvaluation.MODERATE, 5 },
		{ DifficultyEvaluation.MAXIMAL, 5 },
		{ DifficultyEvaluation.CUSTOM, -1 }
	};
	private Dictionary<DifficultyEvaluation, int> targetConditionAccurateBounces = new Dictionary<DifficultyEvaluation, int>()
	{
		{ DifficultyEvaluation.BASE, 0 },
		{ DifficultyEvaluation.MODERATE, 5 },
		{ DifficultyEvaluation.MAXIMAL, 0 },
		{ DifficultyEvaluation.CUSTOM, -1 }
	};

	private List<int> performanceDifficulties = new List<int>();
	float trialDuration = 0;

	int difficultyExampleValue = 2;
	float difficultyExampleTime = 30f;

	int highestBounces, highestAccurateBounces;

	GlobalControl globalControl;
	DataHandler dataHandler;

	void Start()
	{
		globalControl = GlobalControl.Instance;
		dataHandler = GetComponent<DataHandler>();

		difficultyEvaluationTrials = globalControl.difficultyEvaluationTrials;

		Instantiate(globalControl.environments[globalControl.environmentOption]);

		if(globalControl.session == Session.BASELINE)
		{
			performanceDifficulties.Add(1);
		}
		else
		{
			performanceDifficulties.Add(globalControl.difficulty);
		}

		// Get reference to Paddle
		paddle = GetActivePaddle();
		useLeft = false;

		m_MotionData = ball.GetComponent<Ball>().m_MotionData;

		// Calibrate the target line to be at the player's eye level
		SetTargetLineHeight(globalControl.targetLineHeightOffset);
		targetRadius = globalControl.targetHeightEnabled ? globalControl.targetRadius : 0f;

		if (globalControl.numPaddles > 1)
		{
			rightPaddle.GetComponent<Paddle>().EnablePaddle();
			rightPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.RIGHT);

			leftPaddle.GetComponent<Paddle>().EnablePaddle();
			leftPaddle.GetComponent<Paddle>().SetPaddleIdentifier(Paddle.PaddleIdentifier.LEFT);
		}

		PopulateScoreEffects();

		InitializeTrialConditions();

		if(globalControl.session == Session.SHOWCASE)
		{
			globalControl.recordingData = false;
			globalControl.maxTrialTime = 0;


		}
	
		Initialize();

		SetDifficulty(difficulty);
		// difficulty shifts timescale, so pause it again
		Time.timeScale = 0;

		globalControl.ResetTimeElapsed();

		pauseHandler.Pause();


		// countdown for first drop
		// ResetTrial();
		// inRespawnMode = true;
	}

	void Update()
	{
		if(Time.timeScale == 0)
		{
			// no processing until unpaused
			return;
		}

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

#if UNITY_EDITOR
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
		if (Input.GetKeyDown(KeyCode.Q))
		{
			QuitTask();
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			globalControl.timeElapsed += 60;
		}
		if (Input.GetKeyDown(KeyCode.J))
		{
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			if(session == Session.BASELINE)
			{
				numBounces += targetConditionBounces[difficultyEvaluation] * 7;
				numAccurateBounces += targetConditionBounces[difficultyEvaluation] * 7;
			}
		}
#endif

		if (globalControl.recordingData)
		{
			trialDuration += Time.deltaTime;
		}

		if (globalControl.GetTimeElapsed() > GetMaxDifficultyTrialTime(difficultyEvaluation) /*globalControl.GetTimeLimitSeconds()*/)
		{
#if !UNITY_EDITOR
			Debug.Log("Time limit of " + globalControl.GetTimeLimitSeconds() + " seconds has passed. Quitting");
#endif

			Debug.LogFormat("time elapsed {0} greater than max trial time {1}", globalControl.GetTimeElapsed(), GetMaxDifficultyTrialTime(difficultyEvaluation));
			EvaluateDifficultyResult(false);
		}
	}

	/// <summary>
	/// Stop the task, write data and return to the start screen
	/// </summary>
	void QuitTask()
	{
		// This is to ensure that the final trial is recorded.
		ResetTrial(true);

		dataHandler.WriteDataToFiles();

		// clean DDoL objects and return to the start scene
		Destroy(GlobalControl.Instance.gameObject);
		Destroy(gameObject);

		SceneManager.LoadScene(0);
	}

	#region Initialization

	public void Initialize()
	{
		// Initialize Condition and Visit types
		condition = globalControl.condition;
		expCondition = globalControl.expCondition;
		session = globalControl.session;
		maxTrialTime = globalControl.maxTrialTime;
		hoverTime = globalControl.ballResetHoverSeconds;
		degreesOfFreedom = globalControl.degreesOfFreedom;
		ballResetHoverSeconds = globalControl.ballResetHoverSeconds;

		if (globalControl.recordingData)
		{
			StartRecording();
		}

		if (globalControl.targetHeightEnabled == false) targetLine.SetActive(false);

		effectController.dissolve.effectTime = ballRespawnSeconds;
		effectController.respawn.effectTime = hoverTime;

		curScore = 0;

		if (globalControl.session == Session.BASELINE)
		{
			difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
			difficulty = GetDifficulty(difficultyEvaluationOrder[difficultyEvaluationIndex]);
#if UNITY_EDITOR
			// difficulty = 10;
#endif
			difficultyDisplay.text = difficulty.ToString();
			trialData.Add(new DifficultyEvaluationData<TrialData>(difficultyEvaluationOrder[difficultyEvaluationIndex], new List<TrialData>()));
			dataHandler.InitializeDifficultyEvaluationData(difficultyEvaluationOrder[difficultyEvaluationIndex]);

		}
		else if (globalControl.session == Session.SHOWCASE)
		{
			difficulty = 2;
			StartShowcase();
		}
		else
		{
			difficulty = globalControl.difficulty;
			trialData.Add(new DifficultyEvaluationData<TrialData>(DifficultyEvaluation.CUSTOM, new List<TrialData>()));
			dataHandler.InitializeDifficultyEvaluationData(DifficultyEvaluation.CUSTOM);
		}

		// difficulty shifts timescale, so pause it again
		Time.timeScale = 0;

		globalControl.ResetTimeElapsed();

		pauseHandler.Pause();

		highestBounces = 0;
		highestAccurateBounces = 0;
		UpdateHighestBounceDisplay();
		feedbackCanvas.UpdateScoreText(curScore, numBounces);

		// ensure drop time on first drop
		inHoverMode = true;
		effectController.StopAllParticleEffects();

		Debug.Log("Initialized");
	}

	// Sets Target Line height based on HMD eye level and target position preference
	public void SetTargetLineHeight(float offset)
	{
		Vector3 tlPosn = targetLine.transform.position;

		float x = tlPosn.x;
		float z = tlPosn.z;
		float y = ApplyInstanceTargetHeightPref(GetHmdHeight()) + offset;

		targetLine.transform.position = new Vector3(x, y, z);

		// Update Exploration Mode height calibration
		GetComponent<ExplorationMode>().CalibrateEyeLevel(targetLine.transform.position.y);

		var kinematics = ball.GetComponent<Kinematics>();
		if (kinematics)
		{
			kinematics.storedPosition = Ball.spawnPosition(targetLine);
		}
		ball.transform.position = Ball.spawnPosition(targetLine);
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

	/// <summary>
	/// note that score exists as a vestige. it is tracked internally to allow for these effects but will not be shown to the user
	/// </summary>
	private void PopulateScoreEffects()
	{
		// enter score effects in ascending order of the score needed to trigger them
		scoreEffects.Add(new ScoreEffect(25, effectController.embers, null));
		scoreEffects.Add(new ScoreEffect(50, effectController.fire, null));
		scoreEffects.Add(new ScoreEffect(75, effectController.blueEmbers, null, new List<Effect>() { effectController.embers }));
		scoreEffects.Add(new ScoreEffect(100, effectController.blueFire, null, new List<Effect>() { effectController.fire }));


		int highestScore = 0;
		for(int i = 0; i < scoreEffects.Count; i++)
		{
			if (scoreEffects[i].score > highestScore)
			{
				highestScore = scoreEffects[i].score;
			}
			else
			{
				// could create a sorting algorithm but it's a bit more work to deal with the custom class. dev input in the correct order will be sufficient
				Debug.LogErrorFormat("ERROR! Invalid Score order entered, must be in ascending order. Entry {0} had score {1}, lower than the minimum {2}", i, scoreEffects[i], highestScore);
			}
		}

		scoreEffectTarget = 0;
	}

	/// <summary>
	/// Set up the conditions in which conditions or feedback would be triggered. a function is created for each condition to evaluate the data and will return the number of true conditions
	/// </summary>
	void InitializeTrialConditions()
	{
		// difficulty conditions
		baseTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{
//#if UNITY_EDITOR
//			if (false)
//#else
			if (trialData.numBounces >= targetConditionBounces[DifficultyEvaluation.BASE])
// #endif
			{
				return trialData.numBounces / targetConditionBounces[DifficultyEvaluation.BASE];
			}
			return 0; 
		});
		moderateTrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{ 
			if (trialData.numBounces >= targetConditionBounces[DifficultyEvaluation.MODERATE] && (!globalControl.targetHeightEnabled || trialData.numAccurateBounces >= targetConditionAccurateBounces[DifficultyEvaluation.MODERATE])) 
			{
				int bounces = trialData.numBounces / targetConditionBounces[DifficultyEvaluation.MODERATE];
				int accurateBounces = trialData.numAccurateBounces / targetConditionAccurateBounces[DifficultyEvaluation.MODERATE];
				return !globalControl.targetHeightEnabled ? bounces : accurateBounces; 
			} 
			return 0; 
		});
		maximaltrialCondition = new TrialCondition(7, 10, false, feedbackExample, (TrialData trialData) => 
		{
			if (trialData.numBounces >= targetConditionBounces[DifficultyEvaluation.MAXIMAL])
			{
				return trialData.numAccurateBounces / targetConditionBounces[DifficultyEvaluation.MAXIMAL];
			}
			return 0; 
		});

		// feedback conditions
		trialConditions.Add(new TrialCondition(5, 5, true, feedbackExample, (TrialData trialData) => { if (trialData.numBounces < 0) { return 1; } return 0; }));
	}

	/// <summary>
	/// run through all diffiuclties in a short amount of time to get a feel for them
	/// </summary>
	void StartShowcase()
	{
		StartCoroutine(StartDifficultyDelayed(difficultyExampleTime, true));
	}

	IEnumerator StartDifficultyDelayed(float delay, bool initial = false)
	{
		if (initial)
		{
			// wait until after the pause is lifted, when timescale is 0
			yield return new WaitForSeconds(.1f);
		}

		var audioClip = GetDifficiultyAudioClip(difficulty);
		if (audioClip != null)
		{
			difficultySource.PlayOneShot(audioClip);
		}
		Debug.Log("playing difficulty audio " + (audioClip != null ? audioClip.name : "null"));

		yield return new WaitForSecondsRealtime(delay);

		// reset ball, change difficulty level, possible audio announcement.
		if (difficulty >= 10)
		{
			// finish up the difficulty showcase, quit application
			QuitTask();
		}
		else
		{
			SetDifficulty(difficulty + 2);
			if (difficulty == 10)
			{
				yield return new WaitForSecondsRealtime(delay);
			}
			StartCoroutine(StartDifficultyDelayed(difficultyExampleTime));
		}

		ball.GetComponent<Ball>().ResetBall();
	}

	#endregion // Initialization

	#region Reset Trial

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
				// Reset trial
				ResetTrial();
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

	// The ball was reset after hitting the ground. Reset bounce and score.
	public void ResetTrial(bool final = false)
	{
		// Don't run this code the first time the ball is reset or when there are 0 bounces
		//if (trialNum < 1 /*|| numBounces < 1*/)
		//{
		//	trialNum++;
		//	CheckEndCondition();
		//	return;
		//}

		// Record data for final bounce in trial
		GatherBounceData();

		if (globalControl.recordingData)
		{
			// Record Trial Data from last trial
			dataHandler.recordTrial(degreesOfFreedom, Time.time, trialNum, numBounces, numAccurateBounces, difficultyEvaluation, difficulty);
			// CheckDifficulty();
			trialData[difficultyEvaluationIndex].datas.Add(new TrialData(degreesOfFreedom, Time.time, trialNum, numBounces, numAccurateBounces, difficulty));

		}

		if (!final && trialNum != 0 && trialNum % 10 == 0)
		{
			// some difficulty effects are regenerated every 10 trials
			SetDifficulty(difficulty);
		}

		if (numBounces > highestBounces)
		{
			highestBounces = numBounces;
		}
		if (numAccurateBounces > highestAccurateBounces)
		{
			highestAccurateBounces = numAccurateBounces;
		}

		UpdateHighestBounceDisplay();

		trialNum++;
		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;

		if (!final)
		{
			// Check if game should end or evaluation set change
			CheckEndCondition();
		}
	}

	#endregion // Reset

	#region Checks, Interactions, Data

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

		CheckEndCondition(true);
	}

	// Turns ball green briefly and plays success sound.
	public void IndicateSuccessBall()
	{
		Ball b = ball.GetComponent<Ball>();

		ballSoundPlayer.PlaySuccessSound();

		b.TurnBallGreen();
		StartCoroutine(b.TurnBallWhiteCR(0.3f));
	}

	/// <summary>
	/// if the evaluation does not end, chacks all other conditions
	/// </summary>
	void CheckEndCondition(bool fromBounce = false)
	{
		if(session == Session.SHOWCASE)
		{
			return;
		}

 		if (CheckScoreCondition())
		{
			EvaluateDifficultyResult(true);
		}
		else
		{
			CheckTrialConditions(fromBounce);
		}

		if (difficultyEvaluation == DifficultyEvaluation.CUSTOM && globalControl.GetTimeLimitSeconds() == 0)
		{
			return;
		}
	}

	void CheckTrialConditions(bool fromBounce = false)
	{
		foreach(var trialCondition in trialConditions)
		{
			CheckCondition(trialCondition, fromBounce);
		}
	}

	bool CheckScoreCondition()
	{
		TrialCondition difficultyCondition = GetDifficultyCondition(difficultyEvaluation);
   		return CheckCondition(difficultyCondition);
	}

	/// <summary>
	/// evaluate the set of recent bouces in the trial and recent trials. will determine how many true conditions there are/>
	/// </summary>
	/// <param name="trialCondition"></param>
	/// <param name="fromBounce"></param>
	/// <returns></returns>
	bool CheckCondition(TrialCondition trialCondition, bool fromBounce = false)
	{
		var datas = new List<TrialData>(trialData[difficultyEvaluationIndex].datas);

		if (fromBounce)
		{
			// add data from current set in progress
			datas.Add(new TrialData(degreesOfFreedom, Time.time, trialNum, numBounces, numAccurateBounces, difficulty));
		}

		if (trialCondition.trialEvaluationCooldown > 0)
		{
			trialCondition.trialEvaluationCooldown--;
			return false;
		}

		int trueCount = 0;

		// check for true conditions in recent data  
 		for(int i = datas.Count - 1; i >= datas.Count - (trialCondition.trialEvaluationsSet < datas.Count ? trialCondition.trialEvaluationsSet : datas.Count) && i >= 0; i--)
		{
			int trueInTrial = trialCondition.checkTrialCondition(datas[i]);
			if (trueInTrial > 0)
			{
				trueCount += trueInTrial;
			}
			else if (trialCondition.sequential)
			{
				trueCount = 0;
			}

			if (trueCount >= trialCondition.trialEvaluationTarget)
			{
				// successful condition
				if (trialCondition.conditionFeedback != null)
				{	
					feedbackSource.PlayOneShot(trialCondition.conditionFeedback);
				}
				trialCondition.trialEvaluationCooldown = trialCondition.trialEvaluationsSet;
				return true;
			}
		}
		return false;
	}

	void UpdateHighestBounceDisplay()
	{
		string bounces = highestBounces.ToString();
		highestBouncesDisplay.text = String.Format("{0} bounces in a row!", bounces);
	
		if (targetLine.activeInHierarchy)
		{
			string accurateBounces = highestAccurateBounces.ToString();
			highestAccurateBouncesDisplay.text = String.Format("{0} target hits!", accurateBounces);
		}
		else
		{
			highestAccurateBouncesDisplay.text = "";
		}
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

	public TrialCondition GetDifficultyCondition(DifficultyEvaluation evaluation)
	{
		if (evaluation == DifficultyEvaluation.BASE)
		{
			return baseTrialCondition;
		}
		else if (evaluation == DifficultyEvaluation.MODERATE)
		{
			return moderateTrialCondition;
		}
		else if (evaluation == DifficultyEvaluation.MAXIMAL)
		{
			return maximaltrialCondition;
		}

		return null;
	}

	public int GetMaxDifficultyTrialTime(DifficultyEvaluation difficultyEvaluation)
	{
		int trialTime = -1;
		if (difficultyEvaluation == DifficultyEvaluation.BASE)
		{
			trialTime = globalControl.maxBaselineTrialTime;
		}
		else if (difficultyEvaluation == DifficultyEvaluation.MODERATE)
		{
			if(difficultyEvaluationIndex == difficultyEvaluationOrder.IndexOf(DifficultyEvaluation.MODERATE))
			{
				trialTime = globalControl.maxModerate1TrialTime;
			}
			else if (difficultyEvaluationIndex == difficultyEvaluationOrder.LastIndexOf(DifficultyEvaluation.MODERATE))
			{
				trialTime = globalControl.maxModerate2TrialTime;
			}
		}
		else if (difficultyEvaluation == DifficultyEvaluation.MAXIMAL)
		{
			trialTime = globalControl.maxMaximalTrialTime;
		}
		else if (difficultyEvaluation == DifficultyEvaluation.CUSTOM)
		{
			trialTime = globalControl.maxTrialTime;
		}

		return trialTime != -1 ? trialTime * 60 : trialTime;
	}

	private AudioClip GetDifficiultyAudioClip(int difficulty)
	{
		foreach(var difficultyAudioClip in difficultyAudioClips)
		{
			if(difficultyAudioClip.difficulty == difficulty)
			{
				return difficultyAudioClip.audioClip;
			}
		}
		return null;
	}

	#region Gathering and recording data

	public void StartRecording()
	{
		// Record session data
		dataHandler.recordHeaderInfo(condition, expCondition, session, maxTrialTime, hoverTime, targetRadius);
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

	// Determine data for recording a bounce and finally, record it.
	private void GatherBounceData()
	{
		float apexHeight = Mathf.Max(bounceHeightList.ToArray());
		float apexTargetError = globalControl.targetHeightEnabled ? (apexHeight - targetLine.transform.position.y) : 0;

		bool apexSuccess = globalControl.targetHeightEnabled ? GetHeightInsideTargetWindow(apexHeight) : true;

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

	#endregion // Gathering and recording data

	#endregion // Checks, Interactions, Data

	#region Difficulty

	private int GetDifficulty(DifficultyEvaluation evaluation)
	{
		if (evaluation == DifficultyEvaluation.CUSTOM)
		{
			return globalControl.difficulty;
		}
		else if (evaluation == DifficultyEvaluation.BASE)
		{
#if UNITY_EDITOR
			// return 10;
#endif
			return 1;
		}
		else
		{
			return performanceDifficulties[difficultyEvaluationIndex];
		}
	}

	/// <summary>
	/// evaluate the data from this evaluation, and set the difficulty and perpare next evaluation
	/// </summary>
	/// <param name="successfulCompletion"></param>
	void EvaluateDifficultyResult(bool successfulCompletion)
	{
		if(difficultyEvaluationIndex + 1 == difficultyEvaluationOrder.Count)
		{
			Debug.Log("Last evaluation start");
		}

		var difficultyScalar = GetPerformanceBasedDifficultyScalar(successfulCompletion);
		int difficulty = 0;

		if(session == Session.ACQUISITION || session == Session.SHOWCASE)
		{
			// over once end time is reached.
			QuitTask();
			return;
		}

		// each are evaluating for the next difficultyEvaluation
		if (difficultyEvaluation == DifficultyEvaluation.BASE)
		{
			difficulty = Mathf.RoundToInt(Mathf.Lerp(2, 5, difficultyScalar));
		}
		else if (difficultyEvaluation == DifficultyEvaluation.MODERATE)
		{
			difficulty = Mathf.RoundToInt(Mathf.Lerp(6, 10, difficultyScalar));
		}
		else if (difficultyEvaluation == DifficultyEvaluation.MAXIMAL)
		{
			difficulty = Mathf.RoundToInt(Mathf.Lerp(2, 5, difficultyScalar));
		}

		if (difficulty < 0 || difficulty > 10)
		{
			// invalid, get more data
			return;
		}

		performanceDifficulties.Add(difficulty);

		// reset cooldown, condition may be used again
		GetDifficultyCondition(difficultyEvaluation).trialEvaluationCooldown = 0;

		if (difficultyEvaluationIndex + 1 == difficultyEvaluationOrder.Count)
		{
			QuitTask();
			return;
		}

		difficultyEvaluationIndex++;
		difficultyEvaluation = difficultyEvaluationOrder[difficultyEvaluationIndex];
		difficultyEvaluationTrials = GetDifficultyCondition(difficultyEvaluationOrder[difficultyEvaluationIndex]).trialEvaluationTarget;

		SetDifficulty(difficulty);

		Initialize();

		// await input for next difficulty evaluation
		pauseHandler.Pause();
	}

	/// <summary>
	/// evaluates performance compared to trial condition
	/// </summary>
	/// <param name="successfulCompletion"></param>
	/// <returns></returns>
	private float GetPerformanceBasedDifficultyScalar(bool successfulCompletion)
	{
		if (difficultyEvaluationIndex >= 0 && difficultyEvaluationIndex < difficultyEvaluationOrder.Count)
		{
			var datas = trialData[difficultyEvaluationIndex].datas;

			// evaluating time percentage of the way to end, 10 min
			var timeScalar = 1 - (globalControl.GetTimeElapsed() / GetMaxDifficultyTrialTime(difficultyEvaluation));

			// evaluating performance, average bounces and accurate bounces. 
			int bounces = 0, accurateBounces = 0;
			float averageBounces, averageAccurateBounces;


			foreach(var trial in datas)
			{
				bounces += trial.numBounces;
				accurateBounces += trial.numAccurateBounces;
			}

			averageBounces = (float)bounces / (float)datas.Count;
			averageAccurateBounces = (float)accurateBounces / (float)datas.Count;

			// average bounces
			float averageBounceScalar = Mathf.Clamp(averageBounces / (float)targetConditionBounces[difficultyEvaluation], 0f, 1.3f);
			
			// accurate bounces
			float averageAccurateBounceScalar = globalControl.targetHeightEnabled ? Mathf.Clamp(averageAccurateBounces / targetConditionAccurateBounces[difficultyEvaluation], 0f, 1.3f) : 0;

			return Mathf.Clamp01((averageBounceScalar + averageAccurateBounceScalar + timeScalar) / (globalControl.targetHeightEnabled ? 3f : 2f));
		}

		return -1;
	}

	public void SetDifficulty(int difficultyNew)
	{
		if (difficultyNew < 0 || difficultyNew > 10)
		{
			Debug.LogError("Issue setting difficulty, not in expected range: " + difficultyNew);
			
			return;
		}
		else
		{
			Debug.Log("Setting Difficulty: " + difficultyNew);
		}

		numBounces = 0;
		numAccurateBounces = 0;
		curScore = 0f;
		scoreEffectTarget = 0;
		maxScoreEffectReached = false;

		difficulty = difficultyNew;

		float ballSpeedNew = GetBallSpeedDifficulty(difficulty); // Mathf.Lerp(ballSpeedMin, ballSpeedMax, difficultyScalar);

		// removed bounce height change for now
		// int ballBounceEnd = UnityEngine.Random.Range(0, 2) == 0 ? ballBounceMin : ballBounceMax;
		// int bouncinessNew = (int)Math.Round(Mathf.Lerp(ballBounceMid, ballBounceEnd, difficultyScalar), 0);

		bool targetLineHeightEnabled = GetTargetLineActiveDifficulty(difficulty);
		globalControl.targetHeightEnabled = targetLineHeightEnabled;

		if (globalControl.targetHeightEnabled)
		{
			targetLine.SetActive(true);
		}
		else
		{
			targetLine.SetActive(false);
		}

		float targetLineHeightOffset = GetTargetLineHeightOffsetDifficulty(performanceDifficulties[difficultyEvaluationIndex]);
		globalControl.targetLineHeightOffset = targetLineHeightOffset;
		SetTargetLineHeight(targetLineHeightOffset);


		float targetRadiusNew = globalControl.targetHeightEnabled ? GetTargetLineWidthDifficulty(difficulty) / 2f : 0; // Mathf.Lerp(targetRadiusMin, targetRadiusMax, difficultyScalar);

		globalControl.timescale = ballSpeedNew;
		Time.timeScale = ballSpeedNew;

		globalControl.targetRadius = targetRadiusNew;
		targetRadius = targetRadiusNew;
		targetLine.transform.localScale = new Vector3(targetLine.transform.localScale.x, targetRadius * 2f, targetLine.transform.localScale.z);

		difficultyDisplay.text = difficulty.ToString();

		// record difficulty values change
		if (globalControl.recordingData)
		{
			dataHandler.recordDifficulty(ballSpeedNew, targetLineHeightEnabled, targetLineHeightOffset, targetRadiusNew, Time.time, difficulty);
		}

		if (performanceDifficulties.Count > difficultyEvaluationOrder.Count)
		{
			// all difficulties recored
			QuitTask();
		}
	}

	private float GetBallSpeedDifficulty(int difficulty)
	{
		switch (difficulty)
		{
#if UNITY_EDITOR
			// testing at .3 too time consuming
			case 1: return .3f;
#else
			case 1: return .3f;
#endif
			case 2: return .4f;
			case 3: return .45f;
			case 4: return .5f;
			case 5: return .55f;
			case 6: return .6f;
			case 7: return .7f;
			case 8: return .9f;
			case 9: return UnityEngine.Random.Range(.95f, .95f + .1f);
			case 10: return UnityEngine.Random.Range(.95f, .95f + .15f);
			default: return 1f;
		}
	}

	private bool GetTargetLineActiveDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return false;
			case 2: return false;
			case 3: return false;
			case 4: return false;
			case 5: return true;
			case 6: return true;
			case 7: return true;
			case 8: return true;
			case 9: return true;
			case 10: return true;
			default: return false;
		}
	}

	private float GetTargetLineHeightOffsetDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return 0f;
			case 2: return 0f;
			case 3: return 0f;
			case 4: return 0f;
			case 5: return 0f;
			case 6: return 0f;
			case 7: return 0f;
			case 8: return 0f;
			case 9: return UnityEngine.Random.Range(-.02f, .02f);
			case 10: return UnityEngine.Random.Range(-.02f, -.02f);
			default: return 0f;
		}
	}

	private float GetTargetLineWidthDifficulty(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return 0f;
			case 2: return 0f;
			case 3: return 0f;
			case 4: return 0f;
			case 5: return .04f;
			case 6: return .04f;
			case 7: return .0375f;
			case 8: return .035f;
			case 9: return .0325f;
			case 10: return .03f;
			default: return .04f;
		}
	}

#endregion // Difficulty

#region Exploration Mode

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

#endregion // Exploration Mode

}
