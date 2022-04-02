﻿using System;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RealPlayer : BasePlayer
{
    [SerializeField] private bool _drawChargingCurve = true;
    [Tooltip("The image of target")]
    [SerializeField] private RawImage _targetImage;
    [Tooltip("The slider of charging value")]
    [SerializeField] private Slider _chargingSlider;
    [Tooltip("Line renderer of charging curve")]
    [SerializeField] private LineRenderer _chargingCurveLineRenderer;

    private PlayerSpawnSystem playerSpawnSystem;

    private string _name; 
    
    private new void Awake()
    {
        base.Awake();
        
        healthSystem.OnHealthZero.AddListener(Eliminated); // On définit la fonction éliminer sur l'event OnHealthZero
        healthSystem.OnHealthChanged.AddListener(() => pickUpDropSystem.IsStone = true);

        playerSpawnSystem = GetComponent<PlayerSpawnSystem>();
    }

    private void Start() //TODO: use by Camille to debug bot
    {
        GameControllerDEBUG.AddPlayer(this); // May cause lag
    }
    
    private void Update()
    {
        UpdateTargetUI();
        
        if (_drawChargingCurve)
        {
            DrawChargingCurve();
        }
        
        _chargingSlider.value = throwSystem.ChargeValue;
    }

    private void DrawChargingCurve()
    {
        if (HasTarget && IsHoldingObject)
        {
            _chargingCurveLineRenderer.enabled = true;
            
            // Calculate the multiplier of step distance
            float multiplier = Mathf.Pow(1.5f, rigidbody.velocity.magnitude);
            multiplier += Mathf.Pow(50f, throwSystem.ChargeValue);

            multiplier = Mathf.Clamp(multiplier, throwSystem.minStepMultiplier, throwSystem.maxStepMultiplier);

            Vector3 stepPosition = (Camera.transform.forward * multiplier) + Camera.transform.position;


            Transform targetPointTransform = Target.transform;
            if (Target.transform.FindObjectsWithTag("Targetter").Count > 0)
            {
                targetPointTransform = Target.transform.FindObjectsWithTag("Targetter").First().transform;
            }
            
            Vector3[] positions = Utils.GetBezierCurvePositions(HoldingObject.transform.position, stepPosition, targetPointTransform.position, 30);
            _chargingCurveLineRenderer.positionCount = positions.Length+1;
            _chargingCurveLineRenderer.SetPositions(positions);
            _chargingCurveLineRenderer.SetPosition(_chargingCurveLineRenderer.positionCount-1, Target.transform.position);
        }
        else
        {
            _chargingCurveLineRenderer.enabled = false;
        }
    }
            
    private void UpdateTargetUI()
    {
        if (HasTarget)
        {
            Vector2 canvasSize = _targetImage.GetComponent<RectTransform>().sizeDelta;
            
            Transform targetPointTransform = Target.transform;
            if (Target.transform.FindObjectsWithTag("Targetter").Count > 0)
            {
                targetPointTransform = Target.transform.FindObjectsWithTag("Targetter").First().transform;
            }
            
            
            Vector3 targetPosition = Camera.WorldToScreenPoint(targetPointTransform.position);

            // TODO - Correct bug that the target image is not on edge of screen when player lokk behind the target
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
    
    private void Eliminated()
    {
        playerSpawnSystem.PlayerEliminated();
    }

    public void ChangeName(string name)
    {
        _name = name;
        CmdChangeName(name, gameObject);
    }

    [Command (requiresAuthority = false)]
    private void CmdChangeName(string name, GameObject player)
    {
        print("cmdChangeName");
        RpcChangeName(name, player);
    }

    [ClientRpc]
    private void RpcChangeName(string name, GameObject player)
    {
        player.name = name;
        print("name changed to " + name);

    }
    
    
    #region "Inputs"
    
    public void TogglePickupDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.started) pickUpDropSystem.TogglePickupDrop();
    }
    
    public void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.started) throwSystem.ChargeThrow();
        if (ctx.canceled) throwSystem.ReleaseThrow();
    }
    
    #endregion
    
}