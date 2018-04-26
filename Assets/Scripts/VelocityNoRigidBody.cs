using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class for calculating an object's velocity if that object is
// not being moved by the physics engine
public class VelocityNoRigidBody : MonoBehaviour {

    // The position of the object at the beginning of the frame
    private Vector3 prevPosition;

    // current velocity of the object
    private Vector3 currVel;

    void Start()
    {
        prevPosition = transform.position;
    }

    void Update()
    {
        // Position at frame start
        prevPosition = transform.position;
        StartCoroutine(CalcVelocity(prevPosition));
    }

    // Get the current velocity of the object
    public Vector3 GetVelocity()
    {
        return currVel;
    }

    IEnumerator CalcVelocity(Vector3 pos)
    {
        // Wait till it the end of the frame
        // Velocity = DeltaPosition / DeltaTime
        yield return new WaitForEndOfFrame();
        currVel = (pos - transform.position) / Time.deltaTime;
    }
}
