using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Compression;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Controller), typeof(Rigidbody))]
public class BasePlayer : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] protected TargetSystem _targetSystem;
    [SerializeField] protected PickUpDropSystem _pickUpDropSystem;
    [SerializeField] protected ThrowSystem _throwSystem;
    [SerializeField] protected HealthSystem _healthSystem;
    [SerializeField] public Controller _controller;
    [SerializeField] protected Rigidbody _rigidbody;
    
    public Camera Camera;

    [HideInInspector] public int teamNumber = 0;
    
    /// <summary>
    /// Return true if the player hold an object
    /// </summary>
    public bool IsHoldingObject
    {
        get => _pickUpDropSystem.PickableObject != null;
        set {
            if (!value) _pickUpDropSystem.PickableObject = null;
        }
    }
    
    /// <summary>
    /// Return the current holding object, null if the player is not holding an object
    /// </summary>
    [CanBeNull] public GameObject HoldingObject
    {
        get
        {
            if (_pickUpDropSystem.PickableObject == null) return null;
            return _pickUpDropSystem.PickableObject.gameObject; 
        }
    }
    
    /// <summary>
    /// Return true if the player has a target, can be use to know if the player can see an enemy
    /// </summary>
    public bool HasTarget
    {
        get => _targetSystem.CurrentTarget != null;
    }
    
    /// <summary>
    /// Return the current target, null if the player has no target
    /// </summary>
    [CanBeNull] public GameObject Target
    {
        get => _targetSystem.CurrentTarget.gameObject;
    }
    
    /// <summary>
    /// Return true if the player is charging an attack
    /// </summary>
    public bool IsCharging
    {
        get => _throwSystem.IsCharging;
    }
    
    /// <summary>
    /// Cancel the ball charge if the player is charging
    /// </summary>
    /// <example>Use when sprinting</example>>
    public void CancelCharge()
    {
        _throwSystem.CancelThrow();
    }

    protected void Awake()
    {
        _targetSystem = GetComponent<TargetSystem>();
        _pickUpDropSystem = GetComponent<PickUpDropSystem>();
        _throwSystem = GetComponent<ThrowSystem>();
        _healthSystem = GetComponent<HealthSystem>();
        _controller = GetComponent<Controller>();
        _rigidbody = GetComponent<Rigidbody>();

        //_targetSystem.Targets = GameObject.FindWithTag("GameController").GetComponent<GameControllerDEBUG>().Targets;
        
        Camera = _controller.camera;
    }
}
