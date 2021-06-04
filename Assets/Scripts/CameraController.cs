using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // target car to follow
    public CarController target;

    // relative direction between camera and car
    private Vector3 offsetDirection;

    // to set in Unity inspector, default min = 15, max = 35
    public float minDistance;
    public float maxDistance;
    private float activeDistance;

    // game object that is aligned geometrically with the car
    public Transform startTargetOffset;

    // Start is called before the first frame update
    void Start()
    {
        // constant geometric positioning difference from the car to the top-down camera at the 1st frame
        offsetDirection = transform.position - startTargetOffset.position;
        // set min distance as active distance
        activeDistance = minDistance;
        // normalize offset direction
        offsetDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        // adjust active distance according to speed
        activeDistance = minDistance + ((maxDistance - minDistance) * (target.theRB.velocity.magnitude / target.maxSpeed));
        // update the camera's geometric position
        transform.position = target.transform.position + (offsetDirection * activeDistance);
    }
}
