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
    public float driftingFadeRate;


    void Start()
    {
        // separate rigidbody from the car at the start
        rb.transform.parent = null;
        
        // setting the drag on "ground" to be equal to that of the "Sphere" -> for value storage purpose
        dragOnGround = rb.drag;
    }


    void Update()
    {
        GetSpeedAndTurn();
        UpdateWheels();
        UpdateDustTrail();
        UpdateSound();
    }


    private void FixedUpdate()
    {
        UpdateInclination();
        UpdateCarPosition();
    }
    

    private void GetSpeedAndTurn()
    {
        float speedInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // update speed and turn
        speed = speedInput > 0
            ? speedInput * forwardAccel
            : speedInput * reverseAccel;
        turn = turnInput;
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


    private void UpdateSound()
    {
        // set engine sound according to speed of the car
        if (engineSound != null) 
        {
            engineSound.pitch = 1f + (rb.velocity.magnitude / maxSpeed) * 2f; // adjust if needed
        }

        if (driftingSound != null)
        {   
            // drifting should only happen while car is on the ground
            if (!isGrounded) return;

            if (Mathf.Abs(turn) > .5f)
            {
                driftingSound.volume = 1f;
            } else
            {
                driftingSound.volume = Mathf.MoveTowards(driftingSound.volume, 0f, driftingFadeRate * Time.deltaTime);
            }
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


    private void UpdateCarPosition()
    {
        if (isGrounded)
        // car on ground, accelerate towards the forward direction
        {
            // set sphere's drag to dragOnGround value
            rb.drag = dragOnGround;
            rb.AddForce(transform.forward * speed * 1000f);
        } else
        // car in the air, accelerate downwards due to gravity
        {
            rb.drag = .1f;
            rb.AddForce(Vector3.up * gravity * 100f);
        }

        // limit car speed to be below max speed
        if (rb.velocity.magnitude > maxSpeed) 
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        // adjust car's orientation
        if (isGrounded && speed != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(
                    0f,
                    turn * turnStrength * Time.deltaTime * Mathf.Sign(speed) * (rb.velocity.magnitude / maxSpeed),
                    0f)
                );
        }

        // realign car's position to sphere
        transform.position = rb.position;
    }
}