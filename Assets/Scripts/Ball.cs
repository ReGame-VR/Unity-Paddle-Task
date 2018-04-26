using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    // The force at which the ball will bounce upon collisions
    [SerializeField]
    private float bounceForce;

    void OnCollisionEnter(Collision c)
    {
        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        // bumper effect to speed up ball
        GetComponent<Rigidbody>().AddForce(c.contacts[0].normal * bounceForce, ForceMode.Impulse);

    }
}
