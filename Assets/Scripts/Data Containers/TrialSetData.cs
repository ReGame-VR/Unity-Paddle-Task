public class TrialSetData
{
	public float bounces, averageBounces;
	public float accurateBounces, averageAccurateBounces;
	public float difficultySlope;

	public TrialSetData(float bouncesVar, float averageBouncesVar, float accurateBouncesVar, float averageAccurateBouncesVar, float difficultySlopeVar)
	{
		bounces = bouncesVar;
		averageBounces = averageBouncesVar;
		accurateBounces = accurateBouncesVar;
		averageAccurateBounces = averageAccurateBouncesVar;
		difficultySlope = difficultySlopeVar;
	}
}
