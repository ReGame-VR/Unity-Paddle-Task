using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for rotating one GameObject around another.
/// </summary>
public class Rotation : MonoBehaviour
{
    /// The game object that will rotate around a center point
    [SerializeField]
    private GameObject game_object;

    /// The axis the object will rotate around - I.E. the axis that doesn't move.
    [SerializeField]
    private RotationAxis axis;

    /// the distance, in Unity units, from center to gameobject
    [SerializeField]
    private float radius;

    /// if the object rotates clockwise (otherwise it rotates counter clockwise)
    [SerializeField]
    private bool clockwise;

    /// The center x, y, and z coordinates and the x, y, and z coordinates of the gameobject
    private float centerx, centery, centerz, posx, posy, posz;

    /// the angle that the rotating object is currently on - dictated by time
    private float angle;

    /// the time, in seconds, it takes for one rotation
    [SerializeField]
    private float secondsPerRotation;

    private float speed; //2*PI in degress is 360, so you get 5 seconds to complete a circle

    /// <summary>
    /// Grab the current position of this object, initialize angle to 0, and set the speed based on
    /// given secondsperrotation parameter.
    /// </summary>
    void Start()
    {
        updateCenterPosn();
        angle = 0;
        speed = (2 * Mathf.PI) / secondsPerRotation;
    }


    /// updates the position based on rotation settings
    /// </summary>
    void Update()
    {
        // in case the object itself is moving
        updateCenterPosn();

        // determine new angle value based on direction of rotation
        if (clockwise)
        {
            angle += Time.deltaTime;
        }
        else
        {
            angle -= Time.deltaTime;
        }

        // change position values based on which axis object rotates around
        switch (axis)
        {
            case RotationAxis.x:
                updatePosn(centerx, centery + Mathf.Sin(angle * speed) * radius, centerz + Mathf.Cos(angle * speed) * radius);
                break;
            case RotationAxis.y:
                updatePosn(centerx + Mathf.Sin(angle * speed) * radius, centery, centerz + Mathf.Cos(angle * speed) * radius);
                break;
            case RotationAxis.z:
                updatePosn(centerx + Mathf.Sin(angle * speed) * radius, centery + Mathf.Cos(angle * speed) * radius, centerz);
                break;
        }

        // set the new position
        game_object.transform.position = new Vector3(posx, posy, posz);
    }

    /// <summary>
    /// helper method that updates the center position - used in Start for initialization and 
    /// Update for updating position of center if it is moving
    /// </summary>
    private void updateCenterPosn()
    {
        centerx = transform.position.x;
        centery = transform.position.y;
        centerz = transform.position.z;
    }

    /// <summary>
    /// A helper method that updates the position of the gameobject with the given values
    /// </summary>
    /// <param name="x"> the x position</param>
    /// <param name="y">the y position </param>
    /// <param name="z"> the z position </param>
    private void updatePosn(float x, float y, float z)
    {
        posx = x;
        posy = y;
        posz = z;
    }

    /// <summary>
    /// An enumeration of the axes.
    /// </summary>
    private enum RotationAxis { x, y, z };
}
