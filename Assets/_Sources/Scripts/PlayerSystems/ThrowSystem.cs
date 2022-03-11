using System;
using Mirror;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThrowSystem : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private BasePlayer _basePlayer;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Settings")]
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeCurve;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] public float minStepMultiplier = 2f;
    [SerializeField] public float maxStepMultiplier = 30f;
    [SerializeField] private bool _drawCurve = true;
    
    private float _startChargeTime;
    private bool _isCharging;

    public bool IsCharging
    {
        get => _isCharging;
        private set => _isCharging = value;
    }

    public float ChargeValue
    {
        get
        {
            if (!IsCharging) return 0;
            
            float chargeTime = Time.time - _startChargeTime;
            float chargeValue = chargeTime / _maxChargeTime;
            
            return Mathf.Clamp(chargeValue, 0, 1);
        }
    }
    
    public override void OnStartAuthority()
    {
        enabled = true;
        _basePlayer = GetComponent<BasePlayer>(); //TODO pour seb: Chez le bot ça c'est pas appelé 
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        _basePlayer = GetComponent<BasePlayer>();
        _rigidbody = GetComponent<Rigidbody>();
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Function to start charging the throw
    /// </summary>
    public void ChargeThrow()
    {
        if (!IsCharging && _basePlayer.IsHoldingObject)
        {
            _basePlayer.controller.NerfSpeedOnCharge(true);
            _startChargeTime = Time.time;
            IsCharging = true;
        }
    }
    
    /// <summary>
    /// Function to stop charging the throw, and throw the object
    /// </summary>
    public void ReleaseThrow()
    {
        if (IsCharging && _basePlayer.IsHoldingObject)
        {
            _basePlayer.controller.NerfSpeedOnCharge(false);

            if (IsCharging) IsCharging = false;
            
            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float multiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = multiplier * chargeValue + _minThrowForce;
            Throw(force);
        }
    }

    /// <summary>
    /// Cancel throw and cancel charge
    /// </summary>
    public void CancelThrow()
    {
        if (IsCharging) IsCharging = false;
    }
    
    private float GetNormalizedForce(float force)
    {
        return (force - _minThrowForce) / (_maxThrowForce - _minThrowForce);
    }
    
    /// <summary>
    /// Function to throw the current picked object
    /// </summary>
    /// <param name="force">speed at the begin in meter/s</param>
    public void Throw(float force)
    {
        if (_basePlayer.IsHoldingObject) // If player hold an object
        {
            ThrowableObject throwableObject = _basePlayer.HoldingObject.GetComponent<ThrowableObject>();
            if (throwableObject == null) return; // If the object is not throwable
            
            _basePlayer.IsHoldingObject = false;
            if (_basePlayer.HasTarget) // If player has a target
            {
                GameObject target = _basePlayer.Target;
                //Sets the other player's ui handler to warn him of the incoming ball
                if (target.TryGetComponent(out DirIndicatorHandler uiHandler))
                {
                    uiHandler.incomingBall = throwableObject.transform;
                }
                
                // Calculate the multiplier of step distance
                float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude);
                multiplier += Mathf.Pow(50f, GetNormalizedForce(force));
                multiplier = Mathf.Clamp(multiplier, minStepMultiplier, maxStepMultiplier);

                Vector3 stepPosition = (_basePlayer.Camera.transform.forward * multiplier) + _basePlayer.Camera.transform.position;

                //float accuracy = ChargeValue; // TODO - Maybe need to calculate the accuracy in other way
                float accuracy = 1; // TODO - Maybe need to calculate the accuracy in other way

                throwableObject.Throw(_basePlayer.gameObject, stepPosition, target.transform, force, accuracy, _speedCurve); // Throw the object
            }
            else
            {
                Vector3 direction = _basePlayer.Camera.transform.forward;
                throwableObject.Throw(_basePlayer.gameObject, direction, force); // Throw the object in front of the camera
            }
        }
    }
}