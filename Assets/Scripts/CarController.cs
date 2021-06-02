using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // reference to Rigidbody sphere
    public Rigidbody rb;

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
    public AudioSource engineSound, driftingSound;
    public float driftingFadeSpeed;


    void Start()
    {
        // separate rigidbody from the car at the start
        rb.transform.parent = null;
        
        // setting the drag on "ground" to be equal to that of the "Sphere" -> for value storage purpose
        dragOnGround = rb.drag;
    }


    void Update()
    {
        float speedInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // update speed and turn
        speed = speedInput > 0
            ? speedInput * forwardAccel
            : speedInput * reverseAccel;
        turn = turnInput;

        UpdateWheels();
        UpdateDustTrail();


        //  Setting the sound of the engine to adjust to the speed of the PlayerCar
        if (engineSound != null) 
        {
            engineSound.pitch = 1f + (rb.velocity.magnitude / maxSpeed) * 2f; 
        }

        if (driftingSound != null)
        {   
            //  drifting sound should only exist on the ground
            if (isGrounded)
            {
                if (Mathf.Abs(turn) > .5f)
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


    private void FixedUpdate()
    {
        UpdateInclination();

        //  Acceleration of the car only allowed if we are on the ground
        if (isGrounded) 
        {
            //  the "Sphere"'s drag to be equal to that of the stored dragOnGround value
            rb.drag = dragOnGround;
            
            //  -> provide a force of 100f unit on "Sphere" in accordance to timestep
            //  -> and always in forward direction
            rb.AddForce(transform.forward * speed * 1000f);
        } else
        {
            //  aero-drag
            rb.drag = .1f;

            //  intensify gravity so that car can fall down more naturally
            rb.AddForce(Vector3.up * gravity * 100f);
        }

        //  -> setting limit on maxSpeed of the vehicle
        if (rb.velocity.magnitude > maxSpeed) 
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        // Debug.Log(rb.velocity.magnitude);

        transform.position = rb.position;


        // left & right movement
        if (isGrounded && speed != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(
                    0f,
                    turn * turnStrength * Time.deltaTime * Mathf.Sign(speed) * (rb.velocity.magnitude / maxSpeed),
                    0f)
                );
        }
    }
    

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


    private void UpdateDustTrail()
    {
        float v = rb.velocity.magnitude;

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


    private void UpdateInclination()
    {
        isGrounded = false;
        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        // check if front wheel on ramp
        if (Physics.Raycast(groundRayPoint1.position, -transform.up, out hit, groundrayLength, groundLayer)) 
        {
            isGrounded = true;
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
}