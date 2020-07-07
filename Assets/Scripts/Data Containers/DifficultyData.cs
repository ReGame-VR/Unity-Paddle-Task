using UnityEngine;
using System.Collections;

public class DifficultyData : MonoBehaviour
{
	public readonly float ballSpeed;
	public readonly bool targetLineActive;
	public readonly float targetLineHeightOffset;
	public readonly float targetLineWidth;
	public readonly float time;

	public DifficultyData(float ballSpeed, bool targetLineActive, float targetLineHeightOffset, float targetLineWidth, float time)
	{
		this.ballSpeed = ballSpeed;
		this.targetLineActive = targetLineActive;
		this.targetLineHeightOffset = targetLineHeightOffset;
		this.targetLineWidth = targetLineWidth;
		this.time = time;
	}

}
