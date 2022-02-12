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
    [SerializeField] private bool _reboundOnKill;
    
    private ThrowState _throwState = ThrowState.Idle;
    private Rigidbody _rigidbody;
    [CanBeNull] private GameObject _owner;
    LimitedQueue<Vector3> _poolPositions;

    public UnityEvent OnStateChanged;
    
    private bool _stopThrow;

    /// <summary>
    /// Returns true if the object is currently being thrown.
    /// </summary>
    public ThrowState ThrowState
    {
        get => _throwState;
        set
        {
            if (_throwState == value) return;
            _throwState = value;
            OnStateChanged.Invoke();
        }
    }
    
    public GameObject Owner
    {
        get => _owner;
        private set
        {
            _owner = value;
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

        //print(ThrowState);
    }


    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="direction">Direction to throw</param>
    /// <param name="force">Force in meter/s</param>
    [Command]
    public void Throw(GameObject player, Vector3 direction, float force)
    {
        if (ThrowState != ThrowState.Idle) return;
        Owner = player;
        ThrowState = ThrowState.FreeThrow;
        _rigidbody.AddForce(transform.position + direction * (force * 50), ForceMode.Acceleration);
    }

    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="player">The player who throw the object</param>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="accuracy">The accuracy of throw (ex: 1 = object finish path on the target, 0.5 = object finish path between first position and now position of the target</param>
    /// <param name="curve">The curve of speed during time</param>
    /// <param name="state">The future state of the object</param>
    public void Throw(GameObject player , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve, ThrowState state = ThrowState.Thrown)
    {
        StartCoroutine(ThrowEnumerator(player, step, target, speed, accuracy, curve, state));
    }

    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="player">The player who throw the object</param>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="accuracy">The accuracy of throw (ex: 1 = object finish path on the target, 0.5 = object finish path between first position and now position of the target</param>
    /// <param name="state">The future state of the object</param>
    public void Throw(GameObject player, Vector3 step, Transform target, float speed, float accuracy, ThrowState state = ThrowState.Thrown)
    {
        StartCoroutine(ThrowEnumerator(player, step, target, speed, accuracy, AnimationCurve.Linear(0, 1, 1, 1), state));
    }


    /// <summary>
    /// Enumerator to throw the object during time.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="accuracy">The accuracy of throw (ex: 1 = object finish path on the target, 0.5 = object finish path between first position and now position of the target</param>
    /// <param name="curve">The curve of speed during time</param>
    /// <param name="state">The future state of the object</param>
    private IEnumerator ThrowEnumerator(GameObject player, Vector3 step, Transform target, float speed, float accuracy,
        AnimationCurve curve, ThrowState state = ThrowState.Thrown)
    {
        ThrowState = ThrowState.Thrown;ThrowState = state;

        Owner = player;

        Vector3 origin = transform.position;
        Vector3 originTargetPosition = target.position;
        Vector3 torqueDirection = -Vector3.Cross(origin - step, Vector3.up).normalized;
        
        
        if (DEBUG)
        {
            Utils.DebugBezierCurve(origin, step, target.position, 10, Color.red, 5);
        }
        
        GetComponent<NetworkIdentity>().AssignClientAuthority(Owner.GetComponent<NetworkIdentity>().connectionToClient);
        while (!GetComponent<NetworkIdentity>().hasAuthority) yield return null;
        
        float time = 0;
        float distance = Utils.BezierCurveDistance(origin, step, target.position, 10);
        float i = 1 / (distance / speed);

        Vector3 direction = (target.position - step);
        Vector3 lastPos = origin;
        
        while (time < 1 && ThrowState != ThrowState.Idle && !_stopThrow)
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
        
        Owner = null;
        _rigidbody.useGravity = true;
        _stopThrow = false;
        ThrowState = ThrowState.Idle;
        
        ApplyThrowForce(direction);
        
        GetComponent<NetworkIdentity>().RemoveClientAuthority(); //On perd l'authorité sur l'ogject qu'on a drop
    }
    
    //fonction appelé dans la coroutine
    [Command]
    private void ApplyThrowForce(Vector3 direction)
    {
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
        _rigidbody.AddTorque(torqueDirection * Mathf.Pow(100, time));
    }
    
    /// <summary>
    /// Function to stop the throw path.
    /// </summary>
    /// <param name="other">The collision</param>
    private void OnCollisionEnter(Collision other)
    {
        StartCoroutine(OnCollisionEnterCoroutine(other));
    }

    /// <summary>
    /// Coroutine to stop the throw path.
    /// </summary>
    /// <param name="other">The collision</param>
    private IEnumerator OnCollisionEnterCoroutine(Collision other)
    {
        GameObject owner = Owner;
        GameObject otherObject = other.gameObject;
        print(ThrowState);
        if (ThrowState != ThrowState.Idle)
        {
            BasePlayer basePlayerParent = otherObject.GetComponentInParent<BasePlayer>();
            // If the player is the owner of the object
            if (Owner != null && (otherObject == Owner || (basePlayerParent != null && basePlayerParent.gameObject == Owner))) yield return null;
            else
            {
                if (ThrowState == ThrowState.FreeThrow) ThrowState = ThrowState.Idle;
                else _stopThrow = true;
            }
        }

        while (ThrowState != ThrowState.Idle) yield return null; // Wait for the end of the throw

        HealthSystem livingObject = otherObject.GetComponent<HealthSystem>();
        if (livingObject != null)
        {
            if (ThrowState != ThrowState.Idle || _rigidbody.velocity.magnitude > 2f) // TODO - maybe change the miminum velocity
            {
                livingObject.TakeDamage(1, _owner); // TODO - change the damage
                
                if (_reboundOnKill)
                {
                    if (livingObject.Health <= 0 && owner != null) // TODO change 1000 to other value
                    {
                        Vector3 direction = _poolPositions.ToArray()[0] - transform.position;
                        float multiplier = _rigidbody.velocity.magnitude;
                        multiplier = Mathf.Clamp(multiplier, 2, 30);

                        Throw(owner, transform.position + direction * multiplier, owner.transform,
                            15, 1f, ThrowState.Rebound);
                        
                        //Utils.DebugBezierCurve(transform.position, transform.position + direction * multiplier, owner.transform.position, 100, Color.blue, 540);
                    }
                }
                
            }
        }
    }
    
}

/// <summary>
/// Enum to define the state of the throw.
/// </summary>
public enum ThrowState
{
    /// <summary>
    /// The throw is idle.
    /// </summary>
    Idle,
    /// <summary>
    /// The throw is in progress.
    /// </summary>
    Thrown,
    /// <summary>
    /// The throw is in progress and the object is rebounding.
    /// </summary>
    Rebound,
    /// <summary>
    /// The object is throw without curve
    /// </summary>
    FreeThrow
}