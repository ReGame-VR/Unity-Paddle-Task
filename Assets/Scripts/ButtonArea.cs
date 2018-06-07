using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonArea : MonoBehaviour {

    public BallShooter shooter;

    void OnCollisionEnter(Collision c)
    {
        // On collision with paddle, ball should bounce
        if (c.gameObject.tag == "Paddle")
        {
            shooter.RocketBall();
        }
        else
        {
            // do nothing
        }
    }
}
