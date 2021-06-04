using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // reference to Rigidbody sphere
    public Rigidbody rigidBody;

    // variable: maximum speed that our player car has
    public float maxSpeed;

    //  Acceleration physics
    public float forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;

    //  Turning physics
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
    public AudioSource engineSound, driftingSound;
    public float driftingFadeRate;

    // checkpoint related variables
    private int nextCheckpoint;
    public int currentLap;

    //  Lap Time Record
    public float lapTime, bestLapTime;

    //  Start is called before the first frame update
    void Start()
    {
        //  Immediately from the start, set the "Sphere" to has no parent
        theRB.transform.parent = null;

        //  setting the drag on "ground" to be equal to that of the "Sphere" -> for value storage purpose
        dragOnGround = theRB.drag;

        //  This sets the start count to 1 and the total lap to the supposed value for the lapDisplay
        UIManager.instance.LapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }


    void Update()
    {
        updateLapTimeDisplay();
        updateAcceleration();
        updateSteering();
        updateDustEmission();
        updateEngineSFX();
        updateDriftingSFX();
    }

    //  Update without reference to framerate
    private void FixedUpdate()
    {
        fixedUpdateInclination();
        fixedUpdateCarMovement();
    }


    //  CH6: L36
    private void updateLapTimeDisplay()
    {
        //  LapTime is incremented as according to the framerate timing
        lapTime += Time.deltaTime;
        //  Conversion from seconds into Time Format
        var ts = System.TimeSpan.FromSeconds(lapTime);
        //  String display of Time
        UIManager.instance.currentLapTimeText.text = string.Format("{0:00}M{1:00}.{2:000}S", ts.Minutes, ts.Seconds, ts.Milliseconds);
    }

    private void updateAcceleration()
    {
        //  vertical movement control
        speedInput = 0f;
        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }
    }

    private void updateSteering()
    {
        //  horizontal movement control
        //  only allows horizontal direction of the car to change if the car is on the ground and there is a forward speed
        turnInput = Input.GetAxis("Horizontal");

        /*  Current segment of code migrated to FixedUpdate()
        if (grounded && Input.GetAxis("Vertical") != 0)
        {
            //  Time.deltaTime is used such that the combination of turnStrength in different framerate is consistent
            //  Mathf.Sign(speedInput) -> +ve is our speedInput is postive, else -ve -> allows steering to be intuitive to control
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles
                + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }
        */

    private void UpdateWheels()
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

    private void updateDustEmission()
    {
        //  control emission of particles -> fade gradually
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        //  if car is on the ground and
        //  we're going to turn or
        //  accelerating and not being stopped -> then set the maximum emission of particles to max
        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0f)))
        {
            emissionRate = maxEmissionRate;
        }

        // set emission rate to 0 if speed is too slow
        if (v <= 0.5f)
        {
            emissionRate = 0;
        }

        //  for loop to set particle emission visual efffect set above to be the same for all 4 wheels
        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }
    }

    private void updateEngineSFX()
    {
        //  Setting the sound of the engine to adjust to the speed of the PlayerCar
        if (engineSound != null)
        {
            engineSound.pitch = 1f + (theRB.velocity.magnitude / maxSpeed) * 2f;
        }
    }

    private void updateDriftingSFX()
    {
        //  Setting the drifting sound of the car to adjust according to the current speed of the car
        if (driftingSound != null)
        {
            //  drifting sound should only exist on the ground
            if (grounded)
            {
                if (Mathf.Abs(turnInput) > .5f)
                {
                    driftingSound.volume = 1f;
                }
                else
                {
                    driftingSound.volume = Mathf.MoveTowards(driftingSound.volume, 0f, driftingFadeSpeed * Time.deltaTime);
                }
            }
            else
            {
                driftingSound.volume = Mathf.MoveTowards(driftingSound.volume, 0f, driftingFadeRate * Time.deltaTime);
            }
        }
    }

    private void fixedUpdateInclination()
    {
        grounded = false;

    private void UpdateInclination()
    {
        isGrounded = false;
        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        //  if the car's front wheels hit something in front...
        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundrayLength, whatIsGround))
        {
            isGrounded = true;
            normalTarget = hit.normal;
        }

        //  if the car's rear wheels hit something in front...
        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundrayLength, whatIsGround))
        {
            isGrounded = true;
            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        //  Rotation of car to match the geometric of the ground surface
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }
    }

    private void fixedUpdateCarMovement()
    {
        //  Acceleration of the car only allowed if we are on the ground
        if (grounded)
        {
            //  the "Sphere"'s drag to be equal to that of the stored dragOnGround value
            theRB.drag = dragOnGround;

            //  -> provide a force of 100f unit on "Sphere" in accordance to timestep
            //  -> and always in forward direction
            theRB.AddForce(transform.forward * speedInput * 1000f);
        }
        else
        {
            rigidBody.drag = .1f;
            rigidBody.AddForce(Vector3.up * gravity * 100f);
        }

        //  -> setting limit on maxSpeed of the vehicle
        if (theRB.velocity.magnitude > maxSpeed)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }

        // adjust car's orientation
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

    //  CheckpointHit is a function that updates the Lap and Checkpoint
    //  record when the car hit the checkpoints
    public void CheckpointHit(int cpNumber)
    {
        if (cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            if (nextCheckpoint == RaceManager.instance.allCheckPoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
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
