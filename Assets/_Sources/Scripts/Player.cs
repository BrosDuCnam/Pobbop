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
    [SerializeField] private float minForce = 10;
    [SerializeField] private float maxForce = 35;
    [SerializeField] private float maxChargeTime = 5;
    [SerializeField] private AnimationCurve chargeFactor;
    [SerializeField] private bool debug = true;

    //private Ball currentBall;
    //private Targeter targeter;

    private Transform target;
    private Vector3 angle;
    public Camera Camera;

    private float startCharge;

    void Start()
    {
        //targeter = GetComponent<Targeter>();
        Camera = Camera.main; //TODO: pas opti pour le moment
    }
    /*
    public void changeBall(Ball ball)
    {
        currentBall = ball;
    }*/

    public void ThrowInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            startCharge = Time.time;

        if (ctx.canceled)
        {
            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - startCharge, 0, maxChargeTime);
            chargeTime = chargeFactor.Evaluate(chargeTime / maxChargeTime) * maxChargeTime;
            float multiplier = (maxForce - minForce) / maxChargeTime;
            float force = multiplier * chargeTime + minForce;

            Debug.Log("force = " + force);
/*
            if (currentBall != null)
            {
                Debug.Log(currentBall.BallState);
                if (targeter.CurrentTarget != null)
                {
                    target = targeter.CurrentTarget.transform;
                
                    //Not so complicated trigonometry to find the angle based on the distance between thrower and target, and the looking angel
                    //I made a doc in Assets/Doc/ThrowAngle for if you need
                    //IMPORTANT : Mathf functions for trigonometry takes radians so don't forget to convert
                    Vector3 VtoTarget = target.position - cam.transform.position; //Vector between player and target
                    float d = Vector3.Distance(cam.transform.position, target.position) / 2; //Half distance between player and target
                    float rAngle = Vector3.Angle(VtoTarget, cam.transform.forward);
                    float h = Mathf.Abs(d * (1 / Mathf.Cos(Mathf.Deg2Rad * rAngle))); //Distance from player to bezier mid point

                    angle = cam.transform.position + cam.transform.forward * h;
                    Vector3 midPoint = target.position - ((target.position - cam.transform.position) * 0.5f); //Position between player and target
                    Vector3 o = (angle - midPoint); 
                    Vector3 torquePivot = Vector3.Cross(VtoTarget, o).normalized; //Axis for the ball to turn around 

                    currentBall.ThrowToTarget(currentBall.transform.position, target, angle, force, 0.8f);
                    currentBall.TorqueAround(torquePivot, 1000);
                }
                else
                {
                    currentBall.Throw(50, cam.transform.forward);
                }
            }*/
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
