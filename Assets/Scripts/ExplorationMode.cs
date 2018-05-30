using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorationMode : MonoBehaviour {

    [Tooltip("The target line that may be moved by this exploration script.")]
    [SerializeField]
    private GameObject targetLine;

    [Tooltip("The ball whose physics may be modified by this script")]
    [SerializeField]
    private GameObject ball;

    // The previous index of the height list that was chosen.
    private int prevHeightIndex = 0;

    // The previous index of the bounce modification that was chosen.
    private int prevBounceModIndex = 0;

    // The height of the user's eyes (y position in m)
    private float eyeLevel;

    [Tooltip("The list of target height ratios (compared to eye height) that the line may be moved to.")]
    public List<float> targetHeightRatios;

    [Tooltip("The list of target height ratios (compared to eye height) that the line may be moved to.")]
    public List<Vector3> bounceModifications;

    void Start()
    {
        ball.GetComponent<Ball>().SetBounceModification(bounceModifications[0]);
    }

    // Set up where the eye level for this game is
    public void CalibrateEyeLevel(float eyeLevel)
    {
        this.eyeLevel = eyeLevel;
    }

    // Move the target line to a different position in the task exploration mode
    public void MoveTargetLine()
    {
        int newTargetHeightIndex = prevHeightIndex;

        // Choose a new target height index thats different from the previous target index
        while (newTargetHeightIndex == prevHeightIndex)
        {
            newTargetHeightIndex = Random.Range(0, targetHeightRatios.Count);
        }

        // Move just the height of the target line
        Vector3 prevPos = targetLine.transform.position;
        targetLine.transform.position = new Vector3(prevPos.x, eyeLevel * targetHeightRatios[newTargetHeightIndex], prevPos.z);

        targetLine.GetComponent<ParticleSpawner>().SpawnParticles();

        prevHeightIndex = newTargetHeightIndex;
    }

    // Make the bounce behave differently
    public void ModifyBouncePhysics()
    {
        int newModIndex = prevBounceModIndex;

        // Choose a new bounce modification that is different from the previous bounce modification
        while (newModIndex == prevBounceModIndex)
        {
            newModIndex = Random.Range(0, bounceModifications.Count);
        }

        // Tell the ball the new bounce modification
        ball.GetComponent<Ball>().SetBounceModification(bounceModifications[newModIndex]);

        prevBounceModIndex = newModIndex;
    }
}
