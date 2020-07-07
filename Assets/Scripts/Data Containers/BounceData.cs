using UnityEngine;
using System.Collections;

public class BounceData
{
    public readonly float degreesOfFreedom;
    public readonly float time;
    public readonly Vector3 bounceModification;
    public readonly int trialNum;
    public readonly int bounceNum;
    public readonly int bounceNumTotal;
    public readonly float apexTargetError;
    public readonly bool success;
    public readonly Vector3 paddleVelocity;
    public readonly Vector3 paddleAccel;

    public BounceData(float degreesOfFreedom, float time, Vector3 bouncemod, int trialNum, int bounceNum, int bounceNumTotal, float apexTargetError, bool success, Vector3 paddleVelocity, Vector3 paddleAccel)
    {
        this.degreesOfFreedom = degreesOfFreedom;
        this.time = time;
        this.bounceModification = bouncemod;
        this.trialNum = trialNum;
        this.bounceNum = bounceNum;
        this.bounceNumTotal = bounceNumTotal;
        this.apexTargetError = apexTargetError;
        this.success = success;
        this.paddleVelocity = paddleVelocity;
        this.paddleAccel = paddleAccel;
    }

}

