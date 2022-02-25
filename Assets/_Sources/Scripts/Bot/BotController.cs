using System;
using System.Collections;
using System.Collections.Generic;
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

    public UnityEvent StopLook;
    public UnityEvent StopLocomotion;

    private new void Awake()
    {
        base.Awake();

        //print(transform.position);
    }

    private void Start()
    {
        //LookAt(target.position, () => StopLook.Invoke());
        GoTo(target, 1);
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

    #region Look Region

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

    private void LookAt(Transform target, Action onFinished)
    {
        StartCoroutine(SetDirectionCoroutine(() => GetDirection(target.position), _rotationSpeed, onFinished));
    }
    
    private void LookAt(Vector3 target, Action onFinished)
    {
        StartCoroutine(SetDirectionCoroutine(() => GetDirection(target), _rotationSpeed, onFinished));
    }
    
    private void LookAt(Vector2 direction, Action onFinished)
    {
        StartCoroutine(SetDirectionCoroutine(() => direction, _rotationSpeed, onFinished));
    }

    private void Rotate(Vector2 direction)
    {
        onDirectionAxis.Invoke(direction);
    }

    private IEnumerator SetDirectionCoroutine(Func<Vector2> directionFunc, float speed, Action callback)
    {
        StopLook.Invoke();
        
        Vector2 direction = directionFunc.Invoke();
        
        bool stopLookAt = false;
        StopLook.AddListener(() => stopLookAt = true);
        
        while (stopLookAt == false)
        {
            Vector2 destination = directionFunc.Invoke();
            
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
            if (Vector2.Distance(currentLook, destination) < 10f)
            {
                //print(Vector2.Distance(currentLook, destination));
                callback.Invoke();
            }
            
            yield return new WaitForEndOfFrame();
        }
        stopLookAt = false;
    }


    #endregion


    #region Locomotion

    public void GoTo(Vector3 destination, float strafeAccuracy = 0)
    {
        StartCoroutine(GoToCoroutine(() => destination, strafeAccuracy));
    }

    public void GoTo(Transform target, float strafeAccuracy = 0)
    {
        StartCoroutine(GoToCoroutine(() => target.position, strafeAccuracy));
    }
    
    private IEnumerator GoToCoroutine(Func<Vector3> destinationFunc, float strafeAccuracy)
    {
        StopLocomotion.Invoke();
        
        NavMeshPath path = new NavMeshPath();
        Vector3 destination = destinationFunc.Invoke();
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
        List<Vector3> corners = path.corners.ToList();
        
        LookAt(corners[0], (() => {}));

        bool stopLocomotion = false;
        StopLocomotion.AddListener(() => stopLocomotion = true);
        
        while (corners.Count > 0 && !stopLocomotion)
        {
            if (Vector3.Distance(destination, destinationFunc.Invoke()) > 1)
            {
                destination = destinationFunc.Invoke();
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
                corners = path.corners.ToList();
            }
            
            Vector3 nextDestination = corners[0];
            if (Vector3.Distance(transform.position, nextDestination) > 1f)
            {
                Vector3 direction3D = transform.position - nextDestination;
                Vector2 direction = new Vector2(direction3D.x, direction3D.z).normalized;
                
                direction = Vector2.Lerp(Vector2.up, direction - Utils.DegreeToVector2(currentLook.x), strafeAccuracy);

                onAxis.Invoke(direction);

                yield return new WaitForEndOfFrame();
            }
            else
            {
                corners.RemoveAt(0);
                
                if (corners.Count > 0)
                    LookAt(corners[0], (() => {}));
            }
        }
        StopLook.Invoke();
        yield return null;
    }

    #endregion

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