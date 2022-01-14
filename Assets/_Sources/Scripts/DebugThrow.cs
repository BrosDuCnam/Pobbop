using System;
using UnityEngine;

[RequireComponent(typeof(ThrowableObject))]
public class DebugThrow : MonoBehaviour
{
    [SerializeField] private ThrowableObject _throwableObject;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private Transform _throwTarget;
    [SerializeField] private Transform _throwStep;
    [SerializeField] private AnimationCurve _throwCurve;

    private void Start()
    {
        _throwableObject = GetComponent<ThrowableObject>();
        
        //Utils.DebugBezierCurve(transform.position, _throwStep.position, _throwTarget.position, 100, Color.red, 100f);
        _throwableObject.Throw(_throwStep.position, _throwTarget, _throwForce, _throwCurve);
    }

    private void Update()
    {
        //print(Utils.BezierCurveDistance(transform.position, _throwStep.position, _throwTarget.position, 100));
    }
}