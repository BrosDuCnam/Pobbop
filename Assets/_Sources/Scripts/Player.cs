using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Compression;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(NewController))]
public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TargetSystem _targetSystem;
    [SerializeField] private PickUpDropSystem _pickUpDropSystem;
    //private Ball _pickDropSystem.PickableObject;
    //private Targeter targeter;
    
    public Camera Camera;

    public bool IsHoldingObject
    {
        get => _pickUpDropSystem.PickableObject != null;
        set {
            if (!value) _pickUpDropSystem.PickableObject = null;
        }
    }
    [CanBeNull] public GameObject HoldingObject
    {
        get => _pickUpDropSystem.PickableObject.gameObject;
    }
    public bool HasTarget
    {
        get => _targetSystem.CurrentTarget != null;
    }
    [CanBeNull] public GameObject Target
    {
        get => _targetSystem.CurrentTarget.gameObject;
    }
    
    
    void Start()
    {
        Camera = Camera.main; //TODO: pas opti pour le moment

        _targetSystem = GetComponent<TargetSystem>();
        _pickUpDropSystem = GetComponent<PickUpDropSystem>();

        _targetSystem.Targets = GameObject.FindWithTag("GameController").GetComponent<GameControllerDEBUG>().Targets;
    }
    
}
