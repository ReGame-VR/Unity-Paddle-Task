using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeData : MonoBehaviour
{
    public readonly float time;
    public readonly bool paused;
    public readonly Vector3 gaze;
    public readonly Vector3 origin;
    public readonly Vector3 direction;
    public readonly int gazedObj;

    public EyeData(float time, bool paused, Vector3 gaze, Vector3 origin, Vector3 direction, int gazedObj)
    {
        this.time = time;
        this.paused = paused;
        this.gaze = gaze;
        this.origin = origin;
        this.direction = direction;
        this.gazedObj = gazedObj;
    }
}
