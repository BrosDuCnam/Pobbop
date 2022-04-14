using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private Transform pickupPoint;
    [SerializeField] private float maxVelToPickup = 5f;
    [SerializeField] private float cooldownTime = 0.4f;
    [CanBeNull] public Transform ball;
    private Player _player;
    private bool cooldown;
    
    private void Start()
    {
        _player = GetComponent<Player>();
    }

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
    }

    [Command]
    public void CmdChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab)
    {
        RpcChangeBallState(_ballRefab, _ballStateRefab);
    }
    [ClientRpc]
    private void RpcChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab)
    {
        _ballRefab.ChangeBallState(_ballStateRefab, gameObject);
        _ballRefab.rb.isKinematic = _ballStateRefab != BallRefab.BallStateRefab.Free;
    }

    public void Throw()
    {
        ball = null;
        cooldown = true;
        StartCoroutine(ResetCooldownCoroutine());
    }

    private IEnumerator ResetCooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        cooldown = false;
    }
    
    private void OnTriggerEnter(Collider col)
    {
        if (!enabled) return;
        if (cooldown) return;
        if (col.CompareTag("Ball"))
        {
            if (col.TryGetComponent(out BallRefab ballRefab))
            {
                if (ballRefab.rb.velocity.magnitude > maxVelToPickup) return;
                //Pick if pickable
                if (ballRefab._ballState != BallRefab.BallStateRefab.Free) return;
                ball = col.transform;
                CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.Picked);
                
                print ("Ball picked :: " + name);
            }
        }
    }
}
