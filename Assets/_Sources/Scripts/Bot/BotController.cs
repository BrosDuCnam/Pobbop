using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
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
    
    [CanBeNull] private NavMeshPath _navMeshPath;
    private Vector3 _finalDestination;
    private Vector3 _destination;
    public Vector3 Destination
    {
        get => _finalDestination;
    }

    public bool HasDestination => _navMeshPath != null;

    private new void Start()
    {
        base.Start();
        _navMeshPath = null;

        print(transform.position);
    }
    
    private new void Update()
    {
        base.Update();

        if (_moveLookDirection)
        {
            if (Time.time < (_finalTime + _timeToReach))
            {
                float normalizedTime = (Time.time - _finalTime) / _timeToReach;
                onDirectionAxis.Invoke(Vector2.Lerp(_baseDirection, _finalLookDirection, normalizedTime));
            }
            else
            {
                onDirectionAxis.Invoke(_finalLookDirection);
                _moveLookDirection = false;
                _tempRotationSpeed = _rotationSpeed;
            }
        }
        
        if (_navMeshPath != null)
        {
            //print(Vector3.Distance(_destination, transform.position));
            //print(_destination);
            if (Vector3.Distance(_destination, transform.position) < 1f) // If the bot is close enough to the destination
            {
                int index = Array.IndexOf(_navMeshPath.corners, _destination);
                if (index < _navMeshPath.corners.Length - 1)
                {
                    _destination = _navMeshPath.corners[index + 1];
                }
                else _navMeshPath = null;
            }
            Debug.DrawLine(transform.position, _destination, Color.red);
            
            Vector3 direction = (_destination - transform.position).normalized;
            print(direction);
            Debug.DrawLine(transform.position, transform.position + direction*30, Color.blue);
            
            float xAxis = Utils.RadianToDegree(new Vector2(direction.y, direction.x));
            xAxis %= 360;
            print(xAxis);
            if (Math.Abs(_finalLookDirection.x - xAxis) > 1f) // If the bot is not facing the right direction
            {
                print(_finalLookDirection);
                LookAt(new Vector2(xAxis, currentLook.y));
            }
            MoveDirection = Vector2.up;
        }
    }
    
    public void SetDestination(Vector3 destination)
    {
        Debug.LogWarning("SetDestination : "+destination);
        _navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, _navMeshPath);
        _finalDestination = destination;
        print("Last corner : " + _navMeshPath.corners.Last());
        _destination = _navMeshPath.corners[0];
        //Utils.DebugNavMeshPath(_navMeshPath, 100);
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