using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffect : MonoBehaviour
{

    
    public float effectTime = 4;
    public AnimationCurve fadeIn;
    public ParticleSystem ps;
    float timer = 0;
    Renderer _renderer;

    int shaderProperty;
    bool playing = false;

    void Awake()
    {
        shaderProperty = Shader.PropertyToID("_cutoff");
        _renderer = GetComponent<Renderer>();
        ps = GetComponentInChildren<ParticleSystem>();

        var main = ps.main;
        main.duration = effectTime;
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
                ResetEffect();
            }
		}
        
        _renderer.material.SetFloat(shaderProperty, fadeIn.Evaluate(Mathf.InverseLerp(0, effectTime, timer)));
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


}
