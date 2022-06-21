using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;

public class Ball : NetworkBehaviour
{
    public Player owner;
    public Rigidbody rb;
    public Collider collider; 
    private RealPlayer localPlayer;
    [SerializeField] private Renderer coreRenderer;
    [SyncVar] [SerializeField] private Color FreeColor;
    [SyncVar] [SerializeField] private Color PickedColor;
    [SyncVar] [SerializeField] private Color CurveColor;
    [SyncVar] [SerializeField] private Color FreeThrowColor;
    [SyncVar] [SerializeField] private Color PassColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        localPlayer = NetworkClient.localPlayer.GetComponent<RealPlayer>();
        GetComponent<Renderer>().material = GetComponent<Renderer>().material;
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

        ChangeMaterialColor(ballState);
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
            _ballState = BallStateRefab.Free;
            localPlayer._pickup.GetComponent<Pickup>().CmdChangeBallState(this, BallStateRefab.Free);
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
        ChangeMaterialColor(ballState);
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

    private void ChangeMaterialColor(BallStateRefab state)
    {
        switch (state)
        {
            case BallStateRefab.Free:
                coreRenderer.material.color = FreeColor;
                break;
            case BallStateRefab.FreeThrow:
                coreRenderer.material.color = FreeThrowColor;
                break;
            case BallStateRefab.Curve:
                coreRenderer.material.color = CurveColor;
                break;
            case BallStateRefab.Picked:
                coreRenderer.material.color = PickedColor;
                break;
            case BallStateRefab.Pass:
                coreRenderer.material.color = PassColor;
                break;
        }
    }

    // private void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 40;
    //     GUILayout.Label("Ball State: " + _ballState, style);
    //     GUILayout.Label("Owner: " + owner, style);
    //     GUILayout.Label("Ve: " + new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude, style);
    // }
}
