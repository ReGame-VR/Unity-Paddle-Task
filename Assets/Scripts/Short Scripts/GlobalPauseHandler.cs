using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPauseHandler : MonoBehaviour
{
    // Handle pausing
    void Update()
    {
        // Handle Paused and Playing states separately.
        if (GlobalControl.Instance.paused)
        {
            // Show pause indicator
            GameObject.Find("Pause Indicator").GetComponent<PauseIndicator>().quad.SetActive(true);

            ListenForResume();
        }
        else 
        {
            // Hide pause indicator
            GameObject.Find("Pause Indicator").GetComponent<PauseIndicator>().quad.SetActive(false);

            ListenForPause();
        }
    }

    void ListenForResume()
    {
        if (Input.GetKeyDown("space"))
        {
            GlobalControl.Instance.paused = false;

            // Trigger Ball physics resume listener
            GameObject.Find("Ball").GetComponent<Kinematics>().TriggerResume();
        }
    }

    void ListenForPause()
    {
        if (Input.GetKeyDown("space"))
        {
            GlobalControl.Instance.paused = true;

            // Trigger Ball physics pause listener
            GameObject.Find("Ball").GetComponent<Kinematics>().TriggerPause();
        }
    }


}

