using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DirIndicatorHandler : MonoBehaviour
{
    [SerializeField] private Material uiMat;
    [SerializeField] private Camera cam;
    [SerializeField] private float minSize = 7;
    [SerializeField] private float sizeFact = 0.5f;
    [SerializeField] private float lengthSize = 0.5f;
    [SerializeField] private float maxBorderSize = 1.5f;
    //Lerp distance for the directional indicator to show or not
    [SerializeField] [Range(0,1)] private float displayFactor = 0.1f;
    [SerializeField] private Color defaultColor;


    [Header("Charge Colors")] 
    [SerializeField] private Color minChargeColor;
    [SerializeField] private Color maxChargeColor;
    
    
    /// <summary>
    /// Value to assign when the ball is thrown
    /// </summary>
    [HideInInspector] public float initialThrowDistance = 10;
    
    private float ballAngle;
    private Transform me;
    private float angleX, angleY, sizeX, sizeY, rotation;
    
    [CanBeNull] private Transform incomingBall;
    [SerializeField] [CanBeNull] private BasePlayer targeter;


    private void Start()
    {
        me = gameObject.transform;
    }

    private void Update()
    {
        //No targeter, no ball incoming
        if (targeter == null && incomingBall == null) return;
        
        //Getting targeted
        if (targeter.IsCharging && incomingBall == null)
        {
            //Change color according to the opponent charge
            uiMat.SetColor("BorderColor", Color.Lerp(minChargeColor, maxChargeColor, targeter.ChargeValue));
            return;
        }
        
        //Ball incoming
        uiMat.SetColor("BorderColor", defaultColor);
        //Set the border size according to the distance between the ball and the player 
        uiMat.SetFloat("BorderSize", 
            maxBorderSize - maxBorderSize * Vector3.Distance(incomingBall.position, transform.position) / initialThrowDistance);

        
        ClaculateUiPos();

        uiMat.SetFloat("PosX", angleX);
        uiMat.SetFloat("PosY", angleY);
        uiMat.SetFloat("SizeX", sizeY);
        uiMat.SetFloat("SizeY", sizeX);
        uiMat.SetFloat("Rotation", rotation);
    
    }
    
    /// <summary>
    /// Calculates the ui directional indicator based on ball position
    /// </summary>
    private void ClaculateUiPos()
    {
        Vector3 ballPos = incomingBall.position;
        Vector3 ballOnScreen = cam.WorldToViewportPoint(ballPos);

        float lerpFactX = 1;
        float lerpFactY = 1;
        
        //Transform variable from [0 ; 1] to [-5 ; 5]
        angleX = ballOnScreen.x * 10 - 5;
        angleY = ballOnScreen.y * 10 - 5;
        
        //Checks if the ball is on the screen bounds
        if (ballOnScreen.x > displayFactor && ballOnScreen.x < 1 - displayFactor && 
            ballOnScreen.y > displayFactor && ballOnScreen.y < 1 - displayFactor && 
            ballOnScreen.z > 0)
        {
            //If it is, don't display it
            sizeX = 0;
            sizeY = 0;
            return;
        }

        //Lots of if to calculate a smooth transition to display the directional indicator 
        if (ballOnScreen.z > 0)
        {
            if (ballOnScreen.x < displayFactor && ballOnScreen.x > 0) 
            { lerpFactX = 1 - ballOnScreen.x * (1 / displayFactor); }
            if (ballOnScreen.x < 1 && ballOnScreen.x > 1 - displayFactor) 
            { lerpFactX = 1 - (1 - ballOnScreen.x) * (1 / displayFactor); }
            if (ballOnScreen.y < displayFactor && ballOnScreen.y > 0) 
            { lerpFactY = 1 - ballOnScreen.y * (1 / displayFactor); }
            if (ballOnScreen.y < 1 && ballOnScreen.y > 1 - displayFactor) 
            { lerpFactY = 1 - (1 - ballOnScreen.y) * (1 / displayFactor); }
            
            if (angleX >= 5 || angleX <= -5 && !(angleY < 5 - displayFactor * 5 && angleY > displayFactor * 5) ||
                angleY >= 5 || angleY <= -5 && !(angleX < 5 - displayFactor * 5 && angleX > displayFactor * 5))
            {
                lerpFactX = 1;
                lerpFactY = 1;
            }
        }

        if (ballOnScreen.z > 0)
        {
            angleX *= -1;
            angleY *= -1;
        }

        Vector2 uiPos = new Vector2(angleX, angleY);
        //7 is the max magnitude (corner)
        if (uiPos.magnitude < 7)
        {
            float fact = 7 / uiPos.magnitude;
            uiPos *= fact;
            angleX = uiPos.x;
            angleY = uiPos.y;
        }

        angleX = Mathf.Clamp(angleX, -5, 5);
        angleY = Mathf.Clamp(angleY, -5, 5);

        rotation = Vector2.Angle(Vector2.up, new Vector2(angleX, angleY));
        if (Vector2.Angle(Vector2.right, new Vector2(angleX, angleY)) < 90)
            rotation = 360 - rotation;
        sizeX = (minSize - Mathf.Abs(angleX * lengthSize)) * sizeFact * lerpFactX;
        sizeY = (minSize - Mathf.Abs(angleY * lengthSize)) * sizeFact * lerpFactY;
        
    }


    /// <summary>
    /// Set the ball targeted to the player how should be warned
    /// </summary>
    /// <param name="ball"></param>
    public void SetIncomingBall(Transform ball)
    {
        incomingBall = ball;
    }
    
    /// <summary>
    /// Reset the incoming ball if the player didn't shoot, switched target, or the ball missed 
    /// </summary>
    public void ResetIncomingBall()
    {
        incomingBall = null;
    }

    public void SetTargeter(BasePlayer player)
    {
        targeter = player;
    }

    public void ResetTargeter()
    {
        targeter = null;
    }

    private void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        GUI.color = Color.black;
        guiStyle.fontSize = 30;
        GUILayout.Space(50);
        GUILayout.Label("x :" + angleX + " y : " + angleY, guiStyle);
    }
}
