using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script extends the physics engine to allow for kinematic calculations.
/// It handles all the motion for the rigidbody this script is attached to
/// except for its useGravity property.
/// 
/// This script must be used in conjunction with a RigidBody and Collider. 
/// </summary>

public class Kinematics : MonoBehaviour
{
    // Reference to this GameObject's rigidbody
    private Rigidbody rb;
    
    public Vector3 storedPosition;
    public Vector3 storedVelocity;
    public Vector3 storedAngularVelocity;
    public Quaternion storedRotation;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        storedPosition = rb.position;
        storedVelocity = rb.velocity;
        storedAngularVelocity = rb.angularVelocity;
        storedRotation = Quaternion.identity;
    }

    // Handle physics
    void FixedUpdate()
    {
        // Handle Paused and Playing states separately.
        if (GlobalControl.Instance.paused)
        {
            DisableMotion();
        }
        else 
        {
            EnableMotion();
        }
    }

    // Contains all the procedures to resume physics. Fires once per input.
    public void TriggerResume()
    {
        // Resume motion
        rb.useGravity = true;
        rb.velocity        = storedVelocity;
        rb.angularVelocity = storedAngularVelocity;
        rb.rotation        = storedRotation;

        // Allow collisions
        rb.detectCollisions = true;
    }

    // Contains all the procedures to pause physics. Fires once per input.
    // TODO turn this into end-of-frame coroutine to avoid "teleporting" pause
    public void TriggerPause()
    {
        // Eliminate forces
        rb.useGravity = false;
        rb.velocity = rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.identity;

        // Do not collide
        rb.detectCollisions = false;
    }

    // Handles the paused physics state. Holds everything constant.
    void DisableMotion()
    {
        // Hold rb still
        rb.position = storedPosition;
    }

    // Handles the enabled (regular) physics state.
    void EnableMotion()
    {
        // Update stored variables
        storedPosition = rb.position;
        storedVelocity = rb.velocity;
        storedAngularVelocity = rb.angularVelocity;
        storedRotation = rb.rotation;
    }

    // Returns the reflected velocity vector of a given input vector and normal contact point.
    // Efficiency represents the percentage (0.0-1.0) of energy dissipated from the impact.  
    public Vector3 GetReflectionDamped(Vector3 inVector, Vector3 inNormal, float efficiency = 1.0f)
    {
        Vector3 reflected = Vector3.Reflect(inVector, inNormal);
        reflected *= efficiency;
        return reflected;
    }
}
