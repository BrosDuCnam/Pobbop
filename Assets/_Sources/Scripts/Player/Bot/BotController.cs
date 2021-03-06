using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class BotController : Controller
{
    [Tooltip("1 = 360° in one second")]
    [SerializeField] private float _rotationSpeed = 1f;

    private bool _crouch;
    private bool _lastCrouchInfo;
    
    public UnityEvent StopLook;
    public UnityEvent StopLocomotion;
    public Vector2 lookDestination { get; private set; }
    public bool hasDestination;
    public Vector3 destination { get; private set; }
    public Vector3 tempDestination { get; private set; }

    private new void Update()
    {
        base.Update();

        // If the bot uncrouch make crouch value to false
        if (!base.crouch && _lastCrouchInfo) _crouch = false;
        crouch = _crouch;
        
        _lastCrouchInfo = base.crouch;
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
    
    public void TimedLookAt(Transform target, float time, Action onFinished = null)
    {
        StartCoroutine(SetTimedDirectionCoroutine(() => GetDirection(target.position), _rotationSpeed, onFinished));
    }
    
    public void TimedLookAt(Vector3 target, float time, Action onFinished = null)
    {
        StartCoroutine(SetTimedDirectionCoroutine(() => GetDirection(target), _rotationSpeed, onFinished));
    }
    
    public void TimedLookAt(Vector2 direction, float time, Action onFinished = null)
    {
        StartCoroutine(SetTimedDirectionCoroutine(() => direction, _rotationSpeed, onFinished));
    }
    
    public void LookAt(Transform target, Action onFinished = null)
    {
        StartCoroutine(SetDirectionCoroutine(() => GetDirection(target.position), _rotationSpeed, onFinished));
    }
    
    public void LookAt(Vector3 target, Action onFinished = null)
    {
        StartCoroutine(SetDirectionCoroutine(() => GetDirection(target), _rotationSpeed, onFinished));
    }
    
    public void LookAt(Vector2 direction, Action onFinished = null)
    {
        StartCoroutine(SetDirectionCoroutine(() => direction, _rotationSpeed, onFinished));
    }

    private void Rotate(Vector2 direction)
    {
        onDirectionAxis.Invoke(direction);
    }

    private IEnumerator SetTimedDirectionCoroutine(Func<Vector2> directionFunc, float time, Action callback)
    {
        if (callback == null) callback = () => { };
        
        Vector2 origin = currentLook;
        origin.x = Utils.DegreeFormat180To360(origin.x);
        
        float t = 0;
        float distance = Vector2.Distance(currentLook, directionFunc.Invoke());
        float i = (distance / time);
        bool stopLookAt = false;
        StopLook.AddListener(() => stopLookAt = true);

        while (t < 1 && !stopLookAt)
        {
            Vector2 nextDirection = Vector2.Lerp(origin, directionFunc.Invoke(), t);

            Rotate(nextDirection - currentLook);

            yield return new WaitForEndOfFrame();
            t += i * Time.deltaTime;
        }
        callback.Invoke();
    }

    private IEnumerator SetDirectionCoroutine(Func<Vector2> directionFunc, float speed, Action callback)
    {
        if (callback == null) callback = () => StopLook.Invoke();
        
        StopLook.Invoke();
        
        Vector2 direction = directionFunc.Invoke();
        
        bool stopLookAt = false;
        StopLook.AddListener(() => stopLookAt = true);
        
        while (stopLookAt == false)
        {
            Vector2 destination = directionFunc.Invoke();
            lookDestination = destination;
            
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

    public void GoTo(GameObject destination, float strafeAccuracy = -1)
    {
        StartCoroutine(GoToCoroutine(() => destination.transform.position, strafeAccuracy));
    }
    
    public void GoTo(Vector3 destination, float strafeAccuracy = -1)
    {
        StartCoroutine(GoToCoroutine(() => destination, strafeAccuracy));
    }

    public void GoTo(Transform target, float strafeAccuracy = -1)
    {
        StartCoroutine(GoToCoroutine(() => target.position, strafeAccuracy));
    }
    
    private IEnumerator GoToCoroutine(Func<Vector3> destinationFunc, float strafeAccuracy)
    {
        StopLocomotion.Invoke();
        
        if (strafeAccuracy == -1) strafeAccuracy = Random.Range(0.3f, 1f);
        

        NavMeshPath path = new NavMeshPath();
        destination = destinationFunc.Invoke();
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
        List<Vector3> corners = path.corners.ToList();

        LookAt(corners[0], (() => {}));
        // print(1);
        
        bool stopLocomotion = false;
        StopLocomotion.AddListener(() => stopLocomotion = true);
        
        while (corners.Count > 0 && !stopLocomotion)
        {
            tempDestination = corners[0];
            if (Vector3.Distance(transform.position, tempDestination) > 1f)
            {
                // print(2);
                Vector3 direction3D = transform.InverseTransformPoint(tempDestination);
                Vector2 direction = new Vector2(direction3D.x, direction3D.z).normalized;
                // print(2.1);

                direction = Vector2.Lerp(Vector2.up, direction, strafeAccuracy);
                // print(2.2);

                onAxis.Invoke(direction);
                // print(2.3);

                hasDestination = true;
                yield return new WaitForFixedUpdate();
                // print(2.4);

                destination = destinationFunc.Invoke();
                NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
                corners = path.corners.ToList();
                Utils.DebugNavMeshPath(path);
                // print(2.5);

            }
            else
            {
                print(3);
                corners.RemoveAt(0);
                
                if (corners.Count > 0)
                {
                    print("Look at");
                    LookAt(corners[0], (() => { }));
                }
            }
        }
        // print(4);
        onAxis.Invoke(Vector2.zero);
        //StopLook.Invoke();
        hasDestination = false;
        yield return null;
    }

    #endregion

    /// <summary>
    /// Function to make the bot jump
    /// </summary>
    /// <param name="time">The time to jump</param>
    public void Jump(float time)
    {
        StartCoroutine(Utils.TimedAction(time, b => onJump.Invoke(b)));
    }
    
    /// <summary>
    /// Function to make the bot crouch
    /// </summary>
    /// <param name="time">The time to crouch</param>
    public void Crouch(float time)
    {
        StartCoroutine(Utils.TimedAction(time, b => onCrouch.Invoke(b)));
    }

    /// <summary>
    /// Function to set or not the bot crouch
    /// </summary>
    /// <param name="active">true if the bot crouch</param>
    public void Crouch(bool active)
    {
        _crouch = active;
    }

    /// <summary>
    /// Function to make the bot run
    /// </summary>
    public void Run()
    {
        onRunStart.Invoke();
    }
}