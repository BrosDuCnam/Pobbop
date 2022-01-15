using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : MonoBehaviour
{
    public UnityEvent OnStateChanged;
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

    [SerializeField] private bool DEBUG;

    private bool _stopThrowPath = false;

    private void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 direction, float force)
    {
        if (IsThrown) return;
        IsThrown = true;
        _rigidbody.AddForce(transform.position + direction * (force * 50), ForceMode.Acceleration);
    }

    public void Throw(Vector3 step, Transform target, float speed, AnimationCurve curve)
    {
        StartCoroutine(ThrowEnumerator(step, target, speed, curve));
    }
    
    public void Throw(Vector3 step, Transform target, float speed)
    {
        StartCoroutine(ThrowEnumerator(step, target, speed, AnimationCurve.Linear(0, 1, 1, 1)));
    }
    
    private IEnumerator ThrowEnumerator(Vector3 step, Transform target, float speed, AnimationCurve curve)
    {
        Vector3 origin = transform.position;
        Vector3 torqueDirection = -Vector3.Cross(origin - step, Vector3.up).normalized;
        
        //_rigidbody.useGravity = false;
        _isThrown = true;
        
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
            Vector3 nextPos = Utils.BezierCurve(origin, step, target.position, time);
            
            _rigidbody.MovePosition(nextPos);
            _rigidbody.AddTorque(torqueDirection * Mathf.Pow(10, time));

            new WaitForEndOfFrame();
            time += (i * (curve.Evaluate(time * 3))) * Time.deltaTime; // TODO - Hacky fix for curve
            
            direction = nextPos - lastPos;
            
            yield return null;
        }
        _stopThrowPath = false;
        _rigidbody.useGravity = true;
        _isThrown = false;
        
        _rigidbody.AddForce(direction*50, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isThrown)
        {
            _stopThrowPath = true;
            _isThrown = false;
        }
    }
}