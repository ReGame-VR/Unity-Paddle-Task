using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViveControllerInput : MonoBehaviour
{
    // the bound action to use for reset
    public SteamVR_Action_Boolean resetAction;
    // The object that the controller is currently holding
    // private GameObject objectInHand;

    public SteamVR_Input_Sources source;
    // The 3D point at which the ball will be held when the trigger is pressed
    [SerializeField]
    private GameObject holdPoint;

    // The ball that will be grabbed by this controller
    [SerializeField]
    private GameObject ball;
    private VelocityEstimator velocityEstimate;

    void Awake()
    {
        velocityEstimate = GetComponent<VelocityEstimator>();
    }

    void Update()
    {
        if (resetAction.GetState(source))
        {
            ball.GetComponent<SphereCollider>().enabled = false;
            ball.transform.position = holdPoint.transform.position;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        if (resetAction.GetStateUp(source))
        {
            //Enable the ball's collider so it can be paddled.
            ball.GetComponent<SphereCollider>().enabled = true;
            // Give the ball the controller's velocity so that it is thrown realistically
            ball.GetComponent<Rigidbody>().velocity = velocityEstimate.GetVelocityEstimate();
            ball.GetComponent<Rigidbody>().angularVelocity = velocityEstimate.GetAngularVelocityEstimate();
            ball.GetComponent<Ball>().ResetBall();
        }
    }
}
