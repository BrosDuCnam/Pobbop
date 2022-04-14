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
    [SyncVar] private float currentHealth;
    [SyncVar] public string username = "Noob";
    public Camera playerCam;
    [SerializeField] private RawImage _targetImage;
    [SerializeField] public Transform targetPoint;
    [SerializeField] private float ballVelToDie = 8;

    [SyncVar]
    private bool _isDead = false;
    public bool isDead { get { return _isDead;  } protected set { _isDead = value; } }
    private int _kills = 0;
    public int kills { get { return _kills; } set { _kills = value; } }

    private int _deaths = 0;
    public int deaths { get { return _deaths; } set { _deaths = value; } }

    public Pickup _pickup;
    public Throw _throw;
    public Targeter _targeter;

    public bool IsHoldingObject
    {
        get => _pickup.ball != null;
        set => _pickup.ball = value ? _pickup.ball : null;
    }

    public bool IsCharging { get => _throw.IsCharging; }  

    private void Start()
    {
        _pickup = GetComponent<Pickup>();
        _throw = GetComponent<Throw>();
        _targeter = GetComponent<Targeter>();
    }

    private void Update()
    {
        UpdateTargetUI();
    }

    public Transform GetBall()
    {
        return _pickup.ball;
    }

    private void Die()
    {
        deaths++;
        print("dead" + name);

        isDead = true;

        StartCoroutine(Respawn());
    }
    
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
    
    private void UpdateTargetUI()
    {
        if (HasTarget)
        {
            Vector2 canvasSize = _targetImage.GetComponent<RectTransform>().sizeDelta;
            
            Transform targetPointTransform = Target.transform;
            if (Target.TryGetComponent(out Player otherPlayer))
                targetPointTransform = otherPlayer.targetPoint;
            
            
            
            Vector3 targetPosition = playerCam.WorldToScreenPoint(targetPointTransform.position);

            Vector3 targetPositionInCanvas = new Vector2(targetPosition.x / canvasSize.x * 100,
                targetPosition.y / canvasSize.y * 100);
            
            _targetImage.enabled = true;

            //Clamp the target to the border of the screen if we are looking behind
            if (targetPosition.z < 0)
            {
                //Invert because it's behind
                targetPositionInCanvas = (targetPositionInCanvas - new Vector3(Screen.width/2, Screen.height/2)) * -1 +
                                         new Vector3(Screen.width/2, Screen.height/2);
                
                //Vector from the middle of the screen
                Vector2 targetPositionAbsolute =  (Vector2)targetPositionInCanvas - new Vector2(Screen.width/2, Screen.height/2);
                
                //Max magnitude for fullHD screen (from center to corner) = sqrt((1080/2)² + (1920/2)²) = 1102
                float maxMagnitude = Mathf.Sqrt(Mathf.Pow(Screen.width / 2,2) + Mathf.Pow(Screen.height / 2,2));
                if (targetPositionAbsolute.magnitude < maxMagnitude)
                {
                    //Offsets the target of the screen so we can clamp it back later
                    float fact = maxMagnitude / targetPositionAbsolute.magnitude;
                    targetPositionInCanvas = (targetPositionInCanvas - new Vector3(Screen.width/2, Screen.height/2)) * fact +
                                              new Vector3(Screen.width/2, Screen.height/2);
                }
            }

            targetPositionInCanvas.x = Mathf.Clamp(targetPositionInCanvas.x, 0, Screen.width);
            targetPositionInCanvas.y = Mathf.Clamp(targetPositionInCanvas.y, 0, Screen.height);

            _targetImage.rectTransform.position = targetPositionInCanvas; // Set target image position to the current target
        } else
        {
            _targetImage.enabled = false;
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
    
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            BallRefab ball = col.gameObject.GetComponent<BallRefab>();
            if (ball.rb.velocity.magnitude > ballVelToDie)
            {
                Die();
            }
        }
    }
    
    public void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _throw.ChargeThrow();
        if (ctx.canceled) _throw.ReleaseThrow();
    }
    
}
