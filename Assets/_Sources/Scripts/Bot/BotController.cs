using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BotController : Controller
{
    [Tooltip("1 = 360° in one second")]
    [SerializeField] private float _rotationSpeed = 1f;
    
    private Vector2 _moveDirection;
    public Vector2 MoveDirection
    {
        get => _moveDirection;
        set
        {
            _moveDirection = value;
            onAxis.Invoke(_moveDirection);
        }
    }

    private float _tempRotationSpeed;
    private bool _moveLookDirection;
    private float _finalTime;
    private float _timeToReach;
    private Vector2 _baseDirection;
    private Vector2 _finalLookDirection;
    private Vector2 _lookDirection
    {
        get => currentLook;
        set
        {
            _moveLookDirection = true;
            _finalLookDirection = value;
            _tempRotationSpeed = _rotationSpeed;
        }
    }
    
    private new void Update()
    {
        base.Update();
        
        if (_moveLookDirection)
        {
            if (Time.time < (_finalTime + _timeToReach))
            {
                float normalizedTime = (Time.time - _finalTime) / _timeToReach;
                print($"Interpolating {currentLook} to {_finalLookDirection}, speed: {_tempRotationSpeed}, time {normalizedTime}, now: {Vector2.Lerp(currentLook, _finalLookDirection, Time.deltaTime * _tempRotationSpeed)}");
                onDirectionAxis.Invoke(Vector2.Lerp(_baseDirection, _finalLookDirection, normalizedTime));
                print(Vector2.Lerp(currentLook, _finalLookDirection, normalizedTime));
            }
            else
            {
                onDirectionAxis.Invoke(_finalLookDirection);
                _moveLookDirection = false;
                _tempRotationSpeed = _rotationSpeed;
            }
        }
    }

    /// <summary>
    /// Function to move look direction to a target
    /// </summary>
    /// <param name="direction">The target direction</param>
    public void LookAt(Vector2 direction)
    {
        LookAt(direction, _rotationSpeed);
    }
    
    /// <summary>
    /// Function to move look direction to a target
    /// </summary>
    /// <param name="direction">The tareggt direction</param>
    /// <param name="speed">The speed, 1: 360° in one second</param>
    public void LookAt(Vector2 direction, float speed)
    {
        _baseDirection = currentLook;
        _lookDirection = direction;
        _tempRotationSpeed = speed;
        _timeToReach = Vector2.Distance(currentLook, direction) / (1/speed * 360);
        _finalTime = Time.time;
        print($"Final time: {_finalTime}");
    }

    /// <summary>
    /// Function to make the bot jump
    /// </summary>
    /// <param name="time">The time to jump</param>
    public void Jump(float time)
    {
        StartCoroutine(TimedAction(time, b => onJump.Invoke(b)));
    }
    
    /// <summary>
    /// Function to make the bot crouch
    /// </summary>
    /// <param name="time">The time to crouch</param>
    public void Crouch(float time)
    {
        StartCoroutine(TimedAction(time, b => onCrouch.Invoke(b)));
    }

    /// <summary>
    /// Function to make the bot run
    /// </summary>
    public void Run()
    {
        onRunStart.Invoke();
    }
    
    /// <summary>
    /// Coroutine to make the bot a timed action
    /// </summary>
    /// <param name="time">The time to callback true</param>
    /// <param name="callback">The callback to call</param>
    private IEnumerator TimedAction(float time, Action<bool> callback)
    {
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            callback(true);
            yield return new WaitForEndOfFrame();
        }
        callback(false);
    }
}