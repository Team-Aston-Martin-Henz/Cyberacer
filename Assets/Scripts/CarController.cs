using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // this is the reference to the Rigidbody component of "Sphere"
    public Rigidbody theRB;

    // variable: maximum speed that our player car has
    public float maxSpeed;

    //  Acceleration physics
    public float forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;

    //  Turning physics
    public float turnStrength = 180f;
    private float turnInput;

    //  gravity physics adjustment
    private bool grounded;
    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundrayLength = .75f;

    private float dragOnGround;
    public float gravityMod = 10f;

    //  Turning effect for wheels
    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    //  particle emissions rate for 4 wheels
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;

    //  Sound Effect
    public AudioSource engineSound, driftingSound;
    public float driftingFadeSpeed;

    //  Lap Record and Checkpoint Record
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

    //  Update is called once per frame
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

        //  Turning the wheels, with reference to the (Parent Object) Player object's geometric alignment
        leftFrontWheel.localRotation = Quaternion.Euler
            (
            leftFrontWheel.localRotation.eulerAngles.x,     //  no modification on x axis
            (turnInput * maxWheelTurn) - 180,               //  offset 180 degree
            leftFrontWheel.localRotation.eulerAngles.z      //  no modification on y axis
            );
        rightFrontWheel.localRotation = Quaternion.Euler
            (
            rightFrontWheel.localRotation.eulerAngles.x,    //  no modification on x axis
            (turnInput * maxWheelTurn),                     //  no offset needed
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
            emissionRate = maxEmission;
        }

        if (theRB.velocity.magnitude <= 0.5f)
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
                driftingSound.volume = 0;
            }
        }
    }

    private void fixedUpdateInclination() 
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        //  if the car's front wheels hit something in front...
        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundrayLength, whatIsGround))
        {
            grounded = true;

            //  setting Y-axis of the car to be aligned with that of the Ground Ray Check
            normalTarget = hit.normal;
        }

        //  if the car's rear wheels hit something in front...
        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundrayLength, whatIsGround))
        {
            grounded = true;

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
            //  aero-drag
            theRB.drag = .1f;

            //  intensify gravity so that car can fall down more naturally
            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }

        //  -> setting limit on maxSpeed of the vehicle
        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }

        //  Debug.Log(theRB.velocity.magnitude);

        transform.position = theRB.position;

        if (grounded && Input.GetAxis("Vertical") != 0)
        {
            //  Time.deltaTime is used such that the combination of turnStrength in different framerate is consistent
            //  Mathf.Sign(speedInput) -> +ve is our speedInput is postive, else -ve -> allows steering to be intuitive to control
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles
                + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }
    }

    //  CheckpointHit is a function that updates the Lap and Checkpoint
    //  record when the car hit the checkpoints
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
