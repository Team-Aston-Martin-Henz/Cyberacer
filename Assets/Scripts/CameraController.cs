using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // target car to follow
    public CarController target;

    // relative direction between camera and car
    private Vector3 offsetDirection;

    // to set in Unity inspector
    public float minDistance;
    public float maxDistance;
    private float activeDistance;

    void Start()
    {   
        offsetDirection = transform.position - target.transform.position;
        // set min distance as active distance
        activeDistance = minDistance;
        // normalize offset direction
        offsetDirection.Normalize();
    }


    void Update()
    {
        // adjust active distance according to speed
        activeDistance = minDistance + ((maxDistance - minDistance) * (target.rb.velocity.magnitude / target.maxSpeed));
        // update the camera's geometric position
        transform.position = target.transform.position + (offsetDirection * activeDistance);
    }
}