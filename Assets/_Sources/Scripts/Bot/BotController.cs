using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class BotController : Controller
{
    [SerializeField] public Transform target;
    
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

    public UnityEvent StopLook;
    
    [CanBeNull] private NavMeshPath _navMeshPath;
    private Vector3 _finalDestination;
    private Vector3 _destination;
    public Vector3 Destination
    {
        get => _finalDestination;
    }

    public bool HasDestination => _navMeshPath != null;

    private new void Awake()
    {
        base.Awake();
        _navMeshPath = null;

        //print(transform.position);
    }

    private void Start()
    {
        LookAt(target, () => StopLook.Invoke());
    }

    private new void Update()
    {
        base.Update();


        /*
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
        */
    }
    
    private Vector2 GetDirection(Vector3 target)
    {
        // Set XAxis
        float xAxis = 0;
        Vector3 direction3D = (target - transform.position).normalized;
        Vector2 direction2D = new Vector2(direction3D.z, direction3D.x);

        xAxis = Utils.RadianToDegree(direction2D);
        xAxis = Utils.DegreeFormat360To180(xAxis);

        
        // Set YAxis
        float yAxis = 0;
        direction2D = new Vector2(Vector3.Distance(camera.transform.position, target), target.y).normalized;
        yAxis = Utils.RadianToDegree(direction2D);

        return new Vector2(xAxis, yAxis);
    }

    private void LookAt(Transform target, Action callback)
    {
        StopLook.Invoke();
        StartCoroutine(SetDirectionCoroutine(target, _rotationSpeed, () => callback.Invoke()));
    }

    private void LookAt(Vector2 direction, Action callback)
    {
        StopLook.Invoke();
        StartCoroutine(SetDirectionCoroutine(direction, _rotationSpeed, () => callback.Invoke()));
    }
    
    private void Rotate(Vector2 direction)
    {
        print("Rotate: "+direction);
        onDirectionAxis.Invoke(direction);
    }
    
    private IEnumerator SetDirectionCoroutine(Vector2 direction, float speed, Action callback)
    {
        bool stopLookAt = false;
        StopLook.AddListener(() => stopLookAt = true);

        
        while (stopLookAt == false)
        {
            Vector2 destination = direction;
            
            if (Utils.DegreeFormat360To180(destination.x) > 90 || Utils.DegreeFormat360To180(destination.x) < -90)
            {
                currentLook.x = Utils.DegreeFormat180To360(currentLook.x);
                destination.x = Utils.DegreeFormat180To360(destination.x);
            }
            
            else if (Utils.DegreeFormat180To360(destination.x) > 180)
            {
                currentLook.x = Utils.DegreeFormat360To180(currentLook.x);
                destination.x = Utils.DegreeFormat360To180(destination.x);
            }
            
            direction = destination - currentLook;
            if (direction.magnitude > 1) direction = direction.normalized;
            direction *= Time.deltaTime * speed;
            
            Vector2 nextDirection = currentLook + direction;

            if (direction.x > 0) if (nextDirection.x > destination.x) direction.x = 0;
            if (direction.x < 0) if (nextDirection.x < destination.x) direction.x = 0;
            
            if (direction.y > 0) if (nextDirection.y > destination.y) direction.y = 0;
            if (direction.y < 0) if (nextDirection.y < destination.y) direction.y = 0;

            Rotate(direction);
            if (Vector2.Distance(currentLook, destination) < 0.1f) callback.Invoke();
            
            yield return new WaitForEndOfFrame();
        }
        stopLookAt = false;
    }
    
    private IEnumerator SetDirectionCoroutine(Transform target, float speed, Action callback)
    {
        Vector2 direction = GetDirection(target.position);
        
        bool stopLookAt = false;
        StopLook.AddListener(() => stopLookAt = true);
        
        while (stopLookAt == false)
        {
            Vector2 destination = GetDirection(target.position);
            
            if (Utils.DegreeFormat360To180(destination.x) > 90 || Utils.DegreeFormat360To180(destination.x) < -90)
            {
                currentLook.x = Utils.DegreeFormat180To360(currentLook.x);
                destination.x = Utils.DegreeFormat180To360(destination.x);
            }
            
            else if (Utils.DegreeFormat180To360(destination.x) > 180)
            {
                currentLook.x = Utils.DegreeFormat360To180(currentLook.x);
                destination.x = Utils.DegreeFormat360To180(destination.x);
            }
            
            direction = destination - currentLook;
            if (direction.magnitude > 1) direction = direction.normalized;
            direction *= Time.deltaTime * speed;

            Vector2 nextDirection = currentLook + direction;

            if (direction.x > 0) if (nextDirection.x > destination.x) direction.x = 0;
            if (direction.x < 0) if (nextDirection.x < destination.x) direction.x = 0;
            
            if (direction.y > 0) if (nextDirection.y > destination.y) direction.y = 0;
            if (direction.y < 0) if (nextDirection.y < destination.y) direction.y = 0;

            Rotate(direction);
            print(Vector2.Distance(currentLook, destination));
            if (Vector2.Distance(currentLook, destination) < 10f)
            {
                //print(Vector2.Distance(currentLook, destination));
                callback.Invoke();
            }
            
            yield return new WaitForEndOfFrame();
        }
        stopLookAt = false;
    }

    public void SetDestination(Vector3 destination)
    {
        Debug.LogWarning("SetDestination : "+destination);
        _navMeshPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, _navMeshPath);
        _finalDestination = destination;
        _destination = _navMeshPath.corners[0];
        //Utils.DebugNavMeshPath(_navMeshPath, 100);
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