
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterController : MonoBehaviour
{
    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    [SerializeField]
    private float sensMultiplier = 1f;

    private int groundLayer;
    private float threshold = 0.01f;
    [SerializeField]
    private float maxSpeed = 20, moveSpeed = 10000, counterMovement = 0.175f, airCounterMovement = 0.08f, maxSlopeAngle = 35f;
    private float currentCounterMovement;

    private bool canMove = true;

    public bool grounded;
    public LayerMask whatIsGround;
    
    private Rigidbody rb;

    [SerializeField]
    private Transform playerCam;

    private Vector3 normalVector = Vector3.up, wallNormal;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    [SerializeField]
    private float jumpForce = 550f;

    //Wal climbing
    private bool isOnWall, canWallClimb;
    [SerializeField]
    private float wallClimbForce = 500f, wallClimbDeceleration = 2;
    private float currentWallClimbForce;


    //Input
    private float xInput, yInput, mouseXRot, mouseYRot;
    private bool jumping;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        groundLayer = LayerMask.GetMask("Ground");
    }
    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        MyInput();
        Look();
        FindVelRelativeToLook();
    }

    void MyInput()
    {
        //Movement
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        //Jump
        jumping = Input.GetButtonDown("Jump");
        
    }

    private float desiredX;
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        Vector3 rot = new Vector3(0, mouseX, 0);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rot));        
    }

    void Movement() 
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(xInput, yInput, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump(); 

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //Wall climbing
        WallClimb();

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (xInput > 0 && xMag > maxSpeed) xInput = 0;
        if (xInput < 0 && xMag < -maxSpeed) xInput = 0;
        if (yInput > 0 && yMag > maxSpeed) yInput = 0;
        if (yInput < 0 && yMag < -maxSpeed) yInput = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
            currentCounterMovement = airCounterMovement;
        }
        else
            currentCounterMovement = counterMovement;


        //Apply forces to move player
        if (canMove)
        {
            rb.AddForce(transform.forward * yInput * moveSpeed * Time.deltaTime * multiplier * multiplierV);
            rb.AddForce(transform.right * xInput * moveSpeed * Time.deltaTime * multiplier);
        }
    }
    private void WallClimb()
    {
        if (isOnWall && !grounded)
        {
            if (canWallClimb) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            float currentYVelocity = rb.velocity.y;
            rb.AddForce(transform.up * currentWallClimbForce);
            if (currentWallClimbForce >= 0)
                currentWallClimbForce -= wallClimbDeceleration;
            canWallClimb = false;
        }
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * transform.right * Time.deltaTime * -mag.x * currentCounterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * transform.forward * Time.deltaTime * -mag.y * currentCounterMovement);
        }

        //Limit diagonal running.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private bool cancellingGrounded, cancellingOnWall;
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;

                canWallClimb = true;
                currentWallClimbForce = wallClimbForce;

                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
            if (isWall(normal))
            {
                isOnWall = true;
                cancellingOnWall = false;
                wallNormal = normal;
                CancelInvoke(nameof(StopIsOnWall));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
        if (!cancellingOnWall)
        {
            cancellingOnWall = true;
            Invoke(nameof(StopIsOnWall), Time.deltaTime * delay);
        }
    }
    private bool IsFloor(Vector3 normalV)
    {
        float angle = Vector3.Angle(Vector3.up, normalV);
        return angle < maxSlopeAngle;
    }
    private bool isWall(Vector3 normalV)
    {
        float wallAngle = 90;
        float angle = Vector3.Angle(Vector3.up, normalV);
        return angle < wallAngle + 1 && angle > wallAngle - 1;
    }

    private void StopGrounded()
    {
        grounded = false;
    }
    private void StopIsOnWall()
    {
        isOnWall = false;
    }
}