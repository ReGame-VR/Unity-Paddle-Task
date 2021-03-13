using Tobii.XR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    public Vector3 origin;
    public Vector3 direction;
    public Vector3 gaze;
    public int gazedObj = 0;

    GlobalControl globalControl;
    DataHandler dataHandler;
    public GameObject masterControl;

    void Start()
    {
        globalControl = GlobalControl.Instance;
        dataHandler = masterControl.GetComponent<DataHandler>();
    }

    void Update()
    {
        var recordEye = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        var gazeRay = recordEye.GazeRay;
        origin = gazeRay.Origin;
        direction = gazeRay.Direction;

        RaycastHit hit;
        Debug.DrawRay(gazeRay.Origin, gazeRay.Direction * 1000, Color.green);
        if (Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out hit))
        {
            gaze = hit.point;

            // report the object that is being gazed
            // 1 - ball; 2 - paddle; 3 - target line; 4 - difficulty level text; 5 - podium; 6 - info panel; 7 - effects; 0 - anything else 
            if (hit.collider.gameObject.tag == "Ball")
            {
                gazedObj = 1;
            }
            if (hit.collider.gameObject.tag == "Paddle")
            {
                gazedObj = 2;
            }
            if (hit.collider.gameObject.tag == "Line")
            {
                gazedObj = 3;
            }
            if (hit.collider.gameObject.tag == "LevelText")
            {
                gazedObj = 4;
            }
            if (hit.collider.gameObject.tag == "Podium")
            {
                gazedObj = 5;
            }
            if (hit.collider.gameObject.tag == "InfoPanel")
            {
                gazedObj = 6;
            }
            if (hit.collider.gameObject.tag == "Effect")
            {
                gazedObj = 7;
            }
        }

        if (globalControl.recordingData)
        {
            GatherEyeData();
        }

    }
    /*
    void Initialize()
    {
        if (globalControl.recordingData)
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        // Record session data
        dataHandler.recordHeaderInfo(condition, expCondition, session, maxTrialTime, hoverTime, targetRadius);
    }
    */

    private void GatherEyeData()
    {
        if (globalControl.recordingData)
        {
            dataHandler.recordEye(Time.time, globalControl.paused, gaze, origin, direction, gazedObj);
        }

    }
}
