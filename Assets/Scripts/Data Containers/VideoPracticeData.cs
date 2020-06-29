using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using JetBrains.Annotations;

public class VideoPracticeData
{
	public float playbackDuration;
	public float practiceDuration;
	public UnityAction practiceChanges;

	public VideoPracticeData(float playbackDurationVar, float practiceDurationVar, UnityAction practiceChangesVar)
	{
		playbackDuration = playbackDurationVar;
		practiceDuration = practiceDurationVar;
		practiceChanges = practiceChangesVar;
	}
}
