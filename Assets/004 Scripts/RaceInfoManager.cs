using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceInfoManager : MonoBehaviour
{
    public static RaceInfoManager instance;

    public string trackToLoad;
    public string practiceTrackToLoad;
    public CarController racerToUse;
    public int noOfAI;
    public int noOfLap;

    public bool enteredRace;
    public Sprite trackSprite;
    public Sprite racerSprite;

    public string trackToUnlock;

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

}
