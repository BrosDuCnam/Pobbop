using System;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RealPlayer : Player
{
    [SerializeField] protected RawImage _targetImage;
    [SerializeField] private LineRenderer _chargingCurveLineRenderer;

    #region Inputs

    public void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _throw.ChargeThrow();
        if (ctx.canceled) _throw.ReleaseThrow();
    }

    public void Pass(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _throw.ChargeThrow();
        if (ctx.canceled) _throw.ReleaseThrow(true);
    }
    
    public void Catch(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _pickup.TryToCatch();
    }

    #endregion    
    
    private void Update()
    {
        UpdateTargetUI();
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

    private void DrawChargingCurve()
    {
        if (HasTarget && IsHoldingObject)
        {
            _chargingCurveLineRenderer.enabled = true;
            
            // Calculate the multiplier of step distance
           float multiplier = Mathf.Pow(50f, _throw.ChargeValue);

            multiplier = Mathf.Clamp(multiplier, _throw.minStepMultiplier, _throw.maxStepMultiplier);

            Vector3 stepPosition = (Camera.transform.forward * multiplier) + Camera.transform.position;


            Transform targetPointTransform = Target.transform;
            if (Target.transform.FindObjectsWithTag("Targetter").Count > 0)
            {
                targetPointTransform = Target.transform.FindObjectsWithTag("Targetter").First().transform;
            }
            
            Vector3[] positions = Utils.GetBezierCurvePositions(GetBall().position, stepPosition, targetPointTransform.position, 30);
            _chargingCurveLineRenderer.positionCount = positions.Length+1;
            _chargingCurveLineRenderer.SetPositions(positions);
            _chargingCurveLineRenderer.SetPosition(_chargingCurveLineRenderer.positionCount-1, Target.transform.position);
        }
        else
        {
            _chargingCurveLineRenderer.enabled = false;
        }
    }
}