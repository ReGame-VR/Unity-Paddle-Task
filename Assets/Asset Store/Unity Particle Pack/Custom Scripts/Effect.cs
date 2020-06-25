using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
	public float effectTime = 4;
	public AnimationCurve fadeIn;
	public Material material;
	public List<EffectParticle> effectParticles = new List<EffectParticle>();
	[NonSerialized]
	public ParticleSystem ps;
	new public Renderer renderer;
	[NonSerialized]
	public int shaderProperty;
	public Vector3 localOffset = Vector3.zero;

	float timer = 0;

	bool playing;

	void Awake()
	{
		renderer = GetComponent<Renderer>();
		shaderProperty = Shader.PropertyToID("_cutoff");
		ps = GetComponentInChildren<ParticleSystem>();
		if (ps != null)
		{
			var main = ps.main;
			main.duration = effectTime;
		}
	}

	public void SetEffect(float effectTimeVar, AnimationCurve fadeInVar, Material materialVar, ParticleSystem psVar, int shaderPropertyVar)
	{
		effectTime = effectTimeVar;
		fadeIn = fadeInVar;
		material = materialVar;
		ps = psVar;
		shaderProperty = shaderPropertyVar;
	}

	void Update()
	{
		if (playing)
		{
			if (timer < effectTime)
			{
				timer += Time.deltaTime;
			}
			else
			{
				// ResetEffect();
			}

			renderer.material.SetFloat(shaderProperty, fadeIn.Evaluate(Mathf.InverseLerp(0, effectTime, timer)));
		}
	}

	public void ResetEffect()
	{

		playing = false;
		timer = 0;
	}

	public void StartEffect()
	{
		playing = true;
		ps.Play();
	}

	public  EffectParticle GetEffectParticle(Effect effect)
	{
		foreach (var particle in effectParticles)
		{
			if (particle.effect == effect)
			{
				return particle;
			}
		}

		return null;
	}

}
