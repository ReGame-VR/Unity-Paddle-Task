using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseIndicator : MonoBehaviour
{
    public GameObject quad;
    public bool visibleOverride, visibleOverrideValue;


    void Update()
    {

		if (visibleOverride)
		{
            quad.SetActive(visibleOverrideValue);
		}
        else if (GlobalControl.Instance.paused/* && !GlobalControl.Instance.recordingData*/)
        {
            quad.SetActive(true);
        }
        else
        {
            quad.SetActive(false);
        }
    }
}
