using System;
using Mirror;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThrowSystem : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private BasePlayer basePlayer;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Slider _slider;

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
/// <summary>
/// Get charge value between 0 and 1
/// </summary>
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
        basePlayer = GetComponent<BasePlayer>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        //_player = GetComponent<Player>();
        //_rigidbody = GetComponent<Rigidbody>();
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Function to start charging the throw
    /// </summary>
    public void ChargeThrow()
    {
        if (!IsCharging && basePlayer.IsHoldingObject)
        {
            _startChargeTime = Time.time;
            IsCharging = true;
        }
    }
    
    /// <summary>
    /// Function to stop charging the throw, and throw the object
    /// </summary>
    public void ReleaseThrow()
    {
        if (IsCharging && basePlayer.IsHoldingObject)
        {
            if (IsCharging) IsCharging = false;
            
            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float multiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = multiplier * chargeValue + _minThrowForce;
            Throw(force);
        }
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
        if (basePlayer.IsHoldingObject) // If player hold an object
        {
            ThrowableObject throwableObject = basePlayer.HoldingObject.GetComponent<ThrowableObject>();
            if (throwableObject == null) return; // If the object is not throwable
            
            basePlayer.IsHoldingObject = false;
            if (basePlayer.HasTarget) // If player has a target
            {
                GameObject target = basePlayer.Target;

                // Calculate the multiplier of step distance
                float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude);
                multiplier += Mathf.Pow(50f, GetNormalizedForce(force));
                multiplier = Mathf.Clamp(multiplier, minStepMultiplier, maxStepMultiplier);

                Vector3 stepPosition = (basePlayer.Camera.transform.forward * multiplier) + basePlayer.Camera.transform.position;

                float accuracy = ChargeValue; // TODO - Maybe need to calculate the accuracy in other way

                throwableObject.Throw(basePlayer.gameObject, stepPosition, target.transform, force, accuracy, _speedCurve); // Throw the object
            }
            else
            {
                Vector3 direction = basePlayer.Camera.transform.forward;
                throwableObject.Throw(basePlayer.gameObject, direction, force); // Throw the object in front of the camera
            }
        }
    }
}