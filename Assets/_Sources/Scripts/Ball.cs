using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private PickableObject _pickableObject;
    [SerializeField] private ThrowableObject _throwableObject;
    [SerializeField] private BallState _ballState;

    public BallState BallState
    {
        get => _ballState;
    }
    
    private void StateChanged()
    {
        if (_pickableObject.IsPicked)
        {
            _ballState = BallState.Picked;
        }
        else if (_throwableObject.IsThrown)
        {
            _ballState = BallState.Thrown;
        }
        else
        {
            _ballState = BallState.Idle;
        }
    }
    
    private void Start()
    {
        _pickableObject = GetComponent<PickableObject>();
        _throwableObject = GetComponent<ThrowableObject>();
        
        _pickableObject.OnStateChanged.AddListener(StateChanged);
        _throwableObject.OnStateChanged.AddListener(StateChanged);
    }
    
    
}

public enum BallState
{
    Picked,
    Thrown,
    Idle
}