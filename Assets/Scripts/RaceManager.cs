using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    // singleton instance for CarController to access number of checkpoints
    public static RaceManager instance;
    public Checkpoint[] allCheckpoints;
    public int totalLaps;

    //  Awake function happens every time an object is activated or deactivated in scene
    //  Awake() happens before Start()
    private void Awake()
    {
        // assign this to instance to make sure only 1 race manager exists
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].cpNumber = i;
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
