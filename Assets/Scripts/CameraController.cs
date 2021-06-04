using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // target car to follow
    public CarController target;

    // relative direction between camera and car
    private Vector3 offsetDirection;

    //  set in Unity Inspector -> min = 15, max = 35
    public float minDistance;
    public float maxDistance;
    private float activeDistance;

    //  An GameObject that is aligned geometrically with the playerCar object
    public Transform startTargetOffset;

    // Start is called before the first frame update
    void Start()
    {
        //  a constant geometric positioning difference from the playerCar to the top-down camera at the 1st frame
        offsetDirection = transform.position - startTargetOffset.position;

        //  activeDist -> set to minDist
        activeDistance = minDistance;
        // normalize offset direction
        offsetDirection.Normalize();
    }


    void Update()
    {
        // adjust active distance according to speed
        activeDistance = minDistance + ((maxDistance - minDistance) * (target.rigidBody.velocity.magnitude / target.maxSpeed));
        // update the camera's geometric position
        transform.position = target.transform.position + (offsetDirection * activeDistance);
    }
}
