using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{

    // The materials used to display this paddle. If it is transparent,
    // disable the collider.
    [SerializeField]
    private Material opaquePaddleMat;
    [SerializeField]
    private Material opaqueBacksideMat;
    [SerializeField]
    private Material transparentPaddleMat;
    [SerializeField]
    private Material transparentBacksideMat;

    // The collider for this paddle
    [SerializeField]
    private GameObject paddleCollider;

    // The mesh renderers for this paddle. Modify the material that they use.
    [SerializeField]
    private GameObject paddleModel;
    [SerializeField]
    private GameObject backsideModel;

    public void EnablePaddle()
    {
        paddleCollider.SetActive(true);

        paddleModel.GetComponent<MeshRenderer>().material = opaquePaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = opaqueBacksideMat;
    }

    public void DisablePaddle()
    {
        paddleCollider.SetActive(false);

        paddleModel.GetComponent<MeshRenderer>().material = transparentPaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = transparentBacksideMat;
    }

    public bool ColliderIsActive()
    {
        return paddleCollider.activeInHierarchy;
    }

    public Vector3 GetVelocity()
    {
        return paddleCollider.GetComponent<VelocityNoRigidBody>().GetVelocity();
    }

    public float GetAcceleration()
    {
        return paddleCollider.GetComponent<VelocityNoRigidBody>().GetAcceleration();
    }
}

