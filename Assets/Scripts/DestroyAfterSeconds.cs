using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour {

    // The number of seconds that this object will wait
    // before destroying.
    [SerializeField]
    private int secondsToWait = 5;

    [SerializeField]
    private GameObject objectToDestroy;


    void Start()
    {
        StartCoroutine(WaitToDestroy());
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(secondsToWait);
        Destroy(objectToDestroy);
    }
}
