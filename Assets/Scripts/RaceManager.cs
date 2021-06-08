using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    // singleton instance for CarController to access number of checkpoints
    public static RaceManager instance;
    public Checkpoint[] allCheckpoints;
    public int totalLaps;

    //  Tracker for player's position
    public CarController playerCar;
    public List<CarController> allAICars = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPositionCheck = .2f;    //  check for every 0.2s
    private float positionCheckCounter;

    //  (Inspired from Mario Kart) 
    //  Rubberbanding Mechanics => Allows cars to be faster if at last position
    //  and slower if at first position
    public float aiDefaultSpeed = 30f;
    public float playerDefaultSpeed = 30f;
    public float rubberBandSpeedMod = 3.5f;
    public float rubberBandAccel = .5f;

    //
    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    private float startCounter;
    public int countDownCurrent = 3;

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

        isStarting = true;
        startCounter = timeBetweenStartCount;

        UIManager.instance.countDownText.text = countDownCurrent + "!";
    }


    // Update is called once per frame
    void Update()
    {
        //  when we are counting down
        if (isStarting)
        {
            startCounter -= Time.deltaTime;
            if (startCounter <= 0) 
            {
                countDownCurrent--;
                startCounter = timeBetweenStartCount;

                UIManager.instance.countDownText.text = countDownCurrent + "!";

                //  Countdown to zero, exit countdown mode
                if (countDownCurrent == 0) 
                {
                    isStarting = false;

                    UIManager.instance.countDownText.gameObject.SetActive(false);
                    UIManager.instance.goPromptText.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            /// <Position UI> <Need to Refactor>
            positionCheckCounter -= Time.deltaTime;
            if (positionCheckCounter <= 0f)
            {
                //  For each frame, player's position is always 1
                playerPosition = 1;

                //  for all cars
                foreach (CarController aiCar in allAICars)
                {
                    //  If 1 AI Car is 1 lap ahead of player, player's position increment by 1
                    if (aiCar.currentLap > playerCar.currentLap)
                    {
                        playerPosition++;
                    }
                    //  if AI Car and Player Car on the same lap
                    else if (aiCar.currentLap == playerCar.currentLap)
                    {
                        //  If AI Car's next Checkpoint is larger than player's, player's position increment by 1
                        if (aiCar.nextCheckpoint > playerCar.nextCheckpoint)
                        {
                            playerPosition++;
                        }

                        //  If AI Car's next Checkpoint and Player's are the same, 
                        else if (aiCar.nextCheckpoint == playerCar.nextCheckpoint)
                        {
                            //  Increment Player's Position if AI Car is nearer to the next Checkpoint
                            if (Vector3.Distance(aiCar.transform.position, allCheckpoints[aiCar.nextCheckpoint].transform.position)
                                < Vector3.Distance(playerCar.transform.position, allCheckpoints[aiCar.nextCheckpoint].transform.position))
                            {
                                playerPosition++;
                            }
                        }

                    }
                }
                positionCheckCounter = timeBetweenPositionCheck;

                //  Position Display UI
                UIManager.instance.positionText.text = playerPosition + "/" + (allAICars.Count + 1);
            }

            /// <Manage rubberbanding> 
            /// <Need to be refactor>
            //  If player is first
            if (playerPosition == 1)
            {
                //  for all AI car, we make their maxSpeed higher
                foreach (CarController aiCar in allAICars)
                {
                    aiCar.maxSpeed =
                        Mathf.MoveTowards(aiCar.maxSpeed,
                        aiDefaultSpeed + rubberBandSpeedMod,
                        rubberBandAccel * Time.deltaTime
                        );
                }

                //  for player, we make it slower
                playerCar.maxSpeed =
                    Mathf.MoveTowards(
                        playerCar.maxSpeed,
                        playerDefaultSpeed - rubberBandSpeedMod,
                        rubberBandAccel * Time.deltaTime
                        );
            }
            //  if player is not first
            else
            {
                //  for all AI cars, we make their maxSpeed lower
                foreach (CarController aiCar in allAICars)
                {
                    aiCar.maxSpeed =
                        Mathf.MoveTowards(aiCar.maxSpeed,
                        aiDefaultSpeed - (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))),
                        rubberBandAccel * Time.deltaTime
                        );
                }

                //  for player, we make it faster
                playerCar.maxSpeed =
                    Mathf.MoveTowards(
                        playerCar.maxSpeed,
                        playerDefaultSpeed + (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))),
                        rubberBandAccel * Time.deltaTime
                        );
            }

        }
    }
}
