using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class for calculating an object's velocity if that object is
// not being moved by the physics engine
public class VelocityNoRigidBody : MonoBehaviour {

    // FOR VELOCITY CALCULATION:
    // The position of the object at the beginning of the frame
    private Vector3 prevPosition;
    // current velocity of the object
    private Vector3 currVel;


    // FOR ACCELERATION CALCULATION:
    // The velocity of the object at the beginning of the frame
    private Vector3 prevVelocity;
    // current acceleration of the object
    private Vector3 currAccel;


    void Start()
    {
        prevPosition = transform.position;
        currVel = new Vector3(0, 0, 0);
    }

    void Update()
    {
        // Position at frame start
        prevPosition = transform.position;
        StartCoroutine(CalcVelocity(prevPosition));

        // Velocity at frame start
        prevVelocity = currVel;
        StartCoroutine(CalcAcceleration(prevVelocity));
    }

    IEnumerator CalcVelocity(Vector3 pos)
    {
        // Wait till it the end of the frame
        // Velocity = DeltaPosition / DeltaTime
        yield return new WaitForEndOfFrame();
        currVel = (pos - transform.position) / Time.deltaTime;
    }

    IEnumerator CalcAcceleration(Vector3 vel)
    {
        // Wait till it the end of the frame
        // Acceleration = DeltaVelocity / DeltaTime
        yield return new WaitForEndOfFrame();
        //currAccel = (currVel.magnitude - prevVelocity.magnitude) / Time.deltaTime;
        currAccel = (currVel - prevVelocity) / Time.deltaTime;
    }

    // Get the current velocity of the object
    public Vector3 GetVelocity()
    {
        return currVel;
    }

    // Get the current velocity of the object
    public Vector3 GetAcceleration()
    {
        return currAccel;
    }
}
