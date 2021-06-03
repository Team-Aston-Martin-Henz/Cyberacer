using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckpointChecker : MonoBehaviour
{
    // reference to player car
    public CarController car;

    // built-in function, called when colliding with trigger-type box collider component 
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Checkpoint") 
        {
            car.CheckpointHit(other.GetComponent<Checkpoint>().cpNumber); 
        }
    }
}