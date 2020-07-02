using UnityEngine;
using System.Collections;

public class TrialCondition
{
	public int trialEvaluationsSet;
	public int trialEvaluationTarget;
	public bool sequential = false;
	public AudioClip conditionFeedback;
	public int trialEvaluationCooldown = 0;
	
	public delegate bool CheckTrial(TrialData trialData);
	public CheckTrial checkTrialCondition;

	public TrialCondition(int trialEvaluationTargetVar, int trialEvaluationSetVar, bool sequentialVar, AudioClip conditionFeedbackVar, CheckTrial checkTrialConditionVar)
	{
		trialEvaluationTarget = trialEvaluationTargetVar;
		trialEvaluationsSet = trialEvaluationSetVar;
		sequential = sequentialVar;
		conditionFeedback = conditionFeedbackVar;
		checkTrialCondition = checkTrialConditionVar;
	}
}
