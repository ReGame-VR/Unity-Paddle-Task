using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds functions for responding to and recording preferences on menu.
/// </summary>
public class MenuController : MonoBehaviour {

    /// <summary>
    /// Disable VR for menu scene and hide warning text until needed.
    /// </summary>
    void Start()
    {
        // disable VR settings for menu scene
        UnityEngine.XR.XRSettings.enabled = false;
        GlobalControl.Instance.numPaddles = 1;
        GlobalControl.Instance.participantID = "";
        GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.NONE;

        // Load saved preferences
        GetComponent<MenuPlayerPrefs>().LoadAllPreferences();
    }

    /// <summary>
    /// Records an alphanumeric participant ID. Hit enter to record. May be entered multiple times
    /// but only last submission is used. Called using a dynamic function in the inspector
    /// of the textfield object.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordID(string arg0)
    {
        GlobalControl.Instance.participantID = arg0;
    }


    /// <summary>
    /// Records an float representing degrees of freedom in the xz plane.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordDegrees()
    {
        GlobalControl.Instance.degreesOfFreedom = (GlobalControl.Instance.condition == Condition.REDUCED) ? 0.0f : 90.0f;
        // GetComponent<MenuPlayerPrefs>().SaveDOF(GlobalControl.Instance.degreesOfFreedom);
    }

    /// <summary>
    /// Records an int representing max number of trials allowed for this instance.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordMaxTrials(int arg0)
    {
        Dropdown d = GameObject.Find("Max Trial Time Dropdown").GetComponent<Dropdown>();
        GlobalControl.Instance.maxTrialTime = arg0;
        d.value = arg0;
        GetComponent<MenuPlayerPrefs>().SaveMaxTrials(arg0);
    }

    // Record how many seconds the ball should hover for upon reset
    public void UpdateHoverTime(float value)
    {
        Slider s = GameObject.Find("Ball Respawn Time Slider").GetComponent<Slider>();
        Text sliderText = GameObject.Find("Time Indicator").GetComponent<Text>();

        sliderText.text = value + " seconds";
        s.value = value;
        GlobalControl.Instance.ballResetHoverSeconds = (int)value;

        GetComponent<MenuPlayerPrefs>().SaveHoverTime(value);
    }

    // Set the window for how far the ball can be from the target line and still count as a success
    public void UpdateTargetRadius(float value)
    {
        const float INCHES_PER_METER = 39.37f;
        const float METERS_PER_INCH = 0.0254f;

        Slider s = GameObject.Find("Success Threshold Slider").GetComponent<Slider>(); 
        Text sliderText = GameObject.Find("Width Indicator").GetComponent<Text>();

        float targetThresholdInches = value * 0.5f;
        float targetThresholdMeters = targetThresholdInches * METERS_PER_INCH; // each notch is 0.5 inches 
        sliderText.text = "+/- " + targetThresholdInches.ToString("0.0") + " in.\n(" + value.ToString("0.0") + " in. total)";
        s.value = value;
        GlobalControl.Instance.targetRadius = targetThresholdMeters;

        GetComponent<MenuPlayerPrefs>().SaveTargetRadius(value);
    }

    // Records the Condition from the dropdown menu
    public void RecordCondition(int arg0)
    {
        if (arg0 == 0)
        {
            GlobalControl.Instance.condition = Condition.REGULAR;
        }
        else if (arg0 == 1)
        {
            GlobalControl.Instance.condition = Condition.ENHANCED;
        }
        else if (arg0 == 2)
        {
            GlobalControl.Instance.condition = Condition.REDUCED;
        }
        else if (arg0 == 3)
        {
            GlobalControl.Instance.condition = Condition.TARGETLINE;
        }

        // GetComponent<MenuPlayerPrefs>().SaveCondition(arg0);
    }

    // Records the functional Exploration mode, tied to Condition dropdown menu
    public void RecordExplorationMode(int arg0)
    {
        if (arg0 == 1)
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.FORCED;
        }
        else
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.NONE;
        }

        // GetComponent<MenuPlayerPrefs>().SaveExplorationMode(arg0);
    }

    // Records the Session from the dropdown menu
    public void RecordSession(int arg0)
    {
        if (arg0 == 0)
        {
            GlobalControl.Instance.session = Session.BASELINE;
        }
        if (arg0 == 1)
        {
            GlobalControl.Instance.session = Session.ACQUISITION;
        }
        if (arg0 == 2)
        {
            GlobalControl.Instance.session = Session.RETENTION;
        }
        if (arg0 == 3)
        {
            GlobalControl.Instance.session = Session.TRANSFER;
        }

        // GetComponent<MenuPlayerPrefs>().SaveSession(arg0);
    }

    // Records the Target Line height preference from the dropdown menu
    public void RecordTargetHeight(int arg0)
    {
        if (arg0 == 0)
        {
            GlobalControl.Instance.targetHeightPreference = TargetHeight.DEFAULT;
        }
        if (arg0 == 1)
        {
            GlobalControl.Instance.targetHeightPreference = TargetHeight.LOWERED;
        }
        if (arg0 == 2)
        {
            GlobalControl.Instance.targetHeightPreference = TargetHeight.RAISED;
        }

        //GetComponent<MenuPlayerPrefs>().SaveTargetHeight(arg0);
    }

    // Records the number of paddles from the dropdown nmenu
    public void RecordNumPaddles(int arg0)
    {
        if (arg0 == 0)
        {
            GlobalControl.Instance.numPaddles = 1;
        }
        else
        {
            GlobalControl.Instance.numPaddles = 2;
        }

        // GetComponent<MenuPlayerPrefs>().SaveNumPaddles(arg0);
    }

    /// <summary>
    /// Loads next scene if wii is connected and participant ID was entered.
    /// </summary>
    public void NextScene()
    {
        if (GlobalControl.Instance.numPaddles == 1)
        {
            SceneManager.LoadScene("Paddle");
        }
        else
        {
            SceneManager.LoadScene("Paddle 2");
        }  
    }

    /// <summary>
    /// Re-enable VR when this script is disabled (since it is disabled on moving into next scene).
    /// </summary>
    void OnDisable()
    {
        UnityEngine.XR.XRSettings.enabled = true;
    }
}
