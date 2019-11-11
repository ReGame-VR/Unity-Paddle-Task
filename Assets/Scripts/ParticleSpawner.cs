using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{

    // The game over particles to be spawned
    [SerializeField]
    private GameObject particles;

    public void SpawnParticles()
    {
        Instantiate(particles, transform.position, Quaternion.identity);
    }
}