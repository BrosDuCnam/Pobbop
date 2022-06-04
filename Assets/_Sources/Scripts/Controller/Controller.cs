using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Controller : NetworkBehaviour
{

    public Rigidbody rb;

    private Vector2 axis;
    private Vector3 dir = Vector3.zero;
    private Vector3 groundNormal = Vector3.up;
    
    //Movement
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airSpeed = 3f;
    [SerializeField] private float airAcceleration = 5f;
    [SerializeField] private float jumpUpSpeed = 5f;
    [SerializeField] private float maxGroundAngle = 40f;
    [SerializeField] [Range(1, 2)] private float speedFactToEnterSliding = 1.3f;
    [SerializeField] [Range(0, 1)] private float verticalSpeedSlideMultiplier = 0.4f;
    [SerializeField] private float slideAccelAngle = 10f; //Defines on what ground angle the player can slide
    [SerializeField] private float slideBoost = 11f; 
    [SerializeField] [Tooltip("Used to avoid crouch spam")] private float minSlidePause = 0.5f;
    [SerializeField] [Range(0, 1)] private float slideDeceleration = 0.1f;
    [SerializeField] private float crouchScale = 0.3f;
    [SerializeField] [Range(0, 1)] private float ballChargeSpeedNerf = 0.3f;
    [SerializeField] private float sprintMaxAngle = 50;
    [SerializeField] [Range(0, 1)] private float jumpNerfFactor = 1.5f;
    [SerializeField] private float jumpNerfResetTime = 0.5f;
    [SerializeField] [Range(0, 180)] private float lurchMaxAngle = 20;
    [SerializeField] [Range(0, 1)] private float crouchColliderHeightMultiplier = 0.3f;
    [SerializeField] private float rayLength = 0.5f;
    [SerializeField] private float groundStickForce = 2f;
    
    
    [Header("Camera Settings")]
    [SerializeField] public Camera camera;
    [SerializeField] private float sensX = 0.1f;
    [SerializeField] private float sensY = 0.1f;
    public Animator _TpsAnimator;
    public Animator _PovAnimator;
    [SerializeField] private float camCrouchOffset = 0.8f;
    [SerializeField] private float camCrouchSpeed = 5;
    
    
    //Bools
    [field: SerializeField] public bool isGrounded { get; protected set; }
    [field: SerializeField] public bool run { get; protected set; }
    [field: SerializeField] public bool jump { get; protected set; }
    [field: SerializeField] public bool crouch { get; protected set; }
    [field: SerializeField] public bool sliding { get; protected set; }
    [field: SerializeField] public bool enterSliding { get; protected set; } = true;
    
    [Header("Debug")] 
    [SerializeField] private bool drawGUI;
    [SerializeField] private int fontSize;


    protected UnityEvent<Vector2> onAxis = new UnityEvent<Vector2>();
    protected UnityEvent onRunStart = new UnityEvent();
    protected UnityEvent<bool> onJump = new UnityEvent<bool>();
    protected UnityEvent<bool> onCrouch = new UnityEvent<bool>();
    protected UnityEvent<Vector2> onDirectionAxis = new UnityEvent<Vector2>();
    protected UnityEvent<bool> onLurch = new UnityEvent<bool>();

    private List<float> yVelBuffer = new List<float>();

    private float walkSpeed;
    private float crouchSpeed;
    private float lastJump;
    private bool _isThrowing;
    private bool camCrouch;
    private bool cancelThrow;

    protected bool lurch;
    private float slideNerf;

    private Player player;

    //Cam
    private Vector2 camAxis;
    private float camInitialLocalY;

    private CapsuleCollider collider;
    private float initialColHeight;
    private Vector3 initialColCenter;
    
    [HideInInspector] public Vector2 currentLook;
    
    //Animator
    private float velX;
    private float velY;

    private bool animCrouch;
    private bool animJump;
        
    
    protected void Awake()
    {
        onAxis.AddListener(Axis);
        onRunStart.AddListener(OnRun);
        onJump.AddListener(OnJump);
        onCrouch.AddListener(OnCrouch);
        onDirectionAxis.AddListener(OnDirection);
        onLurch.AddListener(OnLurch);

        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        walkSpeed = _walkSpeed;
        crouchSpeed = _crouchSpeed;
        camInitialLocalY = camera.transform.localPosition.y;
        collider = gameObject.GetComponent<CapsuleCollider>();
        initialColHeight = collider.height;
        initialColCenter = collider.center;

        isGrounded = true;
    }
    
    protected void Update()
    {
        CalculateCam();
        UpdateTPSAnimator();
        UpdatePOVAnimator();
        CamCrouch();
        if (Time.time > slideNerf && slideNerf != 0) enterSliding = true;
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
        }
        RotateCam();
        //Register vertical velocity to adapt the slide when finishing a jump (or fall)
        RegisterFloatBuffer(yVelBuffer, rb.velocity.y, 3);
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
        animCrouch = true;
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        float speedForSliding = speed * Mathf.Clamp(Mathf.Abs(GetFloatBuffValue(yVelBuffer) * verticalSpeedSlideMultiplier), 1, Mathf.Infinity);
        slideNerf = 0;  //Reset the slide nerf
        //Check speed to know if enter sliding
        if (speedForSliding > _walkSpeed * speedFactToEnterSliding)
        {
            //Give speed boost on slide enter
            if (enterSliding && speed < slideBoost)
            {
                //Add boost based on the flat velocity and the wishdir
                Vector3 vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                Vector3 groundOnSlope = Vector3OnSlope(vel.normalized + wishDir.normalized * 0.5f).normalized;
                Vector3 slideForce = groundOnSlope * Mathf.Clamp(slideBoost - speed, 0, Mathf.Infinity);
                //Adapt slide boost speed base on the angle we want to slide along
                float groundAngle = (Vector3.Angle(Vector3.up, groundOnSlope) - 90) / 75;
                slideForce *= 1 + groundAngle;
                rb.AddForce(slideForce, ForceMode.VelocityChange); 
                enterSliding = false;
            }
            sliding = true;
        }
        else
        {
            Walk(wishDir, maxSpeed, acceleration);
            sliding = false;
        }
        
        if (sliding)
        {
            //Slow down
            if (Vector3.Angle(Vector3.up, groundNormal) < slideAccelAngle)
            {
                rb.velocity -= Vector3.Lerp(Vector3.zero, rb.velocity, Time.fixedDeltaTime * slideDeceleration); 
                rb.AddForce(wishDir.normalized * 1, ForceMode.Acceleration);
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

    private void CamCrouch()
    {
        if (animCrouch)
        {
            if (camera.transform.localPosition.y > camInitialLocalY - camCrouchOffset)
            {
                camera.transform.position += Vector3.down * camCrouchSpeed * Time.deltaTime;
                collider.height = initialColHeight * crouchColliderHeightMultiplier;
                collider.center = new Vector3(initialColCenter.x, initialColCenter.y * crouchColliderHeightMultiplier, initialColCenter.z);
            }
        }
        else
        {
            if (camera.transform.localPosition.y < camInitialLocalY)
            {
                camera.transform.position -= Vector3.down * camCrouchSpeed * Time.deltaTime;
                collider.height = initialColHeight;
                collider.center = initialColCenter;
            }
        }
    }

    /// <summary>
    /// Permits light strafe but not sharp turn
    /// </summary>
    /// <param name="wishDir"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="acceleration"></param>
    private void AirStrafe(Vector3 wishDir, float maxSpeed, float acceleration)
    {
        
        float projVel = Vector3.Dot(new Vector3(rb.velocity.x, 0f, rb.velocity.z), wishDir);
        float accelVel = acceleration * Time.deltaTime;

        if (projVel + accelVel > maxSpeed)
            accelVel = Mathf.Max(0, maxSpeed - projVel);
        
        rb.AddForce(wishDir.normalized * accelVel, ForceMode.VelocityChange);

        if (lurch)
        {
            float currentVel = rb.velocity.magnitude;
            float yVel = rb.velocity.y;
            Vector3 newVel = Vector3.RotateTowards(rb.velocity.normalized, wishDir.normalized, 
                Mathf.Deg2Rad * lurchMaxAngle, 1) * (currentVel -0.3f);
            rb.velocity = new Vector3(newVel.x, yVel, newVel.z);
            lurch = false;
        }
    }
    
    
    /// <summary>
    /// Jump up
    /// </summary>
    private void Jump()
    {
        animJump = true;
        //If the player recently jumped, nerf the jump height
        float upForce = Mathf.Clamp( (Time.time < lastJump + jumpNerfResetTime ?
                                         jumpUpSpeed * jumpNerfFactor : jumpUpSpeed) 
                                     - rb.velocity.y, 0, Mathf.Infinity);
        rb.AddForce(new Vector3(0, upForce, 0), ForceMode.VelocityChange);
        lastJump = Time.time;
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
    
    
    public bool IsThrowing
    {
        get => _isThrowing;
        set
        {
            _isThrowing = value;
            NerfSpeedOnCharge(_isThrowing);
        }
    }

    /// <summary>
    /// Used to slow the player if he charges the ball
    /// </summary>
    /// <param name="state"></param>
    private void NerfSpeedOnCharge(bool state)
    {
        walkSpeed = state
            ? _walkSpeed * ballChargeSpeedNerf
            : walkSpeed = _walkSpeed;
        crouchSpeed = state
            ? _crouchSpeed * ballChargeSpeedNerf
            : crouchSpeed = _crouchSpeed;
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

    private bool IsGroundNear()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, Vector3.down * rayLength);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.CompareTag("Player") && !hit.collider.transform.CompareTag("Ball"))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator StickToGround()
    {
        while (!isGrounded && !jump && !animJump)
        {
            if (IsGroundNear())
            {
                rb.AddForce(Vector3.down * groundStickForce, ForceMode.Acceleration);
            }
            yield return null;
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
                    
                    if (slideNerf == 0 && crouch == false)
                        slideNerf = Time.time + minSlidePause;
                    
                    if (Time.time > lastJump + 0.1f) animJump = false;
                }
            }
        }
    }
    private void OnCollisionExit(Collision col)
    {
        isGrounded = false;
        StartCoroutine(StickToGround());
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
    
    
    #region Animator

    private void UpdateTPSAnimator()
    {
        if (_TpsAnimator != null)
        {
            velX = Vector3.Dot(transform.right, rb.velocity) / _walkSpeed;
            velY = Vector3.Dot(transform.forward, rb.velocity) / _walkSpeed;
            //set the velocity to 0 if it's 0.0001...
            velX = Mathf.Round(velX * 1000) / 1000;
            velY = Mathf.Round(velY * 1000) / 1000;
            
            _TpsAnimator.SetFloat("velX", velX);
            _TpsAnimator.SetFloat("velY", velY);
            _TpsAnimator.SetBool("slide", sliding);
            _TpsAnimator.SetBool("crouch", !sliding && animCrouch);
            _TpsAnimator.SetBool("isgrounded", isGrounded);
            _TpsAnimator.SetBool("jumping", animJump);
            _TpsAnimator.SetLayerWeight(1, _isThrowing ? 1 : 0);
        }
    }    
    private void UpdatePOVAnimator()
    {
        if (_TpsAnimator != null)
        {
            _PovAnimator.SetBool("grounded", isGrounded);
            _PovAnimator.SetBool("sliding", sliding);
            _PovAnimator.SetLayerWeight(1, player.IsHoldingObject ? 1 : 0);
            _PovAnimator.SetFloat("speed", sliding ? 0 : rb.velocity.magnitude);
            _PovAnimator.SetFloat("normalizedSpeed",  sliding || !isGrounded ? 0 : rb.velocity.magnitude / runSpeed);
            _PovAnimator.SetLayerWeight(2, player.IsCharging ? 1 : 0);
            _PovAnimator.SetBool("charging", player.IsCharging);
            _PovAnimator.SetBool("cancelThrow", cancelThrow);
            cancelThrow = false;
        }
    }

    #endregion

    
    #region Inputs
    public void Axis(Vector2 axis)
    {
        this.axis = axis;
        if (this.axis.magnitude == 0) run = false;
    }

    public void OnLurch(bool started)
    {
        lurch = started;
    }

    public void OnRun()
    {
        run = true;
        if (run)
        {
            player._throw.CancelThrow();
            cancelThrow = true;
            walkSpeed = _walkSpeed;
        }
    }

    public void OnJump(bool pressed)
    {
        if (isGrounded && !jump)
            jump = pressed;
        if (!pressed)
            jump = false;
        else
            lurch = false;
    }
    
    public void OnCrouch(bool pressed)
    {
        crouch = pressed;
        if (!crouch)
        {
            sliding = false;
            animCrouch = false;
        }
    }

    public void OnDirection(Vector2 axis)
    {
        camAxis = axis;
    }
    #endregion
        
    //Debug : Speed infos
    void OnGUI()
    {
        if (drawGUI)
        {
            GUI.color = Color.red;
            GUIStyle style = new GUIStyle();
            style.fontSize = fontSize;
            GUILayout.Label("speed: " + new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude, style);
            GUILayout.Label("speedUp: " + rb.velocity.y, style);
            GUILayout.Label("yVle: " + Mathf.Clamp(Mathf.Abs(GetFloatBuffValue(yVelBuffer) * 0.35f), 1, Mathf.Infinity), style);
            GUILayout.Label("axis : " + axis, style );
            GUILayout.Label("VelX : " + velX, style );
            GUILayout.Label("VelY : " + velY, style );
            GUILayout.Label("slide : " + sliding, style );
            GUILayout.Label("crouch : " + (!sliding && crouch), style );
            GUILayout.Label("jump : " + animJump, style );
            GUILayout.Label("Cancel Throw : " + cancelThrow, style );
            GUILayout.Label("Charging : " + player.IsCharging, style );
        }
    }
}
