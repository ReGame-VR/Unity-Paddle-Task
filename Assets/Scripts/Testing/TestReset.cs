using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TestReset : MonoBehaviour
{
    public SteamVR_Behaviour_Pose paddlePose;

    new Rigidbody rigidbody;
    GameObject point;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
            paddlePose.enabled = true;
            rigidbody.useGravity = true;
            rigidbody.constraints = RigidbodyConstraints.None;

            if (point != null)
            {
                Destroy(point);
            }
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.gameObject.name == "Ground")
		{
            // rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
            rigidbody.useGravity = false;
            transform.position = new Vector3(0, 2, .705f);
            transform.rotation = Quaternion.identity;

            StartCoroutine(Reset(1f));
        }
		else if (collision.gameObject.tag == "Paddle")
		{
            Debug.LogFormat("Collided with {0} at {1} with contact count", collision.gameObject.name, Time.time, collision.contactCount);

            paddlePose.enabled = false;
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            point = GameObject.CreatePrimitive(PrimitiveType.Cube);
            point.transform.localScale = Vector3.one * .05f;
            point.transform.position = collision.GetContact(0).point;


            StopCoroutine("Reset");

		}
	}

    IEnumerator Reset(float seconds)
	{
        yield return new WaitForSeconds(seconds);
        rigidbody.useGravity = true;
        

	}
}
