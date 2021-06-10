using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    // singleton instance for CarController to access number of checkpoints
    public static RaceManager instance;

    // lap counting related variables
    public Checkpoint[] allCheckpoints;
    public int totalLaps;

    // tracker for player's position
    public CarController playerCar;
    public List<CarController> allAICars = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPositionCheck = .2f;
    private float positionCheckCounter;

    // rubber banding related variables
    public float aiDefaultSpeed = 30f;
    public float playerDefaultSpeed = 30f;
    public float rubberBandSpeedMod = 3.5f;
    public float rubberBandAccel = .5f;

    // starting countdown related variables
    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    private float startCounter;
    public int countdownCurrent = 3;

    //  randomised player and AI starting position
    public int playerStartPosition;
    public int aiNumberToSpawn;
    public Transform[] startPoints;
    public List<CarController> carsToSpawn = new List<CarController>();

    public bool raceCompleted;

    // Awake function happens every time an object is activated or deactivated in scene
    // Awake() happens before Start()
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

        isStarting = true;
        startCounter = timeBetweenStartCount;
        UIManager.instance.countdownText.text = "-" + countdownCurrent + "-";


        /// <NEW: CH9:L52>    
        /// Randomisation for player Car and AI Car
        playerStartPosition = Random.Range(0, aiNumberToSpawn + 1);     //  Total number of position (AI + Player)
        playerCar.transform.position = startPoints[playerStartPosition].position;   //  Alignment   
        playerCar.rigidBody.transform.position = startPoints[playerStartPosition].position; //  Alignment

        for (int i = 0; i < aiNumberToSpawn + 1; i++) 
        {
            if (i != playerStartPosition) 
            {
                int selectedCar = Random.Range(0, carsToSpawn.Count);

                //  Create the AI Car in the scene at its starting position
                //  Instantiate(GameObject, locationInfo, rotationInfo)
                allAICars.Add(Instantiate(carsToSpawn[selectedCar], startPoints[i].position, startPoints[i].rotation));

                //  If there is not enough AI cars to spawn, then we allow duplication
                if (carsToSpawn.Count > aiNumberToSpawn - i) 
                {
                    carsToSpawn.RemoveAt(selectedCar);
                }

            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        // when we are counting down
        if (isStarting)
        {
            UpdateCountdown();
            return;
        }

        // update driver positio
        UpdatePosition();

        // rubber banding
        if (playerPosition == 1)
        {
            FavourAI();
        } else
        {
            FavourPlayer();
        }
    }


    private void UpdateCountdown()
    {
        startCounter -= Time.deltaTime;

        if (startCounter > 0) return; 

        countdownCurrent--;
        startCounter = timeBetweenStartCount;
        UIManager.instance.countdownText.text = "-" + countdownCurrent + "-";

        if (countdownCurrent != 0) return; 

        isStarting = false;
        UIManager.instance.countdownText.gameObject.SetActive(false);
        UIManager.instance.goPromptText.gameObject.SetActive(true);
    }


    private void UpdatePosition()
    {
        positionCheckCounter -= Time.deltaTime;
        if (positionCheckCounter > 0f) return;

        playerPosition = GetPosition();
        positionCheckCounter = timeBetweenPositionCheck;
        UIManager.instance.positionText.text = playerPosition + "/" + (allAICars.Count + 1);
    }


    private int GetPosition()
    {
        int rank = 1;
        foreach (CarController aiCar in allAICars)
        {
            if (aiCar.currentLap < playerCar.currentLap) continue;
            if (aiCar.currentLap == playerCar.currentLap)
            {
                if (aiCar.nextCheckpoint < playerCar.nextCheckpoint) continue;
                if (aiCar.nextCheckpoint == playerCar.nextCheckpoint)
                {
                    Vector3 cp = allCheckpoints[aiCar.nextCheckpoint].transform.position;
                    Vector3 ai = aiCar.transform.position;
                    Vector3 player = playerCar.transform.position;
                    if (Vector3.Distance(ai, cp) > Vector3.Distance(player, cp)) continue;
                }
            }
            rank++;
        }
        return rank;
    }

    private void FavourAI()
    {
        foreach (CarController aiCar in allAICars)
        {
            aiCar.maxSpeed = Mathf.MoveTowards(
                aiCar.maxSpeed,
                aiDefaultSpeed + rubberBandSpeedMod,
                rubberBandAccel * Time.deltaTime
            );
        }

        playerCar.maxSpeed = Mathf.MoveTowards(
            playerCar.maxSpeed,
            playerDefaultSpeed - rubberBandSpeedMod,
            rubberBandAccel * Time.deltaTime
        );
    }


    private void FavourPlayer()
    {
        foreach (CarController aiCar in allAICars)
        {
            aiCar.maxSpeed = Mathf.MoveTowards(
                aiCar.maxSpeed,
                aiDefaultSpeed - (rubberBandSpeedMod * ((float) playerPosition / ((float) allAICars.Count + 1))),
                rubberBandAccel * Time.deltaTime
            );
        }

        playerCar.maxSpeed = Mathf.MoveTowards(
                playerCar.maxSpeed,
                playerDefaultSpeed + (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))),
                rubberBandAccel * Time.deltaTime
        );
    }

    public void FinishRace() 
    {
        raceCompleted = true;
    }

}
