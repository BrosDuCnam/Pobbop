using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Player _player;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Settings")]
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeCurve;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _minStepMultiplier = 2f;
    [SerializeField] private float _maxnStepMultiplier = 30f;
    [SerializeField] private bool _drawCurve = true;


    private float _startChargeTime;
    
    private void Start()
    {
        _player = GetComponent<Player>();
        _rigidbody = GetComponent<Rigidbody>();
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_drawCurve)
        {
            DrawCurve();
        }
    }

    /// <summary>
    /// Function called when the player starts to throw
    /// </summary>
    /// <param name="ctx">The context of input</param>
    public void ThrowInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            _startChargeTime = Time.time;

        if (ctx.canceled)
        {
            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float multiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = multiplier * chargeValue + _minThrowForce;
            Throw(force);
        }
    }

    private void DrawCurve()
    {
        if (_player.HasTarget && _player.IsHoldingObject)
        {
            _lineRenderer.enabled = true;
            
            // Calculate the multiplier of step distance
            float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude); 
            multiplier = Mathf.Clamp(multiplier, _minStepMultiplier, _maxnStepMultiplier);

            Vector3 stepPosition = (_player.Camera.transform.forward * multiplier) + _player.Camera.transform.position;
            
            Vector3[] positions = Utils.GetBezierCurvePositions(_player.HoldingObject.transform.position, stepPosition, _player.Target.transform.position, 30);
            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
    
    /// <summary>
    /// Function to throw the current picked object
    /// </summary>
    /// <param name="force">speed at the begin in meter/s</param>
    public void Throw(float force)
    {
        if (_player.IsHoldingObject) // If player hold an object
        {
            ThrowableObject throwableObject = _player.HoldingObject.GetComponent<ThrowableObject>();
            if (throwableObject == null) return; // If the object is not throwable
            
            _player.IsHoldingObject = false;
            if (_player.HasTarget) // If player has a target
            {
                GameObject target = _player.Target;
                //Sets the other player's ui handler to warn him of the incoming ball
                if (target.TryGetComponent(out DirIndicatorHandler uiHandler))
                {
                    uiHandler.incomingBall = throwableObject.transform;
                }
                
                // Calculate the multiplier of step distance
                float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude); 
                multiplier = Mathf.Clamp(multiplier, _minStepMultiplier, _maxnStepMultiplier);

                Vector3 stepPosition = (_player.Camera.transform.forward * multiplier) + _player.Camera.transform.position;
                
                throwableObject.Throw(_player, stepPosition, target.transform, force, _speedCurve); // Throw the object
            }
            else
            {
                Vector3 direction = _player.Camera.transform.forward;
                throwableObject.Throw(_player, direction, force); // Throw the object in front of the camera
            }
        }
    }
    
}