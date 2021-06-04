using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //  target to follow in Uuity Inspector -> set to player car
    public CarController target;

    private Vector3 offsetDirection;
    
    //  set in Unity Inspector -> min = 15, max = 35
    public float minDistance, maxDistance;
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

        //  set to unit
        offsetDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        //  set the dist of top down camera to adjust according to speed
        activeDistance = minDistance + ((maxDistance - minDistance) * (target.theRB.velocity.magnitude / target.maxSpeed));

        //  constantly update the camera's geometric position to the offset
        transform.position = target.transform.position + (offsetDirection * activeDistance);
    }
}
