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
    [SerializeField] public Transform pickupPoint;
    [SerializeField] private float maxVelToPickup = 5f;
    [SerializeField] private float cooldownTime = 0.4f;
    [CanBeNull] public Transform ballTransform;
    public BallRefab ball;
    private Player _player;
    [HideInInspector] public bool cooldown;
    
    private void Start()
    {
        _player = GetComponent<Player>();
        ballTransform = null;
    }

    private void Update()
    {
        if (ballTransform != null)
        {
            if (ball == null) ball = ballTransform.GetComponent<BallRefab>();
            if (ball._ballState == BallRefab.BallStateRefab.Picked)
            {
                ballTransform.position = pickupPoint.position;
                CmdMoveBall(ballTransform, pickupPoint.position);
            }
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
        _ballRefab.ChangeBallState(_ballStateRefab, _player);
        _ballRefab.rb.isKinematic = _ballStateRefab != BallRefab.BallStateRefab.Free;
    }

    public void Throw()
    {
        ballTransform = null;
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
                //Pick if pickable
                if (ballRefab._ballState != BallRefab.BallStateRefab.Free ||
                    ballRefab.rb.velocity.magnitude > maxVelToPickup ||
                    _player.IsHoldingObject) return;
                
                ballTransform = col.transform;
                ball = ballRefab;
                ballRefab.collider.enabled = false;
                CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.Picked);
                _player.ChangeBallLayer(ballRefab.gameObject, true);
                
                print ("Ball picked :: " + name);
            }
        }
    }
}
