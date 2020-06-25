using UnityEngine;
using System.Collections;

public class ScoreEffects
{
	public int score;
	public Effect effect;
	public AudioClip audioClip;

	public ScoreEffects(int scoreVar, Effect effectVar, AudioClip audioClipVar)
	{
		score = scoreVar;
		effect = effectVar;
		audioClip = audioClipVar;
	}
}
