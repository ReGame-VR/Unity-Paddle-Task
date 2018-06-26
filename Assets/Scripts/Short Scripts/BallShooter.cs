using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour {

    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private GameObject spawnLocation;

    public void RocketBall()
    {
        // Ball is being reset; it is not bouncing yet.
        ball.GetComponent<Ball>().isBouncing = false;

        ball.transform.position = spawnLocation.transform.position;
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 7, -1f);

        ball.GetComponent<Ball>().ResetBall();
    }
}
