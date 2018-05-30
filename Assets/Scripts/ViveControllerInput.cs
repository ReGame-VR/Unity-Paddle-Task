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

        if (Controller.GetHairTriggerDown())
        {
            // Ball is being reset; it is not bouncing yet.
            ball.GetComponent<Ball>().isBouncing = false;

            // Disable the ball collider while it is being reset
            ball.GetComponent<SphereCollider>().enabled = false;
        }

        if (Controller.GetHairTrigger())
        {
            ball.transform.position = holdPoint.transform.position;
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }

        if (Controller.GetHairTriggerUp())
        {
            //Enable the ball's collider so it can be paddled.
            ball.GetComponent<SphereCollider>().enabled = true;

            // Give the ball the controller's velocity so that it is thrown realistically
            ball.GetComponent<Rigidbody>().velocity = Controller.velocity;
            ball.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
            ball.GetComponent<Ball>().ResetBall();
        }
    }
}
