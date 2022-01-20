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
    [SerializeField] private float sizeFact = 0.5f;
    [SerializeField] private float lengthSize = 0.5f;

    private float ballAngle;
    private Transform me;
    [CanBeNull] public Transform incomingBall;
    private float angleX, angleY, sizeX, sizeY;

    
    private void Start()
    {
        me = gameObject.transform;
        incomingBall = GameObject.FindWithTag("Ball").transform; // WIP
    }

    private void Update()
    {
        if (incomingBall != null)
        {
            Vector3 ballPos = incomingBall.position;
            Vector3 ballOnScreen = cam.WorldToViewportPoint(ballPos);
            
            //Transform variable from [0 ; 1] to [-5 ; 5]
            angleX = ballOnScreen.x * 10 - 5;
            angleY = ballOnScreen.y * 10 - 5;

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

            sizeX = (minSize - Mathf.Abs(angleX * lengthSize)) * sizeFact;
            sizeY = (minSize - Mathf.Abs(angleY * lengthSize)) * sizeFact;
            
            uiMat.SetFloat("PosX", angleX);
            uiMat.SetFloat("PosY", angleY);
            uiMat.SetFloat("SizeX", sizeY);
            uiMat.SetFloat("SizeY", sizeX);

        }
    }

    public void ResetBall(){ incomingBall = null; }


    private void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        GUI.color = Color.black;
        guiStyle.fontSize = 25;
        GUILayout.Space(50);
        GUILayout.Label("x :" + angleX + " y : " + angleY, guiStyle);
    }
}
