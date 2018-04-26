using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInput : MonoBehaviour {

    // This controller being tracked by Vive system
    private SteamVR_TrackedObject trackedObj;

    // The object that the controller is currently holding
    private GameObject objectInHand;

    // The 3D point at which the ball will be held when the trigger is pressed
    [SerializeField]
    private GameObject holdPoint;

    // The ball that will be grabbed by this controller
    [SerializeField]
    private GameObject ball;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update () {
        /*
        if (Controller.GetHairTriggerDown())
        {
            GrabObject();            
        }

        if (Controller.GetHairTriggerUp())
        {
            ReleaseObject();            
        }
        */

        if (Controller.GetHairTrigger())
        {
            ball.transform.position = holdPoint.transform.position;
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        }

        if (Controller.GetHairTriggerUp())
        {
            ball.GetComponent<Rigidbody>().velocity = Controller.velocity;
            ball.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
    }

    // Grab the ball and hold it on the designated point on the controller
    private void GrabObject()
    {
        ball.transform.position = holdPoint.transform.position;
        objectInHand = ball;
        
        // Add a joint between grabbed ball and controller 
        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    // Generate a joint for use between the ball and controller
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    // Release the ball being held
    private void ReleaseObject()
    {
        // Make sure there is joint attached to this controller
        if (GetComponent<FixedJoint>())
        {
            // Destroy the joint
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            // Add controller velocity to the object's velocity
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
        // No longer holding anything
        objectInHand = null;
    }
}
