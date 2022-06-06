using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RealPlayer : Player
{
    [SerializeField] protected RawImage _targetImage;
    [SerializeField] private LineRenderer _chargingCurveLineRenderer;
    
    [Header("Crosshair / Target UI params")]
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private Color restColor = Color.white;
    
    [SerializeField] private float targetScale = 4f;
    [SerializeField] private float restScale = 0.2f;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float colorSpeed = 5f;


    private bool lockTargetUiPos = false;

    [SerializeField] private CanvasGroup _escapeCanvasGroup;
    public bool IsEscapeCanvasActive => _escapeCanvasGroup.alpha > 0;

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
        base.Update();
        
        UpdateTargetUI();
        DrawChargingCurve();
    }

    public void Escape(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        ToggleEscapeCanvas();
    }
    
    public void ToggleEscapeCanvas()
    {
        _controller.enabled = !IsEscapeCanvasActive;
        
        _pickup.enabled = !IsEscapeCanvasActive;
        _controller.enabled = !IsEscapeCanvasActive;
        _targeter.enabled = !IsEscapeCanvasActive;
        _throw.enabled = !IsEscapeCanvasActive;
        
        Cursor.visible = !IsEscapeCanvasActive;
        Cursor.lockState = !IsEscapeCanvasActive ? CursorLockMode.None : CursorLockMode.Locked;
        
        float alphaTarget = IsEscapeCanvasActive ? 0 : 1;
        DOTween.To(() => _escapeCanvasGroup.alpha, x => _escapeCanvasGroup.alpha = x, alphaTarget, 0.2f).SetEase(Ease.InOutQuad);
    }    
    private void UpdateTargetUI()
    {
        if (HasTarget && IsHoldingObject)
        {
            Vector2 canvasSize = _targetImage.GetComponent<RectTransform>().sizeDelta;
            
            Transform targetPointTransform = Target.transform;
            if (Target.TryGetComponent(out Player otherPlayer))
                targetPointTransform = otherPlayer.targetPoint;

            _targetImage.color = targetColor;
            
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

            
            //Position
            Vector2 currentPos = _targetImage.rectTransform.position;
            float distance = Vector2.Distance(currentPos, targetPositionInCanvas);
            if (distance > 10)
            {
                float multiplier = distance < 200 ? (200 - distance) / 10 : 1;
                
                Vector2 newPos = Vector2.Lerp(currentPos, targetPositionInCanvas, Time.deltaTime * moveSpeed * multiplier);
                _targetImage.rectTransform.position = newPos;
            }
            else _targetImage.rectTransform.position = targetPositionInCanvas; // Set target image position to the current target
            
            //Scale
            Vector2 currentScale = _targetImage.rectTransform.localScale;
            Vector2 desiredScale = Vector2.one * (1 / targetPosition.z) * targetScale;
            float scaleDistance = Vector2.Distance(currentScale, desiredScale);
            if (Mathf.Abs(desiredScale.magnitude) > 1f)
            {
                if (targetPositionInCanvas.x > Screen.width * 0.9f || targetPositionInCanvas.x < Screen.width * 0.1f 
                || targetPositionInCanvas.y > Screen.height * 0.9f || targetPositionInCanvas.y > Screen.height * 0.9f)
                    return;
                
                float multiplier = scaleDistance < 2 ? (2 - distance) / 2 : 1;
                
                Vector2 newScale = Vector2.Lerp(currentScale, desiredScale, Time.deltaTime * scaleSpeed * multiplier);
                _targetImage.rectTransform.localScale = newScale;
            }
            else _targetImage.rectTransform.localScale = desiredScale;
            
            //Color
            Color currentColor = _targetImage.color;
            Color newColor = Color.Lerp(currentColor, restColor, Time.deltaTime * colorSpeed);
            _targetImage.color = newColor;


            lockTargetUiPos = false;
        } 
        else
        {
            Vector2 screenCenter = new Vector2(Screen.width/2, Screen.height/2);

            //Position
            if (!lockTargetUiPos)
            {
                Vector2 currentPos = _targetImage.rectTransform.position;
                Vector2 newPos = Vector2.Lerp(currentPos, screenCenter, Time.deltaTime * moveSpeed);
                _targetImage.rectTransform.position = newPos;
                if (Vector2.Distance(screenCenter, newPos) < 50) lockTargetUiPos = true;
            }

            
            Vector2 currentScale = _targetImage.rectTransform.localScale;
            Vector2 newScale = Vector2.Lerp(currentScale, Vector2.one * restScale, Time.deltaTime * scaleSpeed);
            _targetImage.rectTransform.localScale = newScale;

            Color currentColor = _targetImage.color;
            Color newColor = Color.Lerp(currentColor, restColor, Time.deltaTime * colorSpeed);
            _targetImage.color = newColor;
            
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