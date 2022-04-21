using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Throw : NetworkBehaviour
{
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] private float _maxChargeTime = 5;
    [SerializeField] private AnimationCurve _chargeCurve;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] public float minStepMultiplier = 2f;
    [SerializeField] public float maxStepMultiplier = 30f;
    [SerializeField] private bool _drawCurve = true;

    [SerializeField] private bool DEBUG;


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
        if (_player.IsHoldingObject)
        {
            ball = _player.GetBall();
            _startChargeTime = Time.time;
            IsCharging = true;
        }
    }
    
    private float GetNormalizedForce(float force)
    {
        return (force - _minThrowForce) / (_maxThrowForce - _minThrowForce);
    }

    public void ReleaseThrow()
    {
        if (ball != null)
        {
            _player._pickup.Throw();
            BallRefab ballRefab = ball.GetComponent<BallRefab>();
            CmdSetKinematic(ballRefab, false);
            _player._pickup.CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.Free);
            _player.ChangeBallLayer(ballRefab.gameObject, false);

            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float fMultiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = fMultiplier * chargeValue + _minThrowForce;

            ball = null;
            IsCharging = false;
            if (true) // If player hold an object
            {
                Transform ball = _player.GetBall();
                
                _player.IsHoldingObject = false;
                if (_player.HasTarget) // If player has a target
                {
                    GameObject target = _player.Target;
                    
                    Transform targetPointTransform = target.transform;
                    targetPointTransform = target.GetComponent<Player>().targetPoint;

                    // Calculate the multiplier of step distance
                    float multiplier = Mathf.Pow(1.5f, _player._controller.rb.velocity.magnitude);
                    multiplier += Mathf.Pow(50f, GetNormalizedForce(force));
                    multiplier = Mathf.Clamp(multiplier, minStepMultiplier, maxStepMultiplier);

                    Vector3 stepPosition = (_player.playerCam.transform.forward * multiplier) + _player.playerCam.transform.position;

                    //float accuracy = ChargeValue; // TODO - Maybe need to calculate the accuracy in other way
                    float accuracy = 1; // TODO - Maybe need to calculate the accuracy in other way
                    
                    CmdThrowBall(ballRefab, _player, stepPosition, targetPointTransform, force, accuracy, _speedCurve, ThrowState.Thrown); // Throw the object
                }
                else
                {
                    //Simple throw
                    print("simple throw");
                    Vector3 direction = _player.playerCam.transform.forward;
                    Vector3 velocity = direction * force;
                    CmdSetKinematic(ballRefab, false);
                    CmdChangeBallState(ballRefab, BallRefab.BallStateRefab.FreeThrow, _player);
                    CmdSimpleThrowBall(ballRefab, velocity); // Throw the object in front of the camera
                }
            }
        }
    }

    [Command]
    private void CmdSimpleThrowBall(BallRefab ballRefab, Vector3 velocity)
    {
        print("simple throw");
        ballRefab.rb.velocity = velocity;
    }
    
    /// <summary>
    /// Function to throw the object.
    /// </summary>
    /// <param name="player">The player who throw the object</param>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="accuracy">The accuracy of throw (ex: 1 = object finish path on the target, 0.5 = object finish path between first position and now position of the target</param>
    /// <param name="curve">The curve of speed during time</param>
    /// <param name="state">The future state of the object</param>
    public void CmdThrowBall(BallRefab ball, Player throwingPlayer , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve, ThrowState state)
    {
        StartCoroutine(ThrowEnumerator(ball, throwingPlayer, step, target, speed, accuracy, curve, state));
    }

    public float dbgTime = 0;
    /// <summary>
    /// Enumerator to throw the object during time.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="step">The position step of bezier curve</param>
    /// <param name="target">The transform of the target</param>
    /// <param name="speed">The speed in meter/s</param>
    /// <param name="accuracy">The accuracy of throw (ex: 1 = object finish path on the target, 0.5 = object finish path between first position and now position of the target</param>
    /// <param name="curve">The curve of speed during time</param>
    /// <param name="state">The future state of the object</param>
    private IEnumerator ThrowEnumerator(BallRefab ball, Player throwingPlayer, Vector3 step, Transform target, float speed, float accuracy,
        AnimationCurve curve, ThrowState state = ThrowState.Thrown)
    {
        ball.owner = throwingPlayer;
        CmdChangeBallState(ball, BallRefab.BallStateRefab.Curve, _player);

        //Vector3 origin = transform.position;
        Vector3 origin = _player._pickup.pickupPoint.position;
        Vector3 originTargetPosition = target.position;
        Vector3 torqueDirection = -Vector3.Cross(origin - step, Vector3.up).normalized;

        if (DEBUG)
        {
            Utils.DebugBezierCurve(origin, step, target.position, 10, Color.red, 5);
        }

        float time = 0;
        float distance = Utils.BezierCurveDistance(origin, step, target.position, 10);
        float i = 1 / (distance / speed);
        float lastDeltaTime = 0;

        Vector3 direction = (target.position - step);
        Vector3 lastPos = origin;

        while (time < 1 && ball._ballState != BallRefab.BallStateRefab.Free)
        {
            Vector3 targetPos = Vector3.Lerp(originTargetPosition, target.position, accuracy);
            Vector3 nextPos = Utils.BezierCurve(origin, step, targetPos, time);
            ball.rb.MovePosition(nextPos);
            CmdMoveBall(ball, nextPos);
            CmdUpdateVelocity(ball, ball.rb.velocity);
            //ball.rb.AddTorque(torqueDirection * Mathf.Pow(100, time));

            new WaitForFixedUpdate();
            time += (i * (curve.Evaluate(time * 3))) * Time.deltaTime; // TODO - Hacky fix for curve
            time += Time.deltaTime; // TODO : Remove this line and use the line above
            
            direction = nextPos - lastPos;
            direction /= lastDeltaTime;
            lastDeltaTime = Time.deltaTime;
            lastPos = nextPos;            

            yield return null;
        }
        //When the ball collides with something, we set the state to FreeThrow
        CmdSetKinematic(ball, false);
        CmdChangeBallState(ball, BallRefab.BallStateRefab.FreeThrow, _player);
        
        ball.rb.velocity = direction;
    }

    [Command]
    private void CmdMoveBall(BallRefab ball, Vector3 nextPos)
    {
        ball.transform.position = ball.rb.position;
        ball.rb.MovePosition(nextPos);
    }



    [Command]
    public void CmdChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab, Player player)
    {
        RpcChangeBallState(_ballRefab, _ballStateRefab, player);
    }
    [ClientRpc]
    private void RpcChangeBallState(BallRefab _ballRefab, BallRefab.BallStateRefab _ballStateRefab, Player player)
    {
        if (_ballStateRefab != BallRefab.BallStateRefab.Free)
            _ballRefab.ChangeBallState(_ballStateRefab, player);
        else 
            _ballRefab.ChangeBallState(_ballStateRefab);
        
        _ballRefab.rb.isKinematic = _ballStateRefab != BallRefab.BallStateRefab.Free && _ballStateRefab != BallRefab.BallStateRefab.FreeThrow;
    }
    
    [Command]
    private void CmdSetKinematic(BallRefab ball, bool isKinematic)
    {
        RpcSetKinematic(ball, isKinematic);
    }
    [ClientRpc]
    private void RpcSetKinematic(BallRefab ball, bool isKinematic)
    {
        ball.rb.isKinematic = isKinematic;
    }
    
    [Command]
    private void CmdUpdateVelocity(BallRefab ball, Vector3 velocity)
    {
        RpcUpdateVelocity(ball, velocity);
    }
    [ClientRpc]
    private void RpcUpdateVelocity(BallRefab ball, Vector3 velocity)
    {
        ball.rb.velocity = velocity;
    }
}
