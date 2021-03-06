using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DirIndicatorHandler : MonoBehaviour
{
    [SerializeField] private Material uiMat;
    [SerializeField] private Camera cam;
    [SerializeField] private float minSize = 7;
    [SerializeField] private float sizeFact = 0.7f;
    [SerializeField] private float lengthSize = 0.1f;
    [SerializeField] [Range(0,1)] private float displayFactor = 0.1f;

    private float ballAngle;
    private float angleX, angleY, sizeX, sizeY, rotation;

    [CanBeNull] public Transform incomingBall
    {
        get => _incomingBall;
        set
        {
            _incomingBall = value;
            if (value == null)
                ResetBall();
        }
    }
    private Transform _incomingBall;
    public bool isTargeted;
    
    
    private void Start()
    {
        uiMat.SetFloat("BorderSize", 0);
        uiMat.SetFloat("SizeX", 0);
        uiMat.SetFloat("SizeY", 0);
    }

    private void Update()
    {
        if (isTargeted && !incomingBall)
        {
            uiMat.SetFloat("BorderSize", 0.2f);
            uiMat.SetColor("BorderColor", Color.blue);
        }
        else
        {
            uiMat.SetFloat("BorderSize", 0);
        }
        
        
        if (incomingBall == null)
        {
            return;
        }
        
        ClaculateUiPos();

        uiMat.SetColor("BorderColor", Color.red);
        uiMat.SetFloat("BorderSize", 0.2f);
        uiMat.SetFloat("PosX", angleX);
        uiMat.SetFloat("PosY", angleY);
        uiMat.SetFloat("SizeX", sizeY);
        uiMat.SetFloat("SizeY", sizeX);
        //uiMat.SetFloat("Rotation", rotation);
    }

    private bool corner;
    float lerpFactX_D;
    float lerpFactY_D;

    private void ClaculateUiPos()
    {
        Vector3 ballPos = incomingBall.position;
        Vector3 ballOnScreen = cam.WorldToViewportPoint(ballPos);

        float lerpFactX = 1;
        float lerpFactY = 1;
        
        //Transform variable from [0 ; 1] to [-5 ; 5] (if on the screen)
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
            //For each sides of the screen
            if (ballOnScreen.x < displayFactor && ballOnScreen.x > 0) 
            { lerpFactX = 1 - ballOnScreen.x * (1 / displayFactor); }
            if (ballOnScreen.x < 1 && ballOnScreen.x > 1 - displayFactor) 
            { lerpFactX = 1 - (1 - ballOnScreen.x) * (1 / displayFactor); }
            if (ballOnScreen.y < displayFactor && ballOnScreen.y > 0) 
            { lerpFactY = 1 - ballOnScreen.y * (1 / displayFactor); }
            if (ballOnScreen.y < 1 && ballOnScreen.y > 1 - displayFactor) 
            { lerpFactY = 1 - (1 - ballOnScreen.y) * (1 / displayFactor); }
            

            //For each corner
            if ((lerpFactX != 1 && lerpFactY != 1) || 
                (ballOnScreen.x >= 1 || ballOnScreen.x <= -1) && (ballOnScreen.y >= 1 || ballOnScreen.y <= -1))
            {
                lerpFactX = 1;
                lerpFactY = 1;
                corner = true;
            }
            else
            {
                corner = false;
            }

            lerpFactX_D = lerpFactX;
            lerpFactY_D = lerpFactY;
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

        //rotation for further graphics implementation
        rotation = Vector2.Angle(Vector2.up, new Vector2(angleX, angleY));
        if (Vector2.Angle(Vector2.right, new Vector2(angleX, angleY)) < 90)
            rotation = 360 - rotation;
        
        sizeX = (minSize - Mathf.Abs(angleX * lengthSize)) * sizeFact * lerpFactX;
        sizeY = (minSize - Mathf.Abs(angleY * lengthSize)) * sizeFact * lerpFactY;

    }

    private void newTest()
    {
        Vector3 ballPos = incomingBall.position;
        Vector3 screenPos = cam.WorldToViewportPoint(ballPos);

        //Flip if is behind the camera
        if (screenPos.z < 0) screenPos *= -1;
        //Center the coordinates
        screenPos -= screenPos / 2;
        screenPos *= 2;

        //Find the angle of the point
        float angle = Mathf.Atan2(screenPos.x, screenPos.y);
        angle -= 90 * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angle);
        float sin = -Mathf.Sin(angle);
            
        float m = cos / sin;
            
        if (cos > 0) { screenPos = new Vector3(1/m, 1, 0); }
        else { screenPos = new Vector3(-1/m, -1, 0); }
            
        if (screenPos.x > 1) { screenPos = new Vector3(1, 1*m, 0);  }
        else if (screenPos.x < -1) { screenPos = new Vector3(-1, -1*m, 0);  }

        angleX = screenPos.y * -5;
        angleY = screenPos.x * -5;
        sizeX = 1;
        sizeY = 1;
    }

    public void SetIncomingBall(Transform ball) { incomingBall = ball; }

    public void ResetBall()
    {
        uiMat.SetFloat("BorderSize", 0);
        uiMat.SetFloat("SizeX", 0);
        uiMat.SetFloat("SizeY", 0);
    }

    private void OnApplicationQuit()
    {
        uiMat.SetFloat("BorderSize", 0);
        uiMat.SetFloat("SizeX", 0);
        uiMat.SetFloat("SizeY", 0);
    }


    /*private void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        GUI.color = Color.black;
        guiStyle.fontSize = 30;
        GUILayout.Space(50);
        GUILayout.Label("x :" + angleX + " y : " + angleY, guiStyle);
        GUILayout.Label("lerpFactx :" + lerpFactX_D + " lerpFactY : " + lerpFactY_D, guiStyle);
        GUILayout.Label("corner :" + corner, guiStyle);
    }*/
}
