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

    public void Pause()
    {
        GlobalControl.Instance.paused = true;
        Time.timeScale = 0;

        // Trigger Ball physics pause listener
        GameObject.Find("Ball").GetComponent<Kinematics>().TriggerPause();
    }

    public void Resume()
    {
        GlobalControl.Instance.paused = false;
        Time.timeScale = GlobalControl.Instance.timescale;
        // Trigger Ball physics resume listener
        GameObject.Find("Ball").GetComponent<Kinematics>().TriggerResume();
    }
}

