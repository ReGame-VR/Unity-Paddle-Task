using UnityEngine;
using System.Collections;

[System.Serializable]
public class EffectParticle
{
	public Effect effect;
	public GameObject particleParent;

	public EffectParticle(Effect effectVar, GameObject particleParentVar)
	{
		effect = effectVar;
		particleParent = particleParentVar;
	}
}
