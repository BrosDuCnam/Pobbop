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
    [SerializeField] private AnimationCurve _passCurve;
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
    public float ChargeValue
    {
        get
        {
            if (!IsCharging) return 0;
            
            float chargeTime = Time.time - _startChargeTime;
            float chargeValue = chargeTime / _maxChargeTime;
            
            return Mathf.Clamp(chargeValue, 0, 1);
        }
    }
    
    void Start()
    {
        _player = GetComponent<Player>();
    }

    #region Throw

    public void ChargeThrow()
    {
        if (_player.IsHoldingObject)
        {
            ball = _player.GetBall();
            _startChargeTime = Time.time;
            IsCharging = true;
            _player._controller.IsThrowing = true;
        }
    }

    private float GetNormalizedForce(float force)
    {
        return (force - _minThrowForce) / (_maxThrowForce - _minThrowForce);
    }

    public void ReleaseThrow()
    {
        if (ball != null && IsCharging)
        {
            _player._controller.IsThrowing = false;
            _player._pickup.Throw();
            Ball ball = this.ball.GetComponent<Ball>();
            ball.collider.enabled = true;
            CmdSetKinematic(ball, false);
            _player._pickup.CmdChangeBallState(ball, Ball.BallStateRefab.Free);
            _player.ChangeBallLayer(ball.gameObject, false);

            //Counts the charging time of the ball and converts it to the fore applied
            float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
            float chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
            float fMultiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
            float force = fMultiplier * chargeValue + _minThrowForce;

            this.ball = null;
            IsCharging = false;
            
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
                
                CmdThrowBall(ball, _player, stepPosition, targetPointTransform, force, accuracy, _speedCurve, ThrowState.Thrown); // Throw the object
            }
            else
            {
                //Simple throw
                print("simple throw");
                Vector3 direction = _player.playerCam.transform.forward;
                Vector3 velocity = direction * force;
                CmdSetKinematic(ball, false);
                CmdChangeBallState(ball, Ball.BallStateRefab.FreeThrow, _player);
                CmdSimpleThrowBall(ball, velocity); // Throw the object in front of the camera
            }
            
        }
    }

    [Command]
    private void CmdSimpleThrowBall(Ball ball, Vector3 velocity)
    {
        print("simple throw");
        ball.rb.velocity = velocity;
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
    public void CmdThrowBall(Ball ball, Player throwingPlayer , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve, ThrowState state)
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
    private IEnumerator ThrowEnumerator(Ball ball, Player throwingPlayer, Vector3 step, Transform target, float speed, float accuracy,
        AnimationCurve curve, ThrowState state = ThrowState.Thrown)
    {
        ball.owner = throwingPlayer;
        CmdChangeBallState(ball, Ball.BallStateRefab.Curve, _player);

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

        Vector3 direction = (target.position - step);
        Vector3 lastPos = origin;

        while (time < 1 && ball._ballState != Ball.BallStateRefab.Free)
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
            direction /= Time.fixedDeltaTime;
            lastPos = nextPos;            

            yield return null;
        }

        if (ball._ballState != Ball.BallStateRefab.Picked)
        {
            CmdSetKinematic(ball, false);
            CmdChangeBallState(ball, Ball.BallStateRefab.FreeThrow, _player);
        }   

        ball.rb.velocity = direction;
    }
    
    /// <summary>
    /// Cancel throw and cancel charge
    /// </summary>
    public void CancelThrow()
    {
        if (IsCharging) IsCharging = false;
    }

    [Command] 
    private void CmdMoveBall(Ball ball, Vector3 nextPos)
    {
        ball.transform.position = ball.rb.position;
        ball.rb.MovePosition(nextPos);
    }

    [Command]
    public void CmdChangeBallState(Ball _ball, Ball.BallStateRefab _ballStateRefab, Player player)
    {
        RpcChangeBallState(_ball, _ballStateRefab, player);
    }
    [ClientRpc]
    private void RpcChangeBallState(Ball _ball, Ball.BallStateRefab _ballStateRefab, Player player)
    {
        if (_ballStateRefab != Ball.BallStateRefab.Free)
            _ball.ChangeBallState(_ballStateRefab, player);
        else 
            _ball.ChangeBallState(_ballStateRefab);
        
        _ball.rb.isKinematic = _ballStateRefab != Ball.BallStateRefab.Free && _ballStateRefab != Ball.BallStateRefab.FreeThrow;
    }
    
    [Command]
    private void CmdSetKinematic(Ball ball, bool isKinematic)
    {
        RpcSetKinematic(ball, isKinematic);
    }
    [ClientRpc]
    private void RpcSetKinematic(Ball ball, bool isKinematic)
    {
        ball.rb.isKinematic = isKinematic;
    }
    
    [Command]
    private void CmdUpdateVelocity(Ball ball, Vector3 velocity)
    {
        RpcUpdateVelocity(ball, velocity);
    }
    [ClientRpc]
    private void RpcUpdateVelocity(Ball ball, Vector3 velocity)
    {
        ball.rb.velocity = velocity;
    }


    #endregion
    
    
    #region Pass

    public void Pass()
    {
        GameObject target = _player.GetDesiredFriendly();
        this.ball = _player.GetBall();
        if (target == null || this.ball == null) return; 

        _player._controller.IsThrowing = false;
        _player._pickup.Throw();
        Ball ball = this.ball.GetComponent<Ball>();
        ball.collider.enabled = true;
        _player._pickup.CmdChangeBallState(ball, Ball.BallStateRefab.Pass);
        _player.ChangeBallLayer(ball.gameObject, false);
        CmdSetKinematic(ball, true);
            
        this.ball = null;
        IsCharging = false;
        _player.IsHoldingObject = false;

        Transform targetPointTransform = target.transform;
        targetPointTransform = target.GetComponent<Player>().targetPoint;
        
        StartCoroutine(PassEnumerator(ball, _player, targetPointTransform, 20f, _passCurve));
    }
    
    private IEnumerator PassEnumerator(Ball ball, Player throwingPlayer, Transform target, float speed, AnimationCurve curve)
    {
        ball.owner = throwingPlayer;
        ball._ballState = Ball.BallStateRefab.Pass;
        CmdChangeBallState(ball, Ball.BallStateRefab.Pass, _player);

        //Vector3 origin = transform.position;
        Vector3 origin = _player._pickup.pickupPoint.position;
        float time = 0;
        Vector3 direction = Vector3.zero;
        Vector3 lastPos = origin;
        print(ball._ballState == Ball.BallStateRefab.Pass);
        while (ball._ballState == Ball.BallStateRefab.Pass)
        {
            Vector3 targetPos = target.position;
            Vector3 nextPos = ball.rb.position + (targetPos - ball.rb.position).normalized * speed / 50;
            ball.rb.MovePosition(nextPos);
            CmdMoveBall(ball, nextPos);
            CmdUpdateVelocity(ball, ball.rb.velocity);
            //ball.transform.position = ball.rb.position;

            new WaitForFixedUpdate();
            time += Time.fixedDeltaTime * speed / 50;
            
            direction = nextPos - lastPos;
            direction /= Time.fixedDeltaTime;
            lastPos = nextPos;    
            
            yield return null;
        }

        if (ball._ballState != Ball.BallStateRefab.Picked)
        {
            CmdSetKinematic(ball, false);
            CmdChangeBallState(ball, Ball.BallStateRefab.FreeThrow, _player);
        
            ball.rb.velocity = direction;
        }
    }

    #endregion
}

/// <summary>
/// Enum to define the state of the throw.
/// </summary>
public enum ThrowState
{
    /// <summary>
    /// The throw is idle.
    /// </summary>
    Idle,
    /// <summary>
    /// The throw is in progress.
    /// </summary>
    Thrown,
    /// <summary>
    /// The throw is in progress and the object is rebounding.
    /// </summary>
    Rebound,
    /// <summary>
    /// The object is throw without curve
    /// </summary>
    FreeThrow
}