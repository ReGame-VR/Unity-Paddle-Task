using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViveControllerInput : MonoBehaviour
{
    // This controller being tracked by Vive system
    //private SteamVR_TrackedObject trackedObj;
    public SteamVR_Action_Boolean resetAction;
    // The object that the controller is currently holding
    private GameObject objectInHand;

    public SteamVR_Input_Sources left, right;
    public GameObject leftController, rightController;

    // The 3D point at which the ball will be held when the trigger is pressed
    [SerializeField]
    private GameObject holdPoint;

    // The ball that will be grabbed by this controller
    //[SerializeField]
    //private GameObject ball;

    private VelocityEstimator velocity;
    //   SteamVR_Controller.Device Controller
    //  {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}

    void Awake()
    {
        // trackedObj = GetComponent<SteamVR_TrackedObject>();
        velocity = GetComponent<VelocityEstimator>();
    }

    // Update is called once per frame
    void Update()
    {

        // if (resetAction.GetState(left) || resetAction.GetState(right))
        //// if (Controller.GetHairTriggerDown())
        // {
        //     // Ball is being reset; it is not bouncing yet.
        //     ball.GetComponent<Ball>().isBouncing = false;

        //     // Disable the ball collider while it is being reset
        //     ball.GetComponent<SphereCollider>().enabled = false;
        // }


        if (resetAction.GetState(left) || resetAction.GetState(right))
        // if (Controller.GetHairTrigger())
        {
            GetComponent<SphereCollider>().enabled = false;
            transform.position = holdPoint.transform.position;
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            
            // Throwable t= ball.GetComponent<Throwable>();
           // VelocityEstimator velocity = GetComponent<VelocityEstimator>();
           // Debug.Log("release vel " + velocity.GetVelocityEstimate() + ", angular " + velocity.GetAngularVelocityEstimate());

        }

        if (resetAction.GetStateUp(left) || resetAction.GetStateUp(right))
        // if (Controller.GetHairTriggerUp())
        {
            //VelocityEstimator velocity = resetAction.GetStateUp(left) ? leftController.GetComponent<VelocityEstimator>() : rightController.GetComponent<VelocityEstimator>();
           // Debug.Log("release vel " + velocity.GetVelocityEstimate() + ", angular " + velocity.GetAngularVelocityEstimate());

            //Enable the ball's collider so it can be paddled.
            GetComponent<SphereCollider>().enabled = true;

            // Give the ball the controller's velocity so that it is thrown realistically
            GetComponent<Rigidbody>().velocity = velocity.GetVelocityEstimate();
            GetComponent<Rigidbody>().angularVelocity = velocity.GetAngularVelocityEstimate();
            GetComponent<Ball>().ResetBall();
        }
    }
}
