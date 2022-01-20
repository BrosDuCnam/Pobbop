using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Player _player;
    [SerializeField] private Rigidbody _rigidbody;

    [Header("Settings")]
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeCurve;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _minStepMultiplier = 2f;
    [SerializeField] private float _maxnStepMultiplier = 30f;


    private float _startChargeTime;
    
    private void Start()
    {
        _player = GetComponent<Player>();
        _rigidbody = GetComponent<Rigidbody>();
    }

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
            //print($"chargeTime: {chargeTime}, charge value {chargeValue}, multiplier: {multiplier}, force: {force}");
        }
    }
    
    public void Throw(float force)
    {
        if (_player.IsHoldingObject)
        {
            ThrowableObject throwableObject = _player.HoldingObject.GetComponent<ThrowableObject>();
            if (throwableObject == null) return;
            
            _player.IsHoldingObject = false;
            if (_player.HasTarget)
            {
                GameObject target = _player.Target;
                //Sets the other player's ui handler to warn him of the incoming ball
                if (target.TryGetComponent(out DirIndicatorHandler uiHandler))
                {
                    uiHandler.incomingBall = throwableObject.transform;
                }
                
                float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude);
                multiplier = Mathf.Clamp(multiplier, _minStepMultiplier, _maxnStepMultiplier);

                Vector3 stepPosition = (_player.Camera.transform.forward * multiplier) + _player.Camera.transform.position;
                
                throwableObject.Throw(stepPosition, target.transform, force, _speedCurve);
            }
            else
            {
                Vector3 direction = _player.Camera.transform.forward;
                throwableObject.Throw(direction, force);
            }
        }
    }
    
}