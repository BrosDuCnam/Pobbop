using System;
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

    private new void Awake()
    {
        base.Awake();
        
        _controller.additiveCamera = true;
        _healthSystem.OnHealthZero.AddListener(Eliminated); // On définit la fonction éliminer sur l'event OnHealthZero
    }
    
    private void Update()
    {
        UpdateTargetUI();
        
        if (_drawChargingCurve)
        {
            DrawChargingCurve();
        }
        
        _chargingSlider.value = _throwSystem.ChargeValue;
    }

    private void DrawChargingCurve()
    {
        if (HasTarget && IsHoldingObject)
        {
            _chargingCurveLineRenderer.enabled = true;
            
            // Calculate the multiplier of step distance
            float multiplier = Mathf.Pow(1.5f, _rigidbody.velocity.magnitude);
            multiplier += Mathf.Pow(50f, _throwSystem.ChargeValue);

            multiplier = Mathf.Clamp(multiplier, _throwSystem.minStepMultiplier, _throwSystem.maxStepMultiplier);

            Vector3 stepPosition = (Camera.transform.forward * multiplier) + Camera.transform.position;

            Vector3[] positions = Utils.GetBezierCurvePositions(HoldingObject.transform.position, stepPosition, Target.transform.position, 30);
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
            _targetImage.enabled = true;
            
            Vector2 canvasSize = _targetImage.GetComponent<RectTransform>().sizeDelta;
            Vector3 targetPosition = Camera.WorldToScreenPoint(Target.transform.position);

            // TODO - Correct bug that the target image is not on edge of screen when player lokk behind the target
            Vector3 targetPositionInCanvas = new Vector2(targetPosition.x / canvasSize.x * 100,
                targetPosition.y / canvasSize.y * 100);
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
        int teamNumber = transform.GetComponent<BasePlayer>().teamNumber;
        int enemyTeam = _healthSystem.LastPlayerDamage.GetComponent<BasePlayer>().teamNumber;
        //NetworkManagerLobby.AddPoint(enemyTeam);
        //PlayerSpawnSystem.PlayerRemoveTransform(transform, teamNumber);
        PlayerSpawnSystem.Respawn(transform, teamNumber);
    }
    
    #region "Inputs"
    
    public void TogglePickupDrop(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _pickUpDropSystem.TogglePickupDrop();
    }
    
    public void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _throwSystem.ChargeThrow();
        if (ctx.canceled) _throwSystem.ReleaseThrow();
    }
    
    #endregion
    
}