using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // reference to Rigidbody sphere
    public Rigidbody rigidBody;

    // car stats
    public float maxSpeed = 30f;
    public float forwardAccel = 8f;
    public float reverseAccel = 4f;
    public float turnStrength = 180f;

    // current speed and current turn
    private float speed;
    private float turn;

    // gravity related variables
    private bool isGrounded;
    public Transform groundRayPoint1, groundRayPoint2;
    public LayerMask groundLayer;
    public float groundrayLength = .75f;
    private float dragOnGround;
    public float gravity = -10f;

    // wheel-turning related variables
    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    // dust trail (particle system) related variables
    public ParticleSystem[] dustTrail;
    public float maxEmissionRate = 25f;
    public float emissionFadeRate = 20f;
    private float emissionRate;

    // sound related variables
    public AudioSource engineSFX, driftingSFX;
    public float driftingFadeRate = 2f;

    // race management related variables
    public int nextCheckpoint;
    public int currentLap;
    public float lapTime, bestLapTime;

    // boolean to indicate if the car is AI
    public bool isAI;

    // next point for the AI to move towards to
    public int currentTarget;
    private Vector3 targetPoint;

    // AI car variables
    public float aiAccel = 1f;
    public float aiTurnSpeed = .8f;
    public float aiReachPointRange = 5f;
    public float aiPointVariance = 3f;
    public float aiMaxTurn = 15f;
    public float aiLevelLowBound = .8f;
    public float aiLevelHighBound = 1.1f;
    private float aiLevel;
    private float aiSpeedInput;

    // reset Cooldown
    public float resetCoolDown = 2f;
    private float resetCounter;


    // Start is called before the first frame update
    void Start()
    {
        // separate rigidbody from the car at the start
        rigidBody.transform.parent = null;
        // setting the drag on ground to be equal to that of the sphere
        dragOnGround = rigidBody.drag;

        if (isAI) 
        {
            // randomise the AI level
            aiLevel = Random.Range(aiLevelLowBound, aiLevelHighBound);
            // reference to the centre of first checkpoint
            targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            RandomiseAITarget();
        }

        // display correct current lap count
        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        resetCounter = resetCoolDown;
    }


    // Update is called once per frame
    void Update()
    {
        // do not update the frame if we are in countdown
        if (RaceManager.instance.isStarting) return;

        if (isAI)
        {
            UpdateAISpeedAndTurn();
        }
        else
        {
            UpdatePlayerSpeedAndTurn();
            CheckReset();
        }

        UpdateSteering();
        UpdateDustTrail();
        UpdateEngineSFX();
        UpdateDriftingSFX();
        UpdateLapTimeDisplay();
    }


    // Update per delta time
    private void FixedUpdate()
    {
        if (RaceManager.instance.isStarting) return;
        FixedUpdateInclination();
        FixedUpdateCarMovement();
    }




    private void UpdateAISpeedAndTurn()
    {
        // keep our target point's height to be aligned to the height of the cars
        targetPoint.y = transform.position.y;
        if (Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
        {
            SetNextAITarget();
        }

        // direction vector towards the target
        Vector3 targetDirection = targetPoint - transform.position;
        // angle from the target point to true front of the car
        float angle = Vector3.Angle(targetDirection, transform.forward);
        // checks if the target point is on the left or on the right
        if (transform.InverseTransformPoint(targetPoint).x < 0f)
        {
            angle *= -1;
        }

        float finalSpeed = Mathf.Abs(angle) < aiMaxTurn
            ? 1f            // angle does not exceed maximum turn, accelerate
            : aiTurnSpeed;  // angle exceeds maximum turn, slow down till preset turning speed
        aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, finalSpeed, aiAccel);
        speed = aiLevel * aiSpeedInput * forwardAccel;

        // turn is limited to 1
        turn = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);
    }


    private void UpdatePlayerSpeedAndTurn()
    {
        float speedInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // update speed and turn
        speed = speedInput > 0
            ? speedInput * forwardAccel
            : speedInput * reverseAccel;
        turn = turnInput;
    }

  
    public void CheckReset()
    {
        if (resetCounter > 0)
        {
            resetCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.R) && resetCounter <= 0)
        {
            ResetToTrack();
        }
    }


    private void UpdateSteering()
    {
        leftFrontWheel.localRotation = Quaternion.Euler(
            leftFrontWheel.localRotation.eulerAngles.x,     //  no modification on x axis
            (turn * maxWheelTurn) - 180,                    //  offset 180 degree
            leftFrontWheel.localRotation.eulerAngles.z      //  no modification on y axis
        );
        rightFrontWheel.localRotation = Quaternion.Euler(
            rightFrontWheel.localRotation.eulerAngles.x,    //  no modification on x axis
            (turn * maxWheelTurn),                          //  no offset needed
            rightFrontWheel.localRotation.eulerAngles.z     //  no modification on y axis
        );
    }


    private void UpdateDustTrail()
    {
        float v = rigidBody.velocity.magnitude;

        // let emission rate decrease gradually
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeRate * Time.deltaTime);

        // set emission rate to be maximum when car is grounded &&
        if (isGrounded &&
            // car is turning or car os accelerating
            (Mathf.Abs(turn) > .5f || (0 < v && v < maxSpeed * .5f))) 
        {
            emissionRate = maxEmissionRate;
        }

        // set emission rate to 0 if speed is too slow
        if (v <= 0.5f)
        {
            emissionRate = 0;
        }

        // loop through the particle system array to set dust trail for all 4 wheels
        for (int i = 0; i < dustTrail.Length; i++) 
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }

    private void UpdateEngineSFX()
    {
        if (engineSFX == null)
        {
            Debug.Log("Engine SFX not found.");
            return;
        }

        engineSFX.pitch = 1f + (rigidBody.velocity.magnitude / maxSpeed) * 2f;
    }


    private void UpdateDriftingSFX()
    {
        if (driftingSFX == null)
        {
            Debug.Log("Drifting SFX not found.");
            return;
        }   

        if (!isGrounded) return;

        if (Mathf.Abs(turn) > .5f)
        {
            driftingSFX.volume = 1f;
        }
        else
        {
            driftingSFX.volume = Mathf.MoveTowards(driftingSFX.volume, 0f, driftingFadeRate * Time.deltaTime);
        }
    }


    private void UpdateLapTimeDisplay()
    {
        // lapTime incremented as according to the frame rate timing
        lapTime += Time.deltaTime;
        // convert from seconds to time format and display it accordingly
        var ts = System.TimeSpan.FromSeconds(lapTime);
        UIManager.instance.currentLapTimeText.text =
            string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);
    }


    private void FixedUpdateInclination()
    {
        isGrounded = false;
        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        // check if front wheel on ramp
        if (Physics.Raycast(groundRayPoint1.position, -transform.up, out hit, groundrayLength, groundLayer))
        {
            isGrounded = true;
            // align y-axis of car to ground ray check
            normalTarget = hit.normal;
        }

        // check if rear wheel on ramp
        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundrayLength, groundLayer))
        {
            isGrounded = true;
            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        // adjust car's inclination to match the geometric of the surface
        if (isGrounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }
    }


    private void FixedUpdateCarMovement()
    {
        if (isGrounded)
        // car on ground, accelerate towards the forward direction
        {
            // set sphere's drag to dragOnGround value
            rigidBody.drag = dragOnGround;
            rigidBody.AddForce(transform.forward * speed * 1000f);
        } else
        // car in the air, accelerate downwards due to gravity
        {
            rigidBody.drag = .1f;
            rigidBody.AddForce(Vector3.up * gravity * 100f);
        }

        // limit car speed to be below max speed
        if (rigidBody.velocity.magnitude > maxSpeed) 
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }

        // adjust car's orientation
        // use Time.deltaTime so that movement is consistent in different frame rates
        if (isGrounded && speed != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(
                    0f,
                    turn * turnStrength * Time.deltaTime * Mathf.Sign(speed) * (rigidBody.velocity.magnitude / maxSpeed),
                    0f)
                );
        }

        // realign car's position to sphere
        transform.position = rigidBody.position;
    }


    public void CheckpointHit(int cpNumber)
    {
        if (cpNumber == nextCheckpoint) 
        {
            nextCheckpoint++;
            if (nextCheckpoint == RaceManager.instance.allCheckpoints.Length) 
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }

        if (isAI && cpNumber == currentTarget)
        {
            SetNextAITarget();
        }
    }


    private void LapCompleted()
    {
        currentLap++;

        // do not update best lap time if car is AI
        if (isAI) return;
        if (lapTime < bestLapTime || bestLapTime == 0f)
        {
            bestLapTime = lapTime;
        }

        //  When we haven't complete our lap
        if (currentLap <= RaceManager.instance.totalLaps)
        {
            // reset lap time to 0 for a new lap
            lapTime = 0f;
            // display best lap time
            var ts = System.TimeSpan.FromSeconds(bestLapTime);
            UIManager.instance.bestLapTimeText.text =
                string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);

            // display updated lap count
            UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        }
        //  When we completed our lap
        else 
        {
            if (!isAI) 
            {
                isAI = true;
                aiLevel = 1f;
                // reference to the centre of checkpoint
                targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                // allow the target point for each checkpoint to be different for all cars
                RandomiseAITarget();

                // display best lap time
                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text =
                    string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);

                RaceManager.instance.FinishRace();
            }
        }
    }

    private void SetNextAITarget() 
    {
        currentTarget++;

        // reset target to 0 once we finish
        if (currentTarget >= RaceManager.instance.allCheckpoints.Length)
        {
            currentTarget = 0;
        }

        // reference to the centre of checkpoint
        targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
        // allow the target point for each checkpoint to be different for all cars
        RandomiseAITarget();
    }


    private void RandomiseAITarget() 
    {
        targetPoint += new Vector3(
            Random.Range(-aiPointVariance, aiPointVariance), 
            0,
            Random.Range(-aiPointVariance, aiPointVariance) 
            );
    }


    private void ResetToTrack() 
    {
        // set to previous checkpoint
        int prevCheckpoint = nextCheckpoint == 0
            ? RaceManager.instance.allCheckpoints.Length - 1
            : nextCheckpoint - 1;

        transform.position = RaceManager.instance.allCheckpoints[prevCheckpoint].transform.position;
        rigidBody.transform.position = transform.position;
        rigidBody.velocity = Vector3.zero;

        speed = 0f;
        turn = 0f;

        resetCounter = resetCoolDown;
    }
}
