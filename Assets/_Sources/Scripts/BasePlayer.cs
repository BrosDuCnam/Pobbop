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
    [SerializeField] public TargetSystem targetSystem;
    [SerializeField] public PickUpDropSystem pickUpDropSystem;
    [SerializeField] public ThrowSystem throwSystem;
    [SerializeField] public HealthSystem healthSystem;
    [SerializeField] public Rigidbody rigidbody;
    [SerializeField] public Controller controller;
    
    public Camera Camera;

    /// <summary>
    /// Return true if the player hold an object
    /// </summary>
    public bool IsHoldingObject
    {
        get => pickUpDropSystem.PickableObject != null;
        set {
            if (!value) pickUpDropSystem.PickableObject = null;
        }
    }
    
    /// <summary>
    /// Return the current holding object, null if the player is not holding an object
    /// </summary>
    [CanBeNull] public GameObject HoldingObject
    {
        get
        {
            if (pickUpDropSystem.PickableObject == null) return null;
            return pickUpDropSystem.PickableObject.gameObject; 
        }
    }
    
    /// <summary>
    /// Return true if the player has a target, can be use to know if the player can see an enemy
    /// </summary>
    public bool HasTarget
    {
        get => targetSystem.CurrentTarget != null;
    }
    
    /// <summary>
    /// Return the current target, null if the player has no target
    /// </summary>
    [CanBeNull] public GameObject Target
    {
        get => targetSystem.CurrentTarget.gameObject;
    }
    
    /// <summary>
    /// Return true if the player is charging an attack
    /// </summary>
    public bool IsCharging
    {
        get => throwSystem.IsCharging;
    }
    
    /// <summary>
    /// Cancel the ball charge if the player is charging
    /// </summary>
    /// <example>Use when sprinting</example>>
    public void CancelCharge()
    {
        throwSystem.CancelThrow();
    }

    [Command]
    public void AssignAuthority(GameObject obj)
    {
        obj.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
    }

    [Command]
    public void RemoveAuthority(GameObject obj)
    {
        obj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }
    
    protected void Awake()
    {
        targetSystem = GetComponent<TargetSystem>();
        pickUpDropSystem = GetComponent<PickUpDropSystem>();
        throwSystem = GetComponent<ThrowSystem>();
        healthSystem = GetComponent<HealthSystem>();
        controller = GetComponent<Controller>();
        rigidbody = GetComponent<Rigidbody>();

        //_targetSystem.Targets = GameObject.FindWithTag("GameController").GetComponent<GameControllerDEBUG>().Targets;
        
        Camera = controller.camera;
    }
}
