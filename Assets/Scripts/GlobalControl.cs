using System.Collections.Generic;
using UnityEngine;


public enum Condition { REGULAR, ENHANCED, REDUCED, TARGETLINE };
public enum Session { BASELINE, ACQUISITION, RETENTION, TRANSFER, SHOWCASE };
public enum TargetHeight { DEFAULT, LOWERED, RAISED };
public enum ExpCondition { RANDOM = 0, HEAVIEST = 1, HEAVIER = 2, NORMAL = 3, LIGHTER = 4, LIGHTEST = 5 };
public enum DifficultyEvaluation { BASE, MODERATE, MAXIMAL, CUSTOM };
public enum Mindset { GROWTH, CONTROL };

/// <summary>
/// Stores calibration data for trial use in a single place.
/// </summary>
public class GlobalControl : MonoBehaviour 
{

    // The single instance of this class
    public static GlobalControl Instance;

    public enum ExplorationMode { NONE, TASK, FORCED };

    public ExplorationMode explorationMode = ExplorationMode.NONE;

    // participant ID to differentiate data files
    public string participantID = "";

    // The number of paddles that the player is using. Usually 1 or 2.
    public int numPaddles = 1;

    // The condition of this instance
    public Condition condition = Condition.ENHANCED;

    // The Exploration condition of this instance (controls randomized physics)
    public ExpCondition expCondition = ExpCondition.NORMAL;

    // Target Line Height
    public TargetHeight targetHeightPreference = TargetHeight.DEFAULT;

    // Target Line Success Threshold
    public float targetRadius = 0.05f;

    // Test period of this instance
    public Session session = Session.BASELINE;

    // Degrees of Freedom for ball bounce for this instance
    public float degreesOfFreedom = 90;

    // Time limit in minutes after beginning, after which the game will end
    public int maxTrialTime = 0;

    // Time elapsed while game is not paused, in seconds 
    public float timeElapsed = 0;

    // Duration for which ball should be held before dropping upon reset
    public int ballResetHoverSeconds = 3;

    // Allow game to be paused
    public bool paused = true;

    // Alter the speed at which physics and other updates occur
    public float timescale = 1f;

    // Will hide the target height and alter behaviors so they are affected by consecutive hits only
    public bool targetHeightEnabled = true;

    // value affecting various metrics increasing randomness and general difficulty
    public int difficulty = 1;

    // Play video at the start
    public bool playVideo = false;

    // Selected enviornment
    public int environmentOption = 0;

    public List<GameObject> environments = new List<GameObject>();

    public bool recordingData = true;

    // How many trials should be used for difficulty evaluation
    public int difficultyEvaluationTrials = 10;

    // the change in difficulty when shifting
    // public int difficultyInterval = 1f;

    // low and high ends for difficulty scale
    public int difficultyMin = 1, difficultyMax = 10;

    public DifficultyEvaluation difficultyEvaluation = DifficultyEvaluation.BASE;

    [System.NonSerialized]
    public float targetLineHeightOffset = 0;

    /// <summary>
    /// Assign instance to this, or destroy it if Instance already exits and is not this instance.
    /// </summary>
    void Awake()
    {
        if (condition == Condition.ENHANCED)
        {
            explorationMode = ExplorationMode.FORCED;
        }

        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!paused)
        {
            timeElapsed += Time.deltaTime;
        }
    }
    
    public float GetTimeLimitSeconds()
    {
        return maxTrialTime * 60.0f;
    }

    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    public void ResetTimeElapsed()
	{
        timeElapsed = 0;
	}
}
