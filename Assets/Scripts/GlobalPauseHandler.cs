using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPauseHandler : MonoBehaviour
{
    public PauseIndicator pauseIndicator;

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
        Debug.Log("pause");
    }

    public void Resume()
    {
        GlobalControl.Instance.paused = false;
        Time.timeScale = GlobalControl.Instance.timescale;
        // Trigger Ball physics resume listener
        GameObject.Find("Ball").GetComponent<Kinematics>().TriggerResume();
        Debug.Log("Resume");
    }

    public void SetIndicatorVisibility(bool visible)
	{
		if (visible)
		{
            pauseIndicator.visibleOverride = true;
            pauseIndicator.visibleOverrideValue = true;
		}
		else
		{
            pauseIndicator.visibleOverride = true;
            pauseIndicator.visibleOverrideValue = false;
		}
	}
}

