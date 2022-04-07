using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Pickup : NetworkBehaviour
{
    public Transform ball;
    private bool move;

    private void Start()
    {
        ball = GameObject.Find("Cube").transform;
    }

    private void Update()
    {
        if (move)
        {
            CmdMoveBall();
        }
    }
    
    [Command]
    private void CmdMoveBall()
    {
        MoveBall();
    }

    [ClientRpc]
    private void MoveBall()
    {
        ball.position += ball.transform.forward * Time.deltaTime * 10;

    }

    public void InputClick(InputAction.CallbackContext ctx)
    {
        move = ctx.performed;
    }
}
