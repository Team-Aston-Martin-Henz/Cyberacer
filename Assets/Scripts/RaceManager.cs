using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    // singleton instance for CarController to access number of checkpoints
    public static RaceManager instance;
    public Checkpoints[] allCheckPoints;
    public int totalLaps;

    //  Awake function happens every time an object is activated or deactivated in scene
    //  NOTE: Awake() fucntion happens before start() function
    private void Awake()
    {
        // assign this to instance to make sure only 1 race manager exists
        instance = this;
    }


    void Start()
    {
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].cpNumber = i;
        }
    }


    void Update()
    {

    }
}
