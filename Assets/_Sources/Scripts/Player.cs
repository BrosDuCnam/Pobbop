using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(NewController))]
public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] public TargetSystem TargetSystem;
    [SerializeField] public PickUpDropSystem PickUpDropSystem;
    
    [Header("Throw variables")]
    [SerializeField] private float _minForce = 10;
    [SerializeField] private float _maxForce = 35;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeFactor;

    [SerializeField] public bool DEBUG = true;

    //private Ball _pickDropSystem.PickableObject;
    //private Targeter targeter;

    private Transform target;
    private Vector3 angle;
    public Camera Camera;

    private float startCharge;

    void Start()
    {
        Camera = Camera.main; //TODO: pas opti pour le moment

        TargetSystem = GetComponent<TargetSystem>();
        PickUpDropSystem = GetComponent<PickUpDropSystem>();
    }


    public void ThrowInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            startCharge = Time.time;

        if (ctx.canceled)
        {
            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - startCharge, 0, _maxChargeTime);
            chargeTime = _chargeFactor.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float multiplier = (_maxForce - _minForce) / _maxChargeTime;
            float force = multiplier * chargeTime + _minForce;

            Debug.Log("force = " + force);

            if (PickUpDropSystem.PickableObject != null && PickUpDropSystem.PickableObject.GetComponent<ThrowableObject>())
            {
                ThrowableObject throwableObject = PickUpDropSystem.PickableObject.GetComponent<ThrowableObject>();
                if (TargetSystem.CurrentTarget != null)
                {
                    target = TargetSystem.CurrentTarget.transform;
                
                    throwableObject.Throw(Camera.transform.forward  * 10 + Camera.transform.position, target, force);
                    PickUpDropSystem.PickableObject = null;
                    
                    /*
                    //Not so complicated trigonometry to find the angle based on the distance between thrower and target, and the looking angel
                    //I made a doc in Assets/Doc/ThrowAngle for if you need
                    //IMPORTANT : Mathf functions for trigonometry takes radians so don't forget to convert
                    Vector3 VtoTarget = target.position - camera.transform.position; //Vector between player and target
                    float d = Vector3.Distance(camera.transform.position, target.position) / 2; //Half distance between player and target
                    float rAngle = Vector3.Angle(VtoTarget, camera.transform.forward);
                    float h = Mathf.Abs(d * (1 / Mathf.Cos(Mathf.Deg2Rad * rAngle))); //Distance from player to bezier mid point

                    angle = camera.transform.position + camera.transform.forward * h;
                    Vector3 midPoint = target.position - ((target.position - camera.transform.position) * 0.5f); //Position between player and target
                    Vector3 o = (angle - midPoint); 
                    Vector3 torquePivot = Vector3.Cross(VtoTarget, o).normalized; //Axis for the ball to turn around 

                    _pickUpDropSystem.PickableObject.ThrowToTarget(_pickUpDropSystem.PickableObject.transform.position, target, angle, force, 0.8f);
                    _pickUpDropSystem.PickableObject.TorqueAround(torquePivot, 1000);*/
                }
                else
                {
                    //_pickUpDropSystem.PickableObject.Throw(50, camera.transform.forward);
                }
            }
        }
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
