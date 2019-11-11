using UnityEngine;


public enum Condition { REGULAR, ENHANCED, REDUCED, TARGETLINE };
public enum Session { BASELINE, ACQUISITION, RETENTION, TRANSFER };
public enum TargetHeight { DEFAULT, LOWERED, RAISED };
public enum ExpCondition { RANDOM, HEAVIEST, HEAVIER, NORMAL, LIGHTER, LIGHTEST };

/// <summary>
/// Stores calibration data for trial use in a single place.
/// </summary>
public class GlobalControl : MonoBehaviour {

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

    // The single instance of this class
    public static GlobalControl Instance;

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
}
