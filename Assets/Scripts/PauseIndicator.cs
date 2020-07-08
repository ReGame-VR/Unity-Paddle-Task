using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseIndicator : MonoBehaviour
{
    public GameObject quad;

    void Update()
    {
        if (GlobalControl.Instance.paused/* && !GlobalControl.Instance.recordingData*/)
        {
            quad.SetActive(true);
        }
        else
        {
            quad.SetActive(false);
        }
    }
}
