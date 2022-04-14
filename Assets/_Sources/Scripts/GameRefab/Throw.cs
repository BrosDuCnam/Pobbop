using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Throw : NetworkBehaviour
{
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeCurve;


    private Player _player;
    private float _startChargeTime;
    private Transform ball;

    private bool _isCharging;

    public bool IsCharging
    {
        get => _isCharging;
        private set => _isCharging = value;
    }
    
    void Start()
    {
        _player = GetComponent<Player>();
    }

    public void ChargeThrow()
    {
        if (_player.GetBall() != null)
        {
            ball = _player.GetBall();
            _startChargeTime = Time.time;
            IsCharging = true;
        }
    }

    public void ReleaseThrow()
    {
        if (ball != null)
        {
            _player._pickup.Throw();
            BallRefab ballRefab = ball.GetComponent<BallRefab>();
            ballRefab.rb.isKinematic = false;
            _player._pickup.CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.Free);

            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float multiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = multiplier * chargeValue + _minThrowForce;

            CmdReleaseThrow(ballRefab, force);
            ball = null;
            IsCharging = false;
        }
    }

    [Command]
    private void CmdReleaseThrow(BallRefab ballRefab, float force)
    {
        ballRefab.rb.isKinematic = false;
        ballRefab.rb.AddForce(_player.playerCam.transform.forward * force, ForceMode.Impulse);
    }
}
