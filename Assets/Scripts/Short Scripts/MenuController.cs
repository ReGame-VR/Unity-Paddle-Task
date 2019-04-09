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
    }

    /// <summary>
    /// Records an int representing max number of trials allowed for this instance.
    /// </summary>
    /// <param name="arg0"></param>
    public void RecordMaxTrials(string arg0)
    {
        GlobalControl.Instance.maxTrialCount = ((arg0 == "") ? 0 : int.Parse(arg0));
    }

    // Record how many seconds the ball should hover for upon reset
    public void UpdateHoverTime(float sValue)
    {
        Slider s = GameObject.Find("Ball Respawn Time Slider").GetComponent<Slider>();
        Text sliderText = GameObject.Find("Time Indicator").GetComponent<Text>();

        sliderText.text = (sValue + " seconds");
        GlobalControl.Instance.ballResetHoverSeconds = (int)sValue;
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
    }

    // Records the number of paddles from the dropdown nmenu
    public void RecordNumPaddles(int choice)
    {
        if (choice == 0)
        {
            GlobalControl.Instance.numPaddles = 1;
        }
        else
        {
            GlobalControl.Instance.numPaddles = 2;
        }
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
