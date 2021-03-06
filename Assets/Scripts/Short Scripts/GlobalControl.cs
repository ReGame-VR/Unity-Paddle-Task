﻿using UnityEngine;

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

    // The single instance of this class
    public static GlobalControl Instance;

    /// <summary>
    /// Assign instance to this, or destroy it if Instance already exits and is not this instance.
    /// </summary>
    void Awake()
    {
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
}
