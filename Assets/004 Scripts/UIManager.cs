using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text lapCounterText;
    public TMP_Text bestLapTimeText;
    public TMP_Text totalTimeText;
    public TMP_Text positionText;
    public TMP_Text countdownText;
    public TMP_Text goPromptText;
    public TMP_Text raceResultText;

    public GameObject resultsScreen;
    public GameObject pauseScreen;
    public GameObject trackUnlockedMessage;

    public bool isPaused;

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
        // Pause Game if Esc is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void PauseUnpause()
    {
        isPaused = !isPaused;
        pauseScreen.SetActive(isPaused);

        //  Stop timeflow
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        //  Resume timeflow
        else 
        {
            Time.timeScale = 1f;
        }
    }

    public void ExitRace() 
    {
        Time.timeScale = 1f;
        RaceManager.instance.ExitRace();
    }

    public void QuitGame() 
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
