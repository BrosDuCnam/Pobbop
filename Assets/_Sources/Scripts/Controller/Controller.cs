using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Controller : NetworkBehaviour
{

    protected Rigidbody rb;

    private Vector2 axis;
    private Vector3 dir = Vector3.zero;
    private Vector3 groundNormal = Vector3.up;
    
    //Movement
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airSpeed = 3f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float jumpUpSpeed = 5f;
    [SerializeField] private float maxGroundAngle = 40f;
    [SerializeField] [Range(1, 2)] private float speedFactToEnterSliding = 1.3f;
    [SerializeField] [Range(0, 1)] private float verticalSpeedSlideMultiplier = 0.4f;
    [SerializeField] private float slideAccelAngle = 10f; //Defines above what ground angle the player can slide
    [SerializeField] private float slideBoost = 11f;
    [SerializeField] [Tooltip("Used to avoid crouch spam")] private float minSlidePause = 1f;
    [SerializeField] [Range(0, 1)] private float slideDeceleration = 0.1f;
    [SerializeField] private float crouchScale = 0.3f;
    [SerializeField] [Range(0, 1)] private float ballChargeSpeedNerf = 0.3f;
    [SerializeField] private float sprintMaxAngle = 50;
    
    [Header("Camera Settings")]
    [SerializeField] public Camera camera;
    [SerializeField] private float sensX = 0.1f;
    [SerializeField] private float sensY = 0.1f;
    [SerializeField] protected Animator _animator;

    //Bools
    [field: SerializeField] public bool isGrounded { get; protected set; }
    [field: SerializeField] public bool run { get; protected set; }
    [field: SerializeField] public bool jump { get; protected set; }
    [field: SerializeField] public bool crouch { get; protected set; }
    [field: SerializeField] public bool sliding { get; protected set; }
    [field: SerializeField] public bool enterSliding { get; protected set; } = true;



    protected UnityEvent<Vector2> onAxis = new UnityEvent<Vector2>();
    protected UnityEvent onRunStart = new UnityEvent();
    protected UnityEvent<bool> onJump = new UnityEvent<bool>();
    protected UnityEvent<bool> onCrouch = new UnityEvent<bool>();
    protected UnityEvent<Vector2> onDirectionAxis = new UnityEvent<Vector2>();

    private float runInputTime;
    private List<float> yVelBuffer = new List<float>();

    private float lastBoost;
    private float walkSpeed;

    private BasePlayer player;

    
    //Cam
    private Vector2 camAxis;

    protected Vector2 currentLook;
    
    //Animator
    private float velX;
    private float velY;


    protected void Awake()
    {
        onAxis.AddListener(Axis);
        onRunStart.AddListener(OnRun);
        onJump.AddListener(OnJump);
        onCrouch.AddListener(OnCrouch);
        onDirectionAxis.AddListener(OnDirection);

        player = GetComponent<BasePlayer>();
        rb = GetComponent<Rigidbody>();
        walkSpeed = _walkSpeed;
    }
    
    protected void Update()
    {
        if (!crouch && isGrounded && Time.time > lastBoost + minSlidePause)
            enterSliding = true;
        CalculateCam();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        dir = Direction();
        
        if (isGrounded)
        {
            if (crouch)
                Crouch(dir, crouchSpeed, groundAcceleration);
            else
            {
                //Calculates if the player walks or run based on his inputs and direction
                Walk(dir, run && Vector3.Angle(dir, transform.forward) < sprintMaxAngle ? 
                    runSpeed : walkSpeed, groundAcceleration);
            }
        }
        else
        {
            AirStrafe(dir, airSpeed, airAcceleration);
            //Register vertical velocity to adapt the slide when finishing a jump (or fall)
            RegisterFloatBuffer(yVelBuffer, rb.velocity.y, 3);
        }
        RotateCam();
    }
    

    #region Movement
    /// <summary>
    ///  Walk towards the given direction with the speed parameters given
    /// </summary>
    /// <param name="wishDir"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="acceleration"></param>
    private void Walk(Vector3 wishDir, float maxSpeed, float acceleration)
    {
        if (jump)
        {
            Jump();
        }
        
        // Calcucates the speed to apply to the given direction
        wishDir = wishDir.normalized;
        Vector3 speed = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (speed.magnitude > maxSpeed)
            acceleration *= speed.magnitude / maxSpeed;
        Vector3 direction = wishDir * maxSpeed - speed;
        
        // Move faster when the player starts moving from 0 velocity 
        if (direction.magnitude < 0.5f)
            acceleration *= direction.magnitude * 2;

        direction = direction.normalized * acceleration;
        float magn = direction.magnitude;
        direction = direction.normalized;
        direction *= magn;
        
        // Let the player have the same velocity while climbing
        Vector3 slopeCorrection = groundNormal * Physics.gravity.y / groundNormal.y;
        slopeCorrection.y = 0f;
        direction += slopeCorrection;

        rb.AddForce(direction, ForceMode.Acceleration);

        jump = false;
    }

    private void Crouch(Vector3 wishDir, float maxSpeed, float acceleration)
    {
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        float speedForSliding = speed * Mathf.Clamp(Mathf.Abs(GetFloatBuffValue(yVelBuffer) * verticalSpeedSlideMultiplier), 1, Mathf.Infinity);
        //Check speed to know if enter sliding
        if (speedForSliding > _walkSpeed * speedFactToEnterSliding)
        {
            //Give speed boost on slide enter
            if (enterSliding && speed < slideBoost &&  Time.time > lastBoost + minSlidePause)
            {
                lastBoost = Time.time;
                //Add boost based on the flat velocity and the wishdir
                Vector3 vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                Vector3 groundOnSlope = Vector3OnSlope(vel.normalized + wishDir.normalized * 0.5f).normalized;
                Vector3 slideForce = groundOnSlope * Mathf.Clamp(slideBoost - speed, 0, Mathf.Infinity);
                //Adapt slide boost speed base on the angle we want to slide along
                float groundAngle = (Vector3.Angle(Vector3.up, groundOnSlope) - 90) / 75;
                slideForce *= 1 + groundAngle;
                Debug.Log(groundAngle);
                rb.AddForce(slideForce, ForceMode.VelocityChange); 
                enterSliding = false;
            }
            sliding = true;
        }
            
        else
            Walk(wishDir, maxSpeed, acceleration);

        // Not working : needs fix (doesn't know if going up or down)
        if (sliding)
        {
            //Slow down
            if (Vector3.Angle(Vector3.up, groundNormal) < slideAccelAngle)
            {
                rb.velocity -= Vector3.Lerp(Vector3.zero, rb.velocity, Time.fixedDeltaTime * slideDeceleration); 
                //Debug.Log("decelerate");
            }
            if (jump) Jump();

        }
        
        //Reset sliding
        if (speed < 0.5f)
        {
            sliding = false;
            enterSliding = true;
        }

        jump = false;
    }

    /// <summary>
    /// Permits light strafe but not sharp turn
    /// </summary>
    /// <param name="wishDir"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="acceleration"></param>
    private void AirStrafe(Vector3 wishDir , float maxSpeed, float acceleration)
    {
        float projVel = Vector3.Dot(new Vector3(rb.velocity.x, 0f, rb.velocity.z), wishDir);
        float accelVel = acceleration * Time.deltaTime;

        if (projVel + accelVel > maxSpeed)
            accelVel = Mathf.Max(0, maxSpeed - projVel);
        
        rb.AddForce(wishDir.normalized * accelVel, ForceMode.VelocityChange);
    }
    
    /// <summary>
    /// Jump up
    /// </summary>
    private void Jump()
    {
        float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);
        rb.AddForce(new Vector3(0, upForce, 0), ForceMode.VelocityChange);
    }


    #endregion

    #region Public Methods

    /// <summary>
    /// Use Punch to add an instant force to the player
    /// </summary>
    /// <param name="force"></param>
    public void Punch(Vector3 force)
    {
        print("Punch : " + force);
        rb.AddForce(force, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Used to slow the player if he charges the ball
    /// </summary>
    /// <param name="state"></param>
    public void NerfSpeedOnCharge(bool state)
    {
        walkSpeed = state
            ? _walkSpeed * ballChargeSpeedNerf
            : walkSpeed = _walkSpeed;
        run = false;
    }

    public void CrouchScale()
    {
        if (crouch)
        {
            gameObject.transform.localScale = new Vector3(1, crouchScale, 1);
        }
        else
        {
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    #endregion

    #region Maths

    /// <summary>
    /// Calculates the forward vector of the player along the ground
    /// </summary>
    /// <param name="forward"></param>
    /// <returns></returns>
    private Vector3 Vector3OnSlope(Vector3 forward)
    {
        Vector3 side = Vector3.Cross(forward, groundNormal).normalized;
        float rot = Vector3.Angle(forward, groundNormal) - 90;
        Vector3 vect = Quaternion.AngleAxis(rot, side) * forward;

        return vect;
    }
    
    /// <summary>
    /// Calculates the direction based on the inputs and the direction where the player looks
    /// </summary>
    /// <returns>Direction vector</returns>
    private Vector3 Direction()
    {
        float xAxis = axis.y;
        float yAxis = axis.x;
        
        //axis = Vector2.zero;

        Vector3 direction = new Vector3(yAxis, 0, xAxis);
        return rb.transform.TransformDirection(direction);
    }

    /// <summary>
    /// Register a float in the given buffer (list)
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    /// <param name="maxSize"></param>
    /// <example> Can be used if you want to know the speed you had n frames before</example>
    private void RegisterFloatBuffer(List<float> buffer, float value, float maxSize)
    {
        buffer.Add(value);
        if (buffer.Count >= maxSize)
            buffer.RemoveAt(0);
    }

    /// <summary>
    /// Returns oldest value of the buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private float GetFloatBuffValue(List<float> buffer)
    {
        if (buffer.Count != 0)
            return buffer[0];
        return 0;
    }

    #endregion

    #region Collisions

    private void OnCollisionStay(Collision col)
    {
        if (col.contactCount > 0)
        {
            float angle;

            foreach (ContactPoint contact in col.contacts)
            {
                angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle < maxGroundAngle)
                {
                    isGrounded = true;
                    groundNormal = contact.normal;
                }
            }
        }
    }
    private void OnCollisionExit(Collision col)
    {
        isGrounded = false;
    }

    #endregion

    #region Camera

    private void RotateCam()
    {
        camera.transform.localRotation = Quaternion.AngleAxis(-currentLook.y, Vector3.right);
        transform.localRotation = Quaternion.Euler(0, currentLook.x, 0);

    }
    
    private void CalculateCam()
    {
        
        camAxis = new Vector2(camAxis.x * sensX, camAxis.y * sensY);

        currentLook.x += camAxis.x;
        currentLook.y = Mathf.Clamp(currentLook.y += camAxis.y, -90, 90);

        currentLook.x = currentLook.x % 360;
        camAxis = Vector2.zero; // Reset axis
    }

    #endregion

    #region Private Methods

    private void UpdateAnimator()
    {
        if (_animator != null)
        {
            velX = Vector3.Dot(transform.right, rb.velocity) / runSpeed;
            velY = Vector3.Dot(transform.forward, rb.velocity) / runSpeed;
            _animator.SetFloat("velX", velX);
            _animator.SetFloat("velY", velY);
        }
    }

    #endregion
    
    #region Inputs
    public void Axis(Vector2 axis)
    {
        this.axis = axis;
        if (this.axis.magnitude == 0) run = false;
    }

    public void OnRun()
    {
        run = true;
        if (run)
        {
            runInputTime = Time.time;
            player.CancelCharge();
        }
    }

    public void OnJump(bool pressed)
    {
        if (isGrounded && !jump)
            jump = pressed;
        if (!pressed)
            jump = false;
    }
    
    public void OnCrouch(bool pressed)
    {
        crouch = pressed;
        CrouchScale();
    }

    public void OnDirection(Vector2 axis)
    {
        camAxis = axis;
    }
    #endregion
        
    //Debug : Speed infos
    void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label("speed: " + new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
        GUILayout.Label("speedUp: " + rb.velocity.y);
        GUILayout.Label("yVle: " + Mathf.Clamp(Mathf.Abs(GetFloatBuffValue(yVelBuffer) * 0.35f), 1, Mathf.Infinity));
        GUILayout.Label("axis : " + axis );
    }
}
