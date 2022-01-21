using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : MonoBehaviour
{
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
    
    [SerializeField] private bool _isThrown = false;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] [CanBeNull] private Player _player;
    
    [SerializeField] private bool DEBUG;

    private bool _stopThrowPath = false;

    private void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //print("etat :"+IsThrown);
    }

    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="direction">Direction to throw</param>
    /// <param name="force">Force in meter/s</param>
    public void Throw(Player player, Vector3 direction, float force)
    {
        if (IsThrown) return;
        _player = player;
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
    public void Throw(Player player , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve)
    {
        StartCoroutine(ThrowEnumerator(_player, step, target, speed, accuracy, curve));
    }
    
    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    public void Throw(Player player, Vector3 step, Transform target, float speed, float accuracy)
    {
        StartCoroutine(ThrowEnumerator(_player, step, target, speed, accuracy, AnimationCurve.Linear(0, 1, 1, 1)));
    }
    
    
    /// <summary>
    /// Enumerator to throw the object during time.
    /// </summary>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="curve">The curve of speed during time</param>
    private IEnumerator ThrowEnumerator(Player player, Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve)
    {
        IsThrown = true;

        _player = player;
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
            
            _rigidbody.MovePosition(nextPos);
            _rigidbody.AddTorque(torqueDirection * Mathf.Pow(10, time));

            new WaitForEndOfFrame();
            time += (i * (curve.Evaluate(time * 3))) * Time.deltaTime; // TODO - Hacky fix for curve
            
            direction = nextPos - lastPos;
            
            yield return null;
        }
        _player = null;
        _stopThrowPath = false;
        _rigidbody.useGravity = true;
        IsThrown = false;
        
        _rigidbody.AddForce(direction*50, ForceMode.Acceleration);
    }

    /// <summary>
    /// Function to stop the throw path.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (IsThrown && other.gameObject == _player?.gameObject)
        {
            _stopThrowPath = true;
            IsThrown = false;
        }
    }
}