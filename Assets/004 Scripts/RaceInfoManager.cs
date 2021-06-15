using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceInfoManager : MonoBehaviour
{
    public static RaceInfoManager instance;

    public string trackToLoad;
    public CarController racerToUse;
    public int noOfAI;
    public int noOfLap;

    public void Awake()
    {
        //  Ensure we only have 1 race info manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
