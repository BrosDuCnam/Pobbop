using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private GameObject myCam;
    [SerializeField] private float sensX = 0.1f;
    [SerializeField] private float sensY = 0.1f;
    
    private Transform player;
    
    private Vector2 camAxis;
    private Vector2 currentLook;

    public override void OnStartAuthority()
    {
        enabled = true;
        player = GetComponent<Rigidbody>().transform;
        myCam.SetActive(true);
    }


    [ClientCallback]
    private void Update()
    {
        CalculateCam();
    }
    [ClientCallback]
    private void FixedUpdate()
    {
        RotatePlayerCam();
    }
    [Client]
    private void CalculateCam()
    {
        camAxis = new Vector2(camAxis.x * sensX, camAxis.y * sensY);

        currentLook.x += camAxis.x;
        currentLook.y = Mathf.Clamp(currentLook.y += camAxis.y, -90, 90);
    }
    [Client]
    private void RotatePlayerCam()
    {
        myCam.transform.localRotation = Quaternion.AngleAxis(-currentLook.y, Vector3.right);
        player.localRotation = Quaternion.Euler(0, currentLook.x, 0);
    }
    
    #region Input
    public void mouseAxis(InputAction.CallbackContext ctx)
    {
        camAxis = ctx.ReadValue<Vector2>();
    }
    #endregion

}
