using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class EffectController : MonoBehaviour
{
    // public GameObject ground;
    public Effect dissolve, respawn, /*sparks,*/ fire, blueFire, embers, blueEmbers;

    public Effect effectTarget;
    Effect activeShaderEffect;
    new Rigidbody rigidbody;

    float respawnTimer = 5f;

   //  private List<Effect> activeParticleEffects = new List<Effect>();

    // public List<Effect> ActiveParticleEffects { get { return activeParticleEffects; } }

    // Start is called before the first frame update
    void Start()
    {
        dissolve?.gameObject.SetActive(false);
        respawn?.gameObject.SetActive(false);
        // sparks?.gameObject.SetActive(false);
        fire?.gameObject.SetActive(false);
        blueFire?.gameObject.SetActive(false);
        embers?.gameObject.SetActive(false);
        blueEmbers?.gameObject.SetActive(false);

        // rigidbody = GetComponent<Rigidbody>();
        // rigidbody.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
		{
            // StartEffect(sparks);
		}
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartEffect(fire);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartEffect(blueFire);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartEffect(embers);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartEffect(blueEmbers);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            rigidbody.useGravity = !rigidbody.useGravity;
        }
    }


    public void StartEffect(Effect effect)
	{
        StartShaderEffect(effect);
        StartParticleEffect(effect);

        effectTarget.StartEffect();
	}

    public void ResetEffects()
	{
        effectTarget.ResetEffect();
    }

    public void StartShaderEffect(Effect effect)
	{
        if (activeShaderEffect != null)
        {
            activeShaderEffect.gameObject.SetActive(false);
        }

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
        if (effectTarget.GetEffectParticle(effect) != null)
		{
            // effectTarget.effectParticles.Add(effect);
            // no support for multiple same particle effects currently
            return; 
		}
		//else
		//{
		//}

        var particleParent = effect.GetEffectParticle(effect).particleParent;
        if (particleParent)
        {
            Vector3 localScale = particleParent.transform.localScale;
            particleParent.transform.SetParent(transform);
            particleParent.transform.localPosition = effect.localOffset;
            particleParent.transform.localScale = localScale;
            particleParent.gameObject.SetActive(true);
            effectTarget.effectParticles.Add(new EffectParticle(effect, particleParent));
        }
    }

    public void StopParticleEffect(Effect effect)
	{
        var effectParticle = effect.GetEffectParticle(effect);
        effect.effectParticles.Remove(effectParticle);
        effectParticle.particleParent.gameObject.SetActive(false);
	}

    public void StopAllParticleEffects()
	{
        foreach (var particle in effectTarget.effectParticles)
        {
            particle.particleParent.SetActive(false);
        }

        effectTarget.effectParticles.Clear();

    }

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (collision.transform.gameObject == ground)
	//	{
 //           StartEffect(dissolve);
 //           rigidbody.useGravity = false;
 //           StartCoroutine(Respawn(respawnTimer));
	//	}
	//}

 //   IEnumerator Respawn(float timer)
	//{
 //       yield return new WaitForSeconds(timer);
 //       transform.position = transform.parent.position + new Vector3(0, 2, 0);
 //       StartEffect(respawn);
 //       yield return new WaitForSeconds(respawn.effectTime);
 //       rigidbody.useGravity = true;
	//}
}
