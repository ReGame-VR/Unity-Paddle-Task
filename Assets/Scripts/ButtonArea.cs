using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonArea : MonoBehaviour {

    public BallShooter shooter;

    // If the paddle is within this range, trigger the reset
    [SerializeField]
    private float distanceToActivate = 0.1f;

    // This is true if the ball was just reset. We dont want to reset the ball again
    // if it was just reset.
    private bool justTriggered;

    [SerializeField]
    private GameObject leftPaddle;

    [SerializeField]
    private GameObject rightPaddle;

    void Update()
    {
        float leftDistance = Vector3.Distance(leftPaddle.transform.position, transform.position);
        float rightDistance = Vector3.Distance(rightPaddle.transform.position, transform.position);

        // If either paddle is within the trigger range, shoot the ball
        if (leftDistance < distanceToActivate || rightDistance < distanceToActivate)
        {
            shooter.RocketBall();
            justTriggered = true;
            StartCoroutine(ResetTrigger());
            leftPaddle.GetComponent<Paddle>().EnablePaddle();
            rightPaddle.GetComponent<Paddle>().EnablePaddle();
        }
    }

    // Wait a few seconds before the trigger is reset
    IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(3);
        justTriggered = false;
    }
}
