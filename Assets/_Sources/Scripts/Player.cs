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

    
    private void Update()
    {/*
        if (targeter.CurrentTarget != null && debug)
        {
            target = targeter.CurrentTarget.transform;

            Vector3 VtoTarget = target.position - cam.transform.position;
            float d = Vector3.Distance(cam.transform.position, target.position) / 2;
            float rAngle = Vector3.Angle(VtoTarget, cam.transform.forward);
            float h = Mathf.Abs(d * (1 / Mathf.Cos(Mathf.Deg2Rad * rAngle)));

            angle = cam.transform.position + cam.transform.forward * h;
            Vector3 midPoint = target.position - ((target.position - cam.transform.position) * 0.5f);
            Vector3 o = (angle - midPoint);
            
            
            Debug.DrawLine(cam.transform.position, target.position, Color.white);
            Debug.DrawLine(cam.transform.position, angle, Color.blue);
            Debug.DrawLine(midPoint, angle, Color.magenta);
            Debug.DrawRay(midPoint, Vector3.Cross(VtoTarget, o), Color.red);

        }*/
    }
}
