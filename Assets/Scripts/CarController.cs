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

    //  Next point for the AI to move towards to
    public int currentTarget;
    private Vector3 targetPoint;

    //  AI Car Property
    public float aiAccelerateSpeed = 1f;
    public float aiTurnSpeed = .8f;     //  The preset speed allocated to AI when it comes to turning
    public float aiReachPointRange = 5f;    //  Allowance given to be away from centre of checkpoints
    public float aiPointVariance = 3f;
    public float aiMaxTurn = 15f;
    private float aiSpeedInput;
    private float aiSpeedMod;

    //  Reset Cooldown
    public float resetCoolDown = 2f;
    private float resetCounter;

    //  Start is called before the first frame update
    void Start()
    {
        // separate rigidbody from the car at the start
        rigidBody.transform.parent = null;
        // setting the drag on ground to be equal to that of the sphere
        dragOnGround = rigidBody.drag;

        //  for AI cars
        if (isAI) 
        {
            //  referencing to the centre of checkpoint
            targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            //  allow the target point for each checkpoint to be different for all cars
            RandomiseAITarget();
            //  randomise the speed of AI Car
            aiSpeedMod = Random.Range(.8f, 1.1f);
        }

        // display correct current lap count
        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;

        resetCounter = resetCoolDown;
    }


    // Update is called once per frame
    void Update()
    {
        //  If we are not in Countdown
        if (!RaceManager.instance.isStarting)
        {
            if (!isAI)
            {
                //  for player
                UpdateSpeedAndTurn();

                /// <Reset Car Position to last Checkpoint>
                if (resetCounter > 0)
                {
                    resetCounter -= Time.deltaTime;
                }

                if (Input.GetKeyDown(KeyCode.R) && resetCounter <= 0)
                {
                    ResetToTrack();
                }

            }
            else
            {
                /// <AI Speed Control and Driving Direction>
                //  We always keep our target point's height to be aligned to the height of the cars
                targetPoint.y = transform.position.y;

                if (Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
                {
                    SetNextAITarget();
                }

                //  target's (x,y,z) - the car's (x,y,z) = a directional vector
                Vector3 targetDirection = targetPoint - transform.position;
                //  angle is the degree angle from the target point to true front of the car
                float angle = Vector3.Angle(targetDirection, transform.forward);

                //  localPosition checks if the target point is on the left or on the right
                Vector3 localPosition = transform.InverseTransformPoint(targetPoint);
                if (localPosition.x < 0f)
                {
                    angle = -angle;
                }

                //  turn angle limitation to |1| at most
                turn = Mathf.Clamp(angle / aiMaxTurn,
                    -1f, // -> limited to 1
                    1f
                    );

                //  if target point's angle from true front of car does not exceed the maximum turn degree of AI
                if (Mathf.Abs(angle) < aiMaxTurn)
                {   //  We accelerate gradually by the preset acceleration rate
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
                }
                else
                {
                    //  We slow down gradually by the preset acceleration rate to the preset turning speed
                    aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
                }

                speed = aiSpeedMod * aiSpeedInput * forwardAccel;
            }


            UpdateSteering();
            UpdateDustTrail();
            UpdateEngineSFX();
            UpdateDriftingSFX();
            UpdateLapTimeDisplay();
        }
    }
  

    // update per delta time
    private void FixedUpdate()
    {
        FixedUpdateInclination();
        FixedUpdateCarMovement();
    }


    private void UpdateSpeedAndTurn()
    {
        float speedInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // update speed and turn
        speed = speedInput > 0
            ? speedInput * forwardAccel
            : speedInput * reverseAccel;
        turn = turnInput;
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
        // set engine sound according to speed of the car
        if (engineSFX != null)
        {
            // adjust constant at the back if needed
            engineSFX.pitch = 1f + (rigidBody.velocity.magnitude / maxSpeed) * 2f;
        }
    }


    private void UpdateDriftingSFX()
    {
        if (driftingSFX != null)
        {
            // drifting should only happen while car is on the ground
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

        if (isAI) 
        {
            if (cpNumber == currentTarget) {
                SetNextAITarget();
            }
        }
    }


    public void LapCompleted()
    {
        currentLap++;

        // update best lap time
        if (lapTime < bestLapTime || bestLapTime == 0f)
        {
            bestLapTime = lapTime;
        }

        // reset lap time to 0 for a new lap
        lapTime = 0f;
        if (!isAI)
        {
            // display best lap time
            var ts = System.TimeSpan.FromSeconds(bestLapTime);
            UIManager.instance.bestLapTimeText.text =
                string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);

            // display updated lap count
            UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        }
    }

    public void RandomiseAITarget() 
    {
        targetPoint += new Vector3(
            Random.Range(-aiPointVariance, aiPointVariance), 
            0,
            Random.Range(-aiPointVariance, aiPointVariance) 
            );
    }

    public void SetNextAITarget() 
    {
        currentTarget++;

        //  Reset to 0 once we finish
        if (currentTarget >= RaceManager.instance.allCheckpoints.Length)
        {
            currentTarget = 0;
        }

        //  referencing to the centre of checkpoint
        targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
        //  allow the target point for each checkpoint to be different for all cars
        RandomiseAITarget();
    }

    void ResetToTrack() 
    {
        //  set to previous checkpoint
        int pointToGoTo = nextCheckpoint - 1;
        if (pointToGoTo < 0) {
            pointToGoTo = RaceManager.instance.allCheckpoints.Length - 1;
        }

        transform.position = RaceManager.instance.allCheckpoints[pointToGoTo].transform.position;
        rigidBody.transform.position = transform.position;
        rigidBody.velocity = Vector3.zero;

        speed = 0f;
        turn = 0f;

        resetCounter = resetCoolDown;
    }

}
