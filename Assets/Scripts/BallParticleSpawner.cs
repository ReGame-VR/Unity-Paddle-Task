using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallParticleSpawner : MonoBehaviour
{

    // The bounce particles to be spawned
    [SerializeField]
    private GameObject bounceParticles;

    // The success particles to be spawned
    [SerializeField]
    private GameObject successParticles;

    // Spawn particles indicating a bounce on the paddle
    public void SpawnBounceParticles()
    {
        Instantiate(bounceParticles, transform.position, Quaternion.identity);
    }

    // Spawn particles indicating a target line hit
    public void SpawnSuccessParticles()
    {
        Instantiate(successParticles, transform.position, Quaternion.identity);
    }
}