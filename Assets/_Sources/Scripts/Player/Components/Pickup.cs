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
    [SerializeField] private float catchDistance = 4f;
    [CanBeNull] public Transform ballTransform;
    public Ball ball;
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
            if (ball == null) ball = ballTransform.GetComponent<Ball>();
            if (ball._ballState == Ball.BallStateRefab.Picked)
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
    public void CmdChangeBallState(Ball _ball, Ball.BallStateRefab _ballStateRefab)
    {
        RpcChangeBallState(_ball, _ballStateRefab);
    }
    [ClientRpc]
    private void RpcChangeBallState(Ball _ball, Ball.BallStateRefab _ballStateRefab)
    {
        _ball.ChangeBallState(_ballStateRefab, _player);
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
            if (col.TryGetComponent(out Ball ballRefab))
            {
                //If it's a pass
                if (ballRefab._ballState == Ball.BallStateRefab.Pass
                    && !_player.IsHoldingObject
                    && ballRefab.owner.teamId == _player.teamId)
                {
                    ballTransform = col.transform;
                    ball = ballRefab;
                    ballRefab.collider.enabled = false;
                    ball._ballState = Ball.BallStateRefab.Picked;
                    CmdChangeBallState(ballRefab, Ball.BallStateRefab.Picked);
                    _player.ChangeBallLayer(ballRefab.gameObject, true);
                    return;
                }

                //Pick if pickable
                if (ballRefab._ballState != Ball.BallStateRefab.Free ||
                    ballRefab.rb.velocity.magnitude > maxVelToPickup ||
                    _player.IsHoldingObject) return;
                
                ballTransform = col.transform;
                ball = ballRefab;
                ballRefab.collider.enabled = false;
                ball._ballState = Ball.BallStateRefab.Picked;
                CmdChangeBallState(ballRefab, Ball.BallStateRefab.Picked);
                _player.ChangeBallLayer(ballRefab.gameObject, true);
                
                print ("Ball picked :: " + name);
            }
        }
    }

    public void TryToCatch()
    {
        RaycastHit[] hits = Physics.SphereCastAll(_player.Camera.transform.position, catchDistance, 
            _player.Camera.transform.forward, catchDistance); // Get all objects in range
        if (hits.Length > 0) // If the list of object is not empty
        {
            Ball[] balls =
                hits.Where(hit => hit.collider.GetComponent<Ball>() != null)
                    .Select(hit => hit.collider.GetComponent<Ball>())
                    .ToArray(); //Take only pickable objects

            balls = balls.Where(ball =>
                Utils.IsVisibleByCamera(ball.transform.position, _player.Camera) &&
                (ball._ballState != Ball.BallStateRefab.Free && ball._ballState != Ball.BallStateRefab.Picked)
                && ball.owner != _player).ToArray(); // Take only visible objects annd not picked

            balls = balls.OrderBy(ball =>
                    Vector3.Distance(ball.transform.position, _player.Camera.transform.position))
                .ToArray(); // Take the closest object

            if (balls.Length > 0) // If there is at least one object in range
            {
                Ball closestBall = balls[0]; // Take the closest object
                
                ball = closestBall;
                ballTransform = closestBall.transform;
                closestBall.collider.enabled = false;
                ball.rb.velocity = Vector3.zero;
                ball._ballState = Ball.BallStateRefab.Picked;
                CmdChangeBallState(closestBall, Ball.BallStateRefab.Picked);
                _player.ChangeBallLayer(closestBall.gameObject, true);
                
                print ("Ball catched :: " + name);
            }
        }
    }
}