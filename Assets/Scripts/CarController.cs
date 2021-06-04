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

    //  Lap Record and Checkpoint Record
    private int nextCheckpoint;
    public int currentLap;



    //  Lap Time Record
    public float lapTime, bestLapTime;








    //  Start is called before the first frame update
    void Start()
    {
        // separate rigidbody from the car at the start
        rigidBody.transform.parent = null;
        // setting the drag on ground to be equal to that of the sphere
        dragOnGround = rigidBody.drag;



        //  This sets the start count to 1 and the total lap to the supposed value for the lapDisplay
        UIManager.instance.LapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeedAndTurn();
        UpdateLapTimeDisplay();
        UpdateSteering();
        UpdateDustTrail();
        UpdateEngineSFX();
        UpdateDriftingSFX();
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



    private void UpdateLapTimeDisplay()
    {
        //  LapTime is incremented as according to the framerate timing
        lapTime += Time.deltaTime;
        //  Conversion from seconds into Time Format
        var ts = System.TimeSpan.FromSeconds(lapTime);
        //  String display of Time
        UIManager.instance.currentLapTimeText.text = string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);
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
                currentLap++;
            }
        }
    }


    //  LapCompleted is a function that updates the bestLapTime and resets the lapTime to 0 for a new lap
    public void LapCompleted()
    {
        //  Update Lap Count
        currentLap++;

        //  If we have a better timing or we don't previously have a lap time -> we update it
        if (lapTime < bestLapTime || bestLapTime == 0f)
        {
            bestLapTime = lapTime;
        }

        //  Reset lapTime to 0 for a new lap
        lapTime = 0f;

        //  Display for best time information
        var ts = System.TimeSpan.FromSeconds(bestLapTime);
        UIManager.instance.bestLapTimeText.text = string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);

        UIManager.instance.LapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }
}
