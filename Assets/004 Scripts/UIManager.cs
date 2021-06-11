using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text lapCounterText;
    public TMP_Text bestLapTimeText;
    public TMP_Text currentLapTimeText;
    public TMP_Text positionText;
    public TMP_Text countdownText;
    public TMP_Text goPromptText;
    public TMP_Text raceResultText;

    public GameObject resultsScreen;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitRace() 
    {
        RaceManager.instance.ExitRace();
    }
}
