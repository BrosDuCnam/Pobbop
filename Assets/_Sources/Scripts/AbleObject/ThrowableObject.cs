using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : NetworkBehaviour
{ 
    [Header("Settings")]
    [SerializeField] private bool DEBUG;
    [SerializeField] private int _poolSize = 10;
    
    private bool _isThrown = false;
    private Rigidbody _rigidbody;
    [CanBeNull] private GameObject _owner;
    LimitedQueue<Vector3> _poolPositions;
    
    
    private bool _stopThrowPath = false;

    public UnityEvent OnStateChanged;
    /// <summary>
    /// Returns true if the object is currently being thrown.
    /// </summary>
    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            if (_isThrown == value) return;
            _isThrown = value;
            OnStateChanged.Invoke();
        }
    }
    
    private void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _poolPositions = new LimitedQueue<Vector3>(_poolSize);
    }

    private void Update()
    {
        _poolPositions.Enqueue(transform.position);
        
        //print(_owner == null);
    }


    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="direction">Direction to throw</param>
    /// <param name="force">Force in meter/s</param>
    [Command] 
    public void Throw(GameObject player, Vector3 direction, float force)
    {
        if (IsThrown) return;
        _owner = player;
        IsThrown = true;
        _rigidbody.AddForce(transform.position + direction * (force * 50), ForceMode.Acceleration);
    }

    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="curve">The curve of speed during time</param>
    public void Throw(GameObject player , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve)
    {
        StartCoroutine(ThrowEnumerator(player, step, target, speed, accuracy, curve));
    }
    
    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    public void Throw(GameObject player, Vector3 step, Transform target, float speed, float accuracy)
    {
        StartCoroutine(ThrowEnumerator(player, step, target, speed, accuracy, AnimationCurve.Linear(0, 1, 1, 1)));
    }
    
    
    /// <summary>
    /// Enumerator to throw the object during time.
    /// </summary>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="curve">The curve of speed during time</param>
    private IEnumerator ThrowEnumerator(GameObject player, Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve)
    {
        IsThrown = true;

        _owner = player;

        Vector3 origin = transform.position;
        Vector3 originTargetPosition = target.position;
        Vector3 torqueDirection = -Vector3.Cross(origin - step, Vector3.up).normalized;
        
        
        if (DEBUG)
        {
            Utils.DebugBezierCurve(origin, step, target.position, 10, Color.red, 5);
        }
        
        float time = 0;
        float distance = Utils.BezierCurveDistance(origin, step, target.position, 10);
        float i = 1 / (distance / speed);

        Vector3 direction = (target.position - step);
        Vector3 lastPos = origin;
        
        while (time < 1 && !_stopThrowPath)
        {
            Vector3 targetPos = Vector3.Lerp(originTargetPosition, target.position, accuracy);
            Vector3 nextPos = Utils.BezierCurve(origin, step, targetPos, time);
            
            ApplyMovePosition(nextPos);
            ApplyTorque(torqueDirection, time);

            new WaitForFixedUpdate();
            time += (i * (curve.Evaluate(time * 3))) * Time.deltaTime; // TODO - Hacky fix for curve
            
            direction = nextPos - lastPos;
            
            yield return null;
        }
        _owner = null;
        _stopThrowPath = false;
        _rigidbody.useGravity = true;
        IsThrown = false;
        
        ApplyThrowForce(direction);
    }
    
    //fonction appelé dans la coroutine
    [Command]
    private void ApplyThrowForce(Vector3 direction)
    {
        Debug.Log("test");
        _rigidbody.AddForce(direction*50, ForceMode.Acceleration);
    }

    //fonction appelé dans la coroutine
    [Command]
    private void ApplyMovePosition(Vector3 nextPos)
    {
        _rigidbody.MovePosition(nextPos);
    }

    //fonction appelé dans la coroutine
    [Command]
    private void ApplyTorque(Vector3 torqueDirection, float time)
    {
        _rigidbody.AddTorque(torqueDirection * Mathf.Pow(10, time));
    }
    
    /// <summary>
    /// Function to stop the throw path.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        HealthSystem livingObject = other.GetComponent<HealthSystem>();
        if (livingObject != null)
        {
            if (IsThrown || _rigidbody.velocity.magnitude > 2f) // TODO - maybe change the miminum velocity
            {
                livingObject.TakeDamage(1, _owner); // TODO - change the damage
                
                if (livingObject.Health <= 0)
                {
                    Vector3 direction = -_rigidbody.velocity.normalized;
        
                    float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude); 
                    multiplier = Mathf.Clamp(multiplier, 2, 30);
        
                    Throw(null, transform.position + direction * multiplier, _owner.transform, _rigidbody.velocity.magnitude, 1f);
                }
                
            }
        }
        
        if (IsThrown && other.gameObject == _owner?.gameObject)
        {
            _stopThrowPath = true;
            IsThrown = false;
            GetComponent<NetworkIdentity>().RemoveClientAuthority(); //On perd l'authorité sur l'ogject qu'on a drop
        }
    }
}