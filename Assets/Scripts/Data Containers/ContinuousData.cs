using UnityEngine;
using System.Collections;

public class ContinuousData
{
    public readonly float degreesOfFreedom;
    public readonly float time;
    public readonly Vector3 bounceModification;
    public readonly bool paused;
    public readonly Vector3 ballPos;
    public readonly Vector3 paddleVelocity;
    public readonly Vector3 paddleAccel;

    public ContinuousData(float degreesOfFreedom, float time, Vector3 bouncemod, bool paused, Vector3 ballPos, Vector3 paddleVelocity, Vector3 paddleAccel)
    {
        this.degreesOfFreedom = degreesOfFreedom;
        this.time = time;
        this.bounceModification = bouncemod;
        this.paused = paused;
        this.ballPos = ballPos;
        this.paddleVelocity = paddleVelocity;
        this.paddleAccel = paddleAccel;
    }
}
