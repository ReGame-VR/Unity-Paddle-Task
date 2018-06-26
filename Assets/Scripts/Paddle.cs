using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    public enum PaddleIdentifier {LEFT, RIGHT};

    // Is this the left paddle or the right paddle?
    private PaddleIdentifier paddleIdentifier;

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

    // Enable this paddle. Make it visible and turn on collider
    public void EnablePaddle()
    {
        paddleCollider.SetActive(true);

        paddleModel.GetComponent<MeshRenderer>().material = opaquePaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = opaqueBacksideMat;
    }

    // Disable this paddle. Make it transparent and turn off collider.
    public void DisablePaddle()
    {
        paddleCollider.SetActive(false);

        paddleModel.GetComponent<MeshRenderer>().material = transparentPaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = transparentBacksideMat;
    }

    // Is the collider on this paddle active?
    public bool ColliderIsActive()
    {
        return paddleCollider.activeInHierarchy;
    }

    // Gets velocity of paddle
    public Vector3 GetVelocity()
    {
        return paddleCollider.GetComponent<VelocityNoRigidBody>().GetVelocity();
    }

    // Gets acceleration of paddle
    public float GetAcceleration()
    {
        return paddleCollider.GetComponent<VelocityNoRigidBody>().GetAcceleration();
    }

    // Set up this paddle as the left or right paddle
    public void SetPaddleIdentifier(PaddleIdentifier paddleId)
    {
        paddleIdentifier = paddleId;
    }

    public PaddleIdentifier GetPaddleIdentifier()
    {
        return paddleIdentifier;
    }
}

