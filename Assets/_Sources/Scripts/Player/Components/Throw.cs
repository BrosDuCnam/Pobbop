using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Throw : NetworkBehaviour
{
    [SerializeField] private float _minThrowForce = 10f;
    [SerializeField] private float _maxThrowForce = 35f;
    [SerializeField] public float _maxChargeTime = 5;
    [SerializeField] public AnimationCurve _chargeCurve;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private AnimationCurve _passCurve;
    [SerializeField] public float minStepMultiplier = 2f;
    [SerializeField] public float maxStepMultiplier = 30f;
    [SerializeField] private bool _drawCurve = true;

    [SerializeField] private bool DEBUG;

    
    private bool _canDoThrowPath = true;
    
    GameObject tempFriendly;

    public UnityEvent OnThrow;
    public UnityEvent OnPass;

    private Player _player;
    [HideInInspector] public float _startChargeTime;
    public Transform ball;

    private bool _isCharging;

    public bool IsCharging
    {
        get => _isCharging;
        set => _isCharging = value;
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
        if (_player.IsHoldingObject && IsCharging == false)
        {
            ball = _player.GetBall();
            _startChargeTime = Time.time;
            IsCharging = true;
            _player._controller.IsThrowing = true;
            tempFriendly = _player.GetDesiredFriendly();
        }
    }

    private float GetNormalizedForce(float force)
    {
        return (force - _minThrowForce) / (_maxThrowForce - _minThrowForce);
    }

    public void ReleaseThrow(bool pass = false, float chargeFoce = -1, float accuracyValue = -1, GameObject targetObj = null)
    {
        if (!enabled) return;

        if (ball != null && IsCharging)
        {
            if (_player != null)
            {
                _player._controller.IsThrowing = false;
                _player._pickup.Throw();
            }
            Ball ball = this.ball.GetComponent<Ball>();
            print(ball.collider == null);
            ball.collider.enabled = true;
            CmdSetKinematic(ball, false);
            if (_player != null) _player.ChangeBallLayer(ball.gameObject, false);

            float force = chargeFoce;
            float chargeValue = 0;
            if (force == -1)
            {
                //Counts the charging time of the ball and converts it to the fore applied
                float chargeTime = Mathf.Clamp(Time.time - _startChargeTime, 0, _maxChargeTime);
                chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
                float fMultiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
                force = fMultiplier * chargeValue + _minThrowForce;
            }
            else
            {
                float chargeTime = Utils.Map(1, 0, _maxChargeTime, 0, force);
                chargeValue = _chargeCurve.Evaluate(chargeTime / _maxChargeTime) * _maxChargeTime;
                float fMultiplier = (_maxThrowForce - _minThrowForce) / _maxChargeTime;
                force = fMultiplier * chargeValue + _minThrowForce;
            }
            
            this.ball = null;
            IsCharging = false;
            
            if (_player != null) _player.IsHoldingObject = false;
            GameObject friendly = tempFriendly; 
            if (_player != null) friendly = tempFriendly == null ? _player.GetDesiredFriendly() : tempFriendly;
            tempFriendly = null;
            if (targetObj != null || (pass && friendly != null) || (!pass && _player.HasTarget)) // If player has a target
            {
                GameObject target = targetObj;
                if (target == null)
                {
                    target = pass ? friendly : _player.Target;
                }


                Transform targetPointTransform = target.transform;
                if (target.TryGetComponent(out Player targetPlayer))
                    targetPointTransform = targetPlayer.targetPoint;
                
                if (!pass && targetPlayer != null)
                    CmdWarnPlayer(targetPlayer, ball, true);

                Transform playerCam = null;
                if (_player == null) playerCam = transform;
                else playerCam = _player.playerCam.transform;
                
                Vector3 a = (targetPointTransform.position - playerCam.position) / 2; // Adjacent : half distance between the player and the target
                float camAngelToA = Vector3.Angle(playerCam.forward, a); // Angle between a and hypotenuse (where the player is looking)

                float hMagnitude = a.magnitude / Mathf.Cos(camAngelToA * Mathf.Deg2Rad);
                Vector3 h = playerCam.forward * hMagnitude; // Hypotenuse
                if (Vector3.Dot(playerCam.forward, a) < 0) h *= -1; // If the player is looking at the inverse direction of the target, invert the vector
                Vector3 o = playerCam.position + h - (playerCam.position + a); // Opposite : the vector from adjacent to hypotenuse
                
                o *= Mathf.Clamp(chargeValue, 0, 1); // Change curve amplitude following the charge time (scale opposite)
                o *= Mathf.Clamp(1 / (o.magnitude / 15 ), 0, 1); // Limit curve amplitude

                Vector3 multiplier = Vector3.zero; 
                if (Vector3.Dot(playerCam.forward, a) < 0) // Add offset to the step point if the player is looking behind
                    multiplier = -a * Mathf.Pow((90 - camAngelToA) / 90, 2);

                Vector3 stepPosition = playerCam.position + a + o + multiplier;
                
                float accuracy = accuracyValue;
                if (accuracy == -1) 1f;
                if (pass) accuracy = 1;

                Ball.BallStateRefab state = pass ? Ball.BallStateRefab.Pass : Ball.BallStateRefab.Curve;
                CmdThrowBall(ball, _player, stepPosition, targetPointTransform, force, accuracy, _speedCurve, state); // Throw the object
                if (pass) OnPass.Invoke();
                else OnThrow.Invoke();
            }
            else
            {
                //Simple throw
                print("simple throw");
                Vector3 direction = _player.playerCam.transform.forward;
                Vector3 velocity = direction * force;
                float magnitude = velocity.magnitude;
                //Add the player current velocity to the throw 
                Vector3 playerVelAlongCam = Vector3.Project(_player._controller.rb.velocity, _player.playerCam.transform.forward);
                velocity += playerVelAlongCam;

                ball.rb.isKinematic = false;
                CmdSetKinematic(ball, false);
                ball._ballState = Ball.BallStateRefab.FreeThrow;
                ball.owner = _player;
                CmdChangeBallState(ball, Ball.BallStateRefab.FreeThrow, _player);
                ball.rb.velocity = velocity;
                CmdSimpleThrowBall(ball, velocity); // Throw the object in front of the camera
                
                OnThrow.Invoke();
            }
        }
    }

    [Command]
    private void CmdSimpleThrowBall(Ball ball, Vector3 velocity)
    {
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
    public void CmdThrowBall(Ball ball, Player throwingPlayer , Vector3 step, Transform target, float speed, float accuracy, AnimationCurve curve, Ball.BallStateRefab state)
    {
        StartCoroutine(ThrowEnumerator(ball, throwingPlayer, step, target, speed, accuracy, curve, state));
    }

    public float dbgTime = 0;
    
    public void StopThrow()
    {
        _canDoThrowPath = false;
    }
    
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
        AnimationCurve curve, Ball.BallStateRefab state = Ball.BallStateRefab.Curve)
    {
        ball.owner = throwingPlayer;
        ball._ballState = state;
        CmdChangeBallState(ball, state, _player);

        Vector3 origin = transform.position;
        if (_player != null) origin = _player._pickup.pickupPoint.position;
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
        _canDoThrowPath = true;

        while (time < 1 && (ball._ballState == Ball.BallStateRefab.Curve || ball._ballState == Ball.BallStateRefab.Pass) && _canDoThrowPath)
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
            CmdValidateAndChangeState(ball, Ball.BallStateRefab.Picked, true, Ball.BallStateRefab.FreeThrow, _player,
                false);
            /*CmdSetKinematic(ball, false);
            CmdChangeBallState(ball, Ball.BallStateRefab.FreeThrow, _player);*/
        }

        _canDoThrowPath = true;
        //ball.rb.velocity = direction;
    }
    

    
    /// <summary>
    /// Cancel throw and cancel charge
    /// </summary>
    public void CancelThrow()
    {
        if (IsCharging) IsCharging = false;
    }

    [Command]
    private void CmdValidateAndChangeState(Ball ball, Ball.BallStateRefab desiredState, bool invertState, 
        Ball.BallStateRefab finalState, Player player, bool ballKinematic)
    {
        if (invertState)
        {
            if (ball._ballState != desiredState)
            {
                RpcSetKinematic(ball, ballKinematic);
                RpcChangeBallState(ball, finalState, player);
            }
        }
        else if (ball._ballState == desiredState)
        {
            RpcSetKinematic(ball, ballKinematic);
            RpcChangeBallState(ball, finalState, player);
        }
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

    #region Ui Player Warn

    [Command]
    public void CmdWarnPlayer(Player player, Ball ball, bool warn)
    {
        RpcWarnPlayer(player, ball, warn);
    }
    
    [ClientRpc]
    private void RpcWarnPlayer(Player player, Ball ball, bool warn)
    {
        player._dirIndicatorHandler.incomingBall = warn ? ball.transform : null;
    }

    #endregion 
    
}
