using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{ 
    [SyncVar] public int teamId;
    private float currentHealth;
    public string username = "Noob";
    public Camera playerCam;
    [SerializeField] public Transform targetPoint;
    [SerializeField] private float ballVelToDie = 8;
    

    [SyncVar]
    private bool _isDead = false;
    public bool isDead { get { return _isDead;  } protected set { _isDead = value; } }
    private int _kills = 0;
    public int kills { get { return _kills; } set { _kills = value; } }

    private int _deaths = 0;
    public int deaths { get { return _deaths; } set { _deaths = value; } }

    private NetworkManagerRefab _networkManagerRefab;
    public Pickup _pickup;
    public Throw _throw;
    public Targeter _targeter;
    public Controller _controller;
    public DirIndicatorHandler _dirIndicatorHandler;
    
    public Camera Camera { get { return _controller.camera; } }

    public bool IsHoldingObject
    {
        get
        {
            if (_pickup == null) return false;
            return _pickup.ball != null;
        } 
        set => _pickup.ball = value ? _pickup.ball : null;
    }

    public bool IsCharging 
    { get
        {
            if (_throw == null) return false;
            return _throw.IsCharging;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    protected void Start()
    {
        _pickup = GetComponent<Pickup>();
        _throw = GetComponent<Throw>();
        _targeter = GetComponent<Targeter>();
        _controller = GetComponent<Controller>();
        _dirIndicatorHandler = GetComponent<DirIndicatorHandler>();
        //teamId = UnityEngine.Random.Range(0, 2220);
        Die(null, 0.4f); //Temp fix for client player TODO: Find a better fix
    }
    

    public Transform GetBall()
    {
        return _pickup.ballTransform;
    }

    public GameObject GetDesiredFriendly()
    {
        return _targeter.GetDesiredFriend();
    }
    
    public void Die(Ball ball = null, float respawnTime = 0.4f)
    {
        if (isDead) return;
        if (ball != null)
            if (GetBall() == ball.transform)
                return;
        deaths++;
        print("dead" + name);
        isDead = true;
        _dirIndicatorHandler.incomingBall = null;
        //Drop ball if it's in hand
        if (_pickup.ball != null)
        {
            ChangeBallLayer(_pickup.ball.gameObject, false);
            _pickup.CmdChangeBallState(_pickup.ball.GetComponent<Ball>(), Ball.BallStateRefab.Free);
            _pickup.ball.collider.enabled = true;
            _pickup.ballTransform = null;
            _pickup.ball = null;
            _throw.IsCharging = false;
        }
        _pickup.enabled = false;
        _controller.enabled = false;
        _targeter.enabled = false;
        _throw.enabled = false;

        StartCoroutine(Respawn(respawnTime));
    }
    
    private IEnumerator Respawn(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        
        
        Transform spawnPoint = NetworkManagerRefab.instance.GetRespawnPosition();
        transform.position =  spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        _controller.rb.velocity = Vector3.zero;
        isDead = false;
        if (isLocalPlayer)
        {
            _pickup.enabled = true;
            _controller.enabled = true;
            _targeter.enabled = true;
            _throw.enabled = true;
        }
    }
    
        
    public bool HasTarget
    {
        get => _targeter.CurrentTarget != null;
    }
    
    [CanBeNull] public GameObject Target
    {
        get => _targeter.CurrentTarget.gameObject;
    }

    /// <summary>
    /// Toggle ball layer. Set to true to be on top.
    /// </summary>
    /// <param name="ball"></param>
    /// <param name="layer"></param>
    public void ChangeBallLayer(GameObject ball, bool layer)
    {
        if (ball == null) return;
        ball.gameObject.layer = layer ? 
            LayerMask.NameToLayer("Always On Top") : 
            LayerMask.NameToLayer("Default");
    }
    
    private void OnCollisionEnter(Collision col)
    {
        if (!enabled) return;
        if (col.gameObject.CompareTag("Ball"))
        {
            Ball ball = col.gameObject.GetComponent<Ball>();
            if (ball.owner != null)
                if (ball.owner.teamId == teamId)
                    return;
            
            if (ball.rb.velocity.magnitude > ballVelToDie && !isDead
                && (ball._ballState == Ball.BallStateRefab.Curve || ball._ballState == Ball.BallStateRefab.FreeThrow))
            {
                Die(ball);
            }
        }
    }
}
