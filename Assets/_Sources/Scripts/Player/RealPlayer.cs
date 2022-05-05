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
        DrawChargingCurve();
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
        if (HasTarget && IsCharging)
        {
            _chargingCurveLineRenderer.enabled = true;
            
            Transform targetPointTransform = Target.transform;
            targetPointTransform = Target.GetComponent<Player>().targetPoint;
            
            float chargeTime = Mathf.Clamp(Time.time - _throw._startChargeTime, 0, _throw._maxChargeTime);
            float chargeValue = _throw._chargeCurve.Evaluate(chargeTime / _throw._maxChargeTime);

            Transform playerCam = this.playerCam.transform;
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

            Debug.DrawRay(playerCam.position, h, Color.red);
            Debug.DrawRay(playerCam.position, a, Color.blue);
            Debug.DrawRay(playerCam.position + a, o, Color.green);
            Debug.DrawRay(playerCam.position, a + o + multiplier, Color.yellow);
            Debug.DrawRay(playerCam.position + a + o, multiplier, Color.magenta);

            Vector3[] positions = Utils.GetBezierCurvePositions(GetBall().position, stepPosition, targetPointTransform.position, 30);
            _chargingCurveLineRenderer.positionCount = positions.Length+1;
            _chargingCurveLineRenderer.SetPositions(positions);
            _chargingCurveLineRenderer.SetPosition(_chargingCurveLineRenderer.positionCount-1, targetPointTransform.position);
        }
        else
        {
            _chargingCurveLineRenderer.enabled = false;
        }
    }
}