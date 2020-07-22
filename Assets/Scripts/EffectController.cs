using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Experimental.AI;

/// <summary>
/// facilitate management of effects. handles setting shader and particla effects. 
/// </summary>
public class EffectController : MonoBehaviour
{
	public Effect dissolve, respawn, /*sparks,*/ fire, blueFire, embers, blueEmbers;

	public Effect effectTarget;
	Effect activeShaderEffect;
	new Rigidbody rigidbody;

	float respawnTimer = 5f;


	void Start()
	{
		dissolve?.gameObject.SetActive(false);
		respawn?.gameObject.SetActive(false);
		// sparks?.gameObject.SetActive(false);
		fire?.gameObject.SetActive(false);
		blueFire?.gameObject.SetActive(false);
		embers?.gameObject.SetActive(false);
		blueEmbers?.gameObject.SetActive(false);

		Initialize();
	}

	void Update()
	{
		// leaving for testing
		//if (Input.GetKeyDown(KeyCode.Y))
		//{
		//	// StartEffect(sparks);
		//}
		//if (Input.GetKeyDown(KeyCode.U))
		//{
		//	StopParticleEffect(blueFire);
		//	StartEffect(fire);
		//}
		//if (Input.GetKeyDown(KeyCode.I))
		//{
		//	StopParticleEffect(fire);
		//	StartEffect(blueFire);
		//}
		//if (Input.GetKeyDown(KeyCode.J))
		//{
		//	StartEffect(embers);
		//}
		//if (Input.GetKeyDown(KeyCode.K))
		//{
		//	StartEffect(blueEmbers);
		//}
		//if (Input.GetKeyDown(KeyCode.O))
		//{
		//	rigidbody.useGravity = !rigidbody.useGravity;
		//}
	}

	void Initialize()
	{
		// very close to turning this into a list of data satructures. 
		InitializeParticleEffect(dissolve);
		InitializeParticleEffect(respawn);
		InitializeParticleEffect(fire);
		InitializeParticleEffect(blueFire);
		InitializeParticleEffect(embers);
		InitializeParticleEffect(blueEmbers);
	}

	void InitializeParticleEffect(Effect effect)
	{
		EffectParticle effectParticle = effect.GetEffectParticle(effect);
		if (effectParticle == null)
		{
			return;
		}

		GameObject particleParent = effectParticle.particleParent;

		if (particleParent)
		{
			Vector3 localScale = particleParent.transform.localScale;
			particleParent.transform.SetParent(effectTarget.transform);
			particleParent.transform.localPosition = effect.localOffset;
			particleParent.transform.localScale = localScale;
			effectTarget.effectParticles.Add(new EffectParticle(effect, particleParent));
		}
	}

	public void StartEffect(Effect effect)
	{
		StartParticleEffect(effect);
		StartShaderEffect(effect);

		effectTarget.StartEffect();
	}

	public void ResetEffects()
	{
		effectTarget.ResetEffect();
	}

	public void StartShaderEffect(Effect effect)
	{
		if (activeShaderEffect == null || activeShaderEffect != effect)
		{
			activeShaderEffect = effect;
			effectTarget.SetEffect(effect.effectTime, effect.fadeIn, effect.material, effect.ps, effect.shaderProperty);
		}

		if (effect.material != null)
		{
			effectTarget.renderer.material = effect.material;
		}
	}

	public void StartParticleEffect(Effect effect)
	{
		EffectParticle effectParticle = effectTarget.GetEffectParticle(effect);
		if (effectParticle == null)
		{
			Debug.Log("particle effect not found");
			return;
		}

		var particleParent = effectParticle.particleParent;
		particleParent.gameObject.SetActive(true);

	}

	public void StopParticleEffect(Effect effect)
	{
		var effectParticle = effectTarget.GetEffectParticle(effect);
		if (effectParticle == null)
		{
			return;
		}

		effectParticle.particleParent.gameObject.SetActive(false);
	}

	public void StopAllParticleEffects()
	{
		foreach (var particle in effectTarget.effectParticles)
		{
			particle.particleParent.SetActive(false);
		}
	}
}
