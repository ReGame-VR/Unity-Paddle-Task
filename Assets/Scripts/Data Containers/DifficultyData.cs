using UnityEngine;
using System.Collections;

public class DifficultyData
{
	public readonly float ballSpeed;
	public readonly bool targetLineActive;
	public readonly float targetLineHeightOffset;
	public readonly float targetLineWidth;
	public readonly float time;
	public readonly int difficulty;

	public DifficultyData(float ballSpeed, bool targetLineActive, float targetLineHeightOffset, float targetLineWidth, float time, int difficulty)
	{
		this.ballSpeed = ballSpeed;
		this.targetLineActive = targetLineActive;
		this.targetLineHeightOffset = targetLineHeightOffset;
		this.targetLineWidth = targetLineWidth;
		this.time = time;
		this.difficulty = difficulty;
	}

}
