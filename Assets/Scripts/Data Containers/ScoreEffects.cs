using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreEffects
{
	public int score;
	public Effect effect;
	public AudioClip audioClip;
	public List<Effect> disableEffects;

	public ScoreEffects(int scoreVar, Effect effectVar, AudioClip audioClipVar, List<Effect> disableEffectsVar = null)
	{
		score = scoreVar;
		effect = effectVar;
		audioClip = audioClipVar;
		disableEffects = disableEffectsVar != null ? disableEffectsVar : new List<Effect>();
	}
}
