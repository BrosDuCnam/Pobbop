using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Ball : NetworkBehaviour
{
    public Player owner;
    public Rigidbody rb;
    public Collider collider; 
    private Pickup localPickup;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        localPickup = NetworkClient.localPlayer.GetComponent<Pickup>();
    }

    public enum BallStateRefab
    {
        Free,
        Picked,
        Curve,
        FreeThrow,
        Pass
    }

    public BallStateRefab _ballState = BallStateRefab.Free;

    public void ChangeBallState(BallStateRefab ballState, Player _owner = null)
    {
        _ballState = ballState;
        owner = _owner;
        if (ballState == BallStateRefab.Picked)
        {
            rb.isKinematic = true;
        }
        else if (ballState == BallStateRefab.Free)
        {
            owner = null;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.TryGetComponent(out Player player))
        {
            LetPlayerDie(player);
        }
        else
        {
            if (_ballState == BallStateRefab.Picked) return;
            
            if (_ballState == BallStateRefab.Curve)
                print("collision with " + col.gameObject.name);
            localPickup.GetComponent<Pickup>().CmdChangeBallState(this, BallStateRefab.Free);
        }
    }

    [ServerCallback]
    private void LetPlayerDie(Player player)
    {
        if (_ballState == BallStateRefab.Picked || _ballState == BallStateRefab.Free || player.teamId == owner.teamId) return;

        RpcDie(player, this);
            
        RpcChangeBallState(BallStateRefab.Free);
        RpcChangeOwner(null);
    }


    [ClientRpc]
    private void RpcDie(Player player, Ball ball)
    {
        player.Die(ball);
    }

    
    [ClientRpc]
    private void RpcChangeBallState(BallStateRefab ballState)
    {
        _ballState = ballState;
    }
    
    [Command]
    private void CmdChangeOwner(Player _owner)
    {
        RpcChangeOwner(_owner);
    }
    
    [ClientRpc]
    private void RpcChangeOwner(Player _owner)
    {
        owner = _owner;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        GUILayout.Label("Ball State: " + _ballState, style);
        GUILayout.Label("Owner: " + owner, style);
        GUILayout.Label("Ve: " + rb.velocity.magnitude, style);
    }
}
