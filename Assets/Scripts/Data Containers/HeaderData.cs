using UnityEngine;
using System.Collections;

public class HeaderData
{
    public readonly Condition condition;
    public readonly ExpCondition expCondition;
    public readonly Session session;
    public readonly int maxTrialTimeMin;
    public readonly float hoverTime;
    public readonly float targetRadius;

    public HeaderData(Condition c, ExpCondition ec, Session s, int maxtime, float htime, float tradius)
    {
        this.condition = c;
        this.expCondition = ec;
        this.session = s;
        this.maxTrialTimeMin = maxtime;
        this.hoverTime = htime;
        this.targetRadius = tradius;
    }
}