using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    //  Singleton
    public static RaceManager instance;
    public Checkpoints[] allCheckPoints;
    public int totalLaps;

    //  Awake function happens every time an object is activated or deactivated in scene
    //  NOTE: Awake() fucntion happens before start() function
    private void Awake()
    {
        //  Immediately when the game is activated, instance is refered to the "Race Manager"
        //  GameObject. If we create a "Race Manager (2)" GameObject in our class, instance will then refer
        //  to "Race Manager (2)" GameObject. This ensures that we only have 1 race manager throughout.
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < allCheckPoints.Length; i++)
        {
            allCheckPoints[i].cpNumber = i;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
