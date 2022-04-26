using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BallRefab : NetworkBehaviour
{
    public Player owner;
    public Rigidbody rb;
    public Collider collider; 

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    public enum BallStateRefab
    {
        Free,
        Picked,
        Curve,
        FreeThrow
    }

    public BallStateRefab _ballState = BallStateRefab.Free;

    public void ChangeBallState(BallStateRefab ballState, Player _owner = null)
    {
        _ballState = ballState;
        owner = _owner;
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.TryGetComponent(out Player player))
        {
            if (_ballState == BallStateRefab.Picked || _ballState == BallStateRefab.Free || player == owner) return;
            RpcDie(player);
        }
        RpcChangeBallState(BallStateRefab.Free);
        RpcChangeOwner(null);

    }

    [ClientRpc]
    private void RpcDie(Player player)
    {
        player.Die();
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
