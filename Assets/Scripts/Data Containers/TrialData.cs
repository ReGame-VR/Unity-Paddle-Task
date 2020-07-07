/// <summary>
/// A class that stores info on each trial relevant to data recording. Every field is
/// public readonly, so can always be accessed, but can only be assigned once in the
/// constructor.
/// </summary>
public class TrialData
{
    public readonly float degreesOfFreedom;
    public readonly float time;
    public readonly int trialNum;
    public readonly int numBounces;
    public readonly int numAccurateBounces;
    public readonly int difficulty;

    public TrialData(float degreesOfFreedom, float time, int trialNum, int numBounces, int numAccurateBounces, int difficulty)
    {
        this.degreesOfFreedom = degreesOfFreedom;
        this.time = time;
        this.trialNum = trialNum;
        this.numBounces = numBounces;
        this.numAccurateBounces = numAccurateBounces;
        this.difficulty = difficulty;
    }
}