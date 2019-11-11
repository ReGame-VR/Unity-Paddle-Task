using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPauseHandler : MonoBehaviour
{
    public void TogglePause()
    {
        if (GlobalControl.Instance.paused == true)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        GlobalControl.Instance.paused = true;

        // Trigger Ball physics pause listener
        GameObject.Find("Ball").GetComponent<Kinematics>().TriggerPause();
    }

    private void Resume()
    {
        GlobalControl.Instance.paused = false;

        // Trigger Ball physics resume listener
        GameObject.Find("Ball").GetComponent<Kinematics>().TriggerResume();
    }
}

