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
        _rigidbody.isKinematic = true;
        _isThrown = true;
        
        if (DEBUG)
        {
            Utils.DebugBezierCurve(origin, step, target.position, 10, Color.red, 5);
        }
        
        float time = 0;
        float distance = Utils.BezierCurveDistance(origin, step, target.position, 10);
        float i = 1 / (distance / speed);
        
        while (time < 1 && !_stopThrowPath)
        {
            Vector3 nextPos = Utils.BezierCurve(origin, step, target.position, time);
            _rigidbody.MovePosition(nextPos);
            
            new WaitForEndOfFrame();
            time += (i * curve.Evaluate(time)) * Time.deltaTime;
            yield return null;
        }
        _stopThrowPath = false;
        _rigidbody.isKinematic = false;
        _isThrown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isThrown)
        {
            print("Collide");
            _stopThrowPath = true;
        }
    }
}