using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class EffectController : MonoBehaviour
{

    public GameObject ground;
    public Effect dissolve, respawn, sparks, fire, blueFire;

    public Effect effectTarget;
    Effect currentEffect;
    new Rigidbody rigidbody;

    float respawnTimer = 5f;

    // Start is called before the first frame update
    void Start()
    {
        dissolve?.gameObject.SetActive(false);
        respawn?.gameObject.SetActive(false);
        sparks?.gameObject.SetActive(false);
        fire?.gameObject.SetActive(false);
        blueFire?.gameObject.SetActive(false);

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
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
        if (Input.GetKeyDown(KeyCode.O))
        {
            rigidbody.useGravity = !rigidbody.useGravity;
        }
    }

    public void StartEffect(Effect effect)
	{
        effectTarget.ResetEffect();

        if (currentEffect != null)
		{
            currentEffect.gameObject.SetActive(false);
            currentEffect.particleParent.SetActive(false);
		}
        
        if (currentEffect == null || currentEffect != effect)
		{
            currentEffect = effect;
            effectTarget.SetEffect(effect.effectTime, effect.fadeIn, effect.material, effect.ps, effect.shaderProperty);
		}
        
        if (effect.material != null)
		{
            effectTarget.renderer.material = effect.material;
		}

        if (effect.particleParent)
		{
            Vector3 localScale = effect.particleParent.transform.localScale;
            effect.particleParent.transform.SetParent(transform);
            effect.particleParent.transform.localPosition = effect.localOffset;
            effect.particleParent.transform.localScale = localScale;
            effect.particleParent.gameObject.SetActive(true);
            effectTarget.particleParent = effect.particleParent;
		}

        effectTarget.StartEffect();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.gameObject == ground)
		{
            StartEffect(dissolve);
            rigidbody.useGravity = false;
            StartCoroutine(Respawn(respawnTimer));
		}
	}

    IEnumerator Respawn(float timer)
	{
        yield return new WaitForSeconds(timer);
        transform.position = transform.parent.position + new Vector3(0, 2, 0);
        StartEffect(respawn);
        yield return new WaitForSeconds(respawn.effectTime);
        rigidbody.useGravity = true;
	}
}
