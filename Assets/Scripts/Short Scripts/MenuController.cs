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

    // Records which exploration mode the user chose
    public void RecordExplorationMode(int arg0)
    {
        if (arg0 == 0)
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.NONE;
        }
        else if (arg0 == 1)
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.TASK;
        }
        else
        {
            GlobalControl.Instance.explorationMode = GlobalControl.ExplorationMode.FORCED;
        }
    }

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
            // two paddles???
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
