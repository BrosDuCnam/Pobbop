using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private Transform pickupPoint;

    [CanBeNull] private Transform ball;
    
    private bool move;

    private void Update()
    {
        if (ball != null)
        {
            ball.position = pickupPoint.position;
            CmdMoveBall(ball, pickupPoint.position);
        }
    }

    [Command]
    private void CmdMoveBall(Transform ball, Vector3 position)
    {
        ball.position = position;
        if (isServer)
        {
            RpcMoveBall(ball, position);
        }
    }
    [ClientRpc]
    private void RpcMoveBall(Transform ball, Vector3 position)
    {
        ball.position = position;
    }
    

    [Command]
    private void CmdChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab)
    {
        RpcChangeBallState(_ballRefab, _ballStateRefab);
    }
    [ClientRpc]
    private void RpcChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab)
    {
        _ballRefab.ChangeBallState(_ballStateRefab);
        _ballRefab.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!enabled) return;
        if (col.CompareTag("Ball"))
        {
            if (col.TryGetComponent(out BallRefab ballRefab))
            {
                //Pick if pickable
                if (ballRefab._ballState != BallRefab.BallStateRefab.Free) return;
                ball = col.transform;
                CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.Picked);
                
                print ("Ball picked :: " + name);
            }
        }
    }

    public void InputClick(InputAction.CallbackContext ctx)
    {
        move = ctx.performed;
    }
}
